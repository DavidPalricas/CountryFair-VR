import { useSyncExternalStore } from "react";
import { Client, CloseCode } from "@colyseus/sdk";
import type { Room } from "@colyseus/sdk/Room";

/** Must match SERVER_PORT in CountryFairWebApp/.env, which the Colyseus server listens on. */
const SERVER_PORT = import.meta.env.VITE_SERVER_PORT ?? "2567";

/**
 * The Colyseus server runs on the same PC that serves this page (the Mobile Hotspot host),
 * so the page's own hostname is the server's address: `localhost` in a browser on that PC,
 * the hotspot IP (e.g. `192.168.137.1`, the same one the Unity build uses) on a therapist's
 * tablet joined to the hotspot. VITE_SERVER_URL overrides it if the server ever moves.
 */
const SERVER_URL =
  import.meta.env.VITE_SERVER_URL ?? `${location.protocol}//${location.hostname}:${SERVER_PORT}`;

/** Wait between fresh join attempts, mirroring Unity's `ConnectToWebApp.retryDelaySeconds`. */
const RETRY_DELAY_MS = 3000;

/**
 * - `connecting`: no room yet (first load, or a fresh join after the previous room was lost);
 * - `connected`: the room is open;
 * - `reconnecting`: the socket dropped and the SDK is resuming the *same* session (same
 *   `sessionId`, state and handlers) while the server holds this platform's slot (W-S7);
 * - `disconnected`: the room is gone for good; a fresh join is scheduled.
 */
export type ConnectionStatus = "connecting" | "connected" | "reconnecting" | "disconnected";

/** Registers handlers on a room and returns what undoes them. See `subscribeRoom`. */
type RoomSetup = (room: Room) => (() => void) | void;

/**
 * The single in-flight/settled connection. Kept at module scope so React re-mounts
 * (StrictMode in development) reuse it instead of asking the server for a second seat —
 * the room only accepts one client per platform and would reject the duplicate.
 */
let roomPromise: Promise<Room> | null = null;
let currentRoom: Room | null = null;
let retryTimer: ReturnType<typeof setTimeout> | null = null;

let status: ConnectionStatus = "connecting";
const statusListeners = new Set<() => void>();

// Every subscribed setup, mapped to the cleanup it returned for `currentRoom` (if any).
const setups = new Map<RoomSetup, (() => void) | void>();

function setStatus(next: ConnectionStatus) {
  status = next;
  statusListeners.forEach((listener) => listener());
}

/** Schedules a fresh join, unless one is already pending. Retries forever, like Unity's `ConnectLoop()`. */
function scheduleRetry() {
  if (retryTimer !== null) return;

  retryTimer = setTimeout(() => {
    retryTimer = null;
    getRoom().catch(() => { /* already logged; `getRoom` scheduled the next attempt */ });
  }, RETRY_DELAY_MS);
}

/** Called once the room is lost for good: drops it so the next `getRoom()` opens a new one. */
function handleRoomLost(code: number) {
  console.warn(`Ligação à sala perdida (código ${code}). Nova tentativa em ${RETRY_DELAY_MS} ms.`);

  setups.forEach((cleanup, setup) => {
    cleanup?.();
    setups.set(setup, undefined);
  });

  currentRoom = null;
  roomPromise = null;
  setStatus("disconnected");
  scheduleRetry();
}

async function connect(): Promise<Room> {
  const client = new Client(SERVER_URL);
  // `platform: "web"` is what the room uses to tell this client apart from the Unity headset.
  const room = await client.joinOrCreate("fairsceneroom", { platform: "web" });

  // The SDK resumes a dropped socket on its own (same Room object, so every `onMessage` and
  // state listener survives). These only mirror its progress into `status`.
  room.onDrop(() => setStatus("reconnecting"));
  room.onReconnect(() => setStatus("connected"));

  // Fired when the room is lost for good: the server closed it, or the SDK gave up resuming
  // (e.g. the server's reconnection window expired). Without this the cached promise would
  // keep handing out a dead room and the UI would freeze in its last phase.
  room.onLeave((code) => {
    if (code !== CloseCode.CONSENTED) handleRoomLost(code);
  });

  // Run synchronously, before this promise settles: the server sends `"gamejoined"` right
  // after the join, and a handler registered later (e.g. in a React effect) could miss it.
  currentRoom = room;
  setups.forEach((_, setup) => setups.set(setup, setup(room)));

  return room;
}

/**
 * Returns the shared room connection, opening it on first call.
 *
 * A failed connection clears the cache and schedules another attempt; a successful one is
 * kept until the room is lost for good (see `connect`), after which a new one is opened.
 * Internal: components send with `sendToRoom` and listen with `subscribeRoom`.
 */
function getRoom(): Promise<Room> {
  if (!roomPromise) {
    setStatus("connecting");

    roomPromise = connect()
      .then((room) => {
        setStatus("connected");
        return room;
      })
      .catch((err) => {
        console.error("Falha ao ligar à sala:", err);
        roomPromise = null;
        setStatus("disconnected");
        scheduleRetry();
        throw err;
      });
  }
  return roomPromise;
}

/**
 * Runs `setup` on the current room, if any, and again on every room that replaces a lost
 * one, so its `onMessage`/state listeners never end up on a dead room. `setup` may return a
 * cleanup, run when that room is lost or when the returned unsubscribe is called.
 *
 * Opens the connection if none exists yet. Meant to be returned from a `useEffect`.
 */
export function subscribeRoom(setup: RoomSetup): () => void {
  setups.set(setup, currentRoom ? setup(currentRoom) : undefined);

  if (!roomPromise && retryTimer === null) {
    getRoom().catch(() => { /* already logged; a retry is scheduled */ });
  }

  return () => {
    setups.get(setup)?.();
    setups.delete(setup);
  };
}

/**
 * Sends a message once the room is available. While the SDK is resuming a dropped socket the
 * message is queued and flushed on reconnect; if the room is lost for good it goes to the next one.
 */
export function sendToRoom(type: string, payload?: unknown) {
  getRoom()
    .then((room) => room.send(type, payload))
    .catch((err) => console.error(`Falha ao enviar "${type}":`, err));
}

function subscribeStatus(listener: () => void) {
  statusListeners.add(listener);
  return () => { statusListeners.delete(listener) };
}

/** Current `ConnectionStatus`, re-rendering the caller whenever it changes. */
export function useConnectionStatus(): ConnectionStatus {
  return useSyncExternalStore(subscribeStatus, () => status);
}
