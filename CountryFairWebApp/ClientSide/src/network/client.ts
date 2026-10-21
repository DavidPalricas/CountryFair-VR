import { Client } from "@colyseus/sdk";
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

/**
 * The single in-flight/settled connection. Kept at module scope so React re-mounts
 * (StrictMode in development) reuse it instead of asking the server for a second seat —
 * the room only accepts one client per platform and would reject the duplicate.
 */
let roomPromise: Promise<Room> | null = null;

async function connect(): Promise<Room> {
  const client = new Client(SERVER_URL);
  // `platform: "web"` is what the room uses to tell this client apart from the Unity headset.
  return client.joinOrCreate("fairsceneroom", { platform: "web" });
}

/**
 * Returns the shared room connection, opening it on first call.
 *
 * A failed connection clears the cache so a later call can retry; a successful one is kept
 * for the lifetime of the page.
 */
export function getRoom(): Promise<Room> {
  if (!roomPromise) {
    roomPromise = connect().catch((err) => {
      roomPromise = null;
      throw err;
    });
  }
  return roomPromise;
}
