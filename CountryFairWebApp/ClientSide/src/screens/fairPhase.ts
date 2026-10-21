import type { ConnectionStatus } from "../network/client";
import type { MiniGameProgress } from "./WatingScreen";
import { classifyScene, MINI_GAME_LABELS } from "./miniGameScene";

/** Status line shown while the survivor is on the hub's own dialogue (intro, session-completed
 * message, or a future per-mini-game cutscene) instead of looking at the fair — the tents on
 * screen would not reflect what the headset shows during that time, so the therapist gets
 * this instead of a stale/empty scene. Deliberately generic: which dialogue is playing does
 * not change what the therapist needs to do (wait), so the game only signals when one ends.
 */
const DIALOGUE_STATUS_TEXT = "O sobrevivente está a interagir com um diálogo do jogo";

/** Status line shown while `tutorialActive` is assumed true — the mini-game name itself lives
 * in the overlay's title ("Mini Jogo: X"), so this stays generic. */
const TUTORIAL_STATUS_TEXT = "O sobrevivente está a fazer o tutorial";

/** Status line shown above the live score/streak/session-goal panel, once the tutorial ends
 * — the mini-game name itself lives in the overlay's title, so this just names what the
 * numbers below it are. */
const PROGRESS_STATUS_TEXT = "Progresso da sessão";

/** Status line shown while the server connection itself is down (W-C2). `connecting` keeps the
 * waiting screen's default text, so the very first load looks exactly as before. */
const CONNECTION_STATUS_TEXT: Record<Exclude<ConnectionStatus, "connected">, string | undefined> = {
  connecting: undefined,
  reconnecting: "Ligação ao servidor perdida. A tentar restabelecer…",
  disconnected: "Sem ligação ao servidor. A tentar novamente…",
};

/** Signboard title for the mini-game overlays, naming which game is loaded. */
const miniGameTitle = (miniGame: string) => `Mini Jogo: ${miniGame}`;

/** Which top-level screen the fair state is in. Drives which overlay (if any) covers `GameScreen`. */
export type Phase = "waiting" | "hub" | "miniGame";

export type FairPhaseState = {
  phase: Phase;
  // True while the hub's own dialogue (intro/session-completed) is assumed to be up.
  // Irrelevant during "waiting" (the waiting overlay is already shown for a different reason)
  // or "miniGame" (see `tutorialActive` instead), so its value doesn't matter there.
  dialogueActive: boolean;
  // True while the current mini-game's tutorial is assumed to be up. Only meaningful once
  // `phase` is "miniGame" (see `dialogueActive` for the hub's own dialogues).
  tutorialActive: boolean;
  // Label of the mini-game currently loaded (e.g. "Arco e Flecha"), or null in the hub. Drives
  // the mini-game overlay's status text.
  miniGame: string | null;
  // Score/streak/session goal for the mini-game currently being played. Kept up to date via
  // the room's synced state (not a message) so a page reload mid-session shows the current
  // numbers immediately instead of the empty defaults until the next change.
  progress: MiniGameProgress;
};

/** One action per thing the room can tell us; `useFairRoom` translates messages into these. */
export type FairAction =
  | { type: "roomJoined" }
  | { type: "gameJoined"; snapshot?: SceneSnapshot }
  | { type: "gameDisconnected" }
  | { type: "dialogueFinished" }
  | { type: "tutorialFinished" }
  | { type: "sceneChanged"; sceneName: string }
  | { type: "progressChanged"; progress: Partial<MiniGameProgress> };

/** The room's `sceneName` / `dialogueActive` / `tutorialActive` (see `FairState`) at the moment of joining. */
export type SceneSnapshot = {
  sceneName: string;
  dialogueActive: boolean;
  tutorialActive: boolean;
};

export const initialFairPhase: FairPhaseState = {
  phase: "waiting",
  dialogueActive: true,
  tutorialActive: true,
  miniGame: null,
  progress: { score: 0, streak: 0, sessionGoal: 0 },
};

/**
 * The web client's view of where the survivor is, as a pure function of the room's messages.
 *
 * `"gamejoined"` (sent once both platforms are in the room) moves to the hub. The game shows
 * its intro dialogue right after connecting, so `dialogueActive` starts `true` there and stays
 * so until `"playerFinishedDialogue"` — there is no matching "started" message; the game is the
 * source of truth for when the *next* dialogue begins and simply does not send this until it
 * ends. If the headset drops (`CountryFairRoom.onLeave` broadcasts `"gameDisconnected"`), the
 * phase goes back to "waiting" until it rejoins.
 *
 * `"updateScene"` (`ConnectToWebApp.UpdateScene`, sent on every scene load) is what actually
 * drives `phase`: `classifyScene` tells the fair hub apart from the four mini-games. Entering a
 * mini-game starts on the tutorial assumption (`tutorialActive`, mirroring `dialogueActive` in
 * the hub — the tutorial is a UIDialog too, just relayed through its own
 * `"playerFinishedTutorial"` message) and switches to the live progress panel once that
 * arrives. Moving back to the hub re-arms `dialogueActive`, because a mini-game session that
 * just finished plays a congratulations dialogue there — `CountryFairDialogue` only knows which
 * case it is once its `Awake()` runs, after the scene (and this message) already loaded. When
 * nothing is going to play, it sends `"playerFinishedDialogue"` immediately instead of a
 * separate "no dialogue" message — from the web's perspective an instantly-finished dialogue
 * and no dialogue at all are the same thing: stop waiting now.
 */
export function fairReducer(state: FairPhaseState, action: FairAction): FairPhaseState {
  switch (action.type) {
    case "roomJoined":
      // A fresh seat knows nothing about the game yet: wait for "gamejoined" again.
      return { ...state, phase: "waiting" };
    case "gameJoined": {
      const { snapshot } = action;
      const scene = snapshot?.sceneName ? classifyScene(snapshot.sceneName) : "hub";

      // No scene reported yet (the game just connected): assume the intro, as before.
      if (!snapshot || !snapshot.sceneName || scene === "hub") {
        return { ...state, phase: "hub", miniGame: null, dialogueActive: snapshot?.sceneName ? snapshot.dialogueActive : true };
      }

      // Joined (or the game reconnected) mid-mini-game: resume from what the room remembers.
      return {
        ...state,
        phase: "miniGame",
        miniGame: scene ? MINI_GAME_LABELS[scene] : snapshot.sceneName,
        tutorialActive: snapshot.tutorialActive,
      };
    }
    case "gameDisconnected":
      return { ...state, phase: "waiting" };
    case "dialogueFinished":
      return { ...state, dialogueActive: false };
    case "tutorialFinished":
      return { ...state, tutorialActive: false };
    case "sceneChanged": {
      const scene = classifyScene(action.sceneName);

      if (scene === "hub") {
        return { ...state, phase: "hub", miniGame: null, dialogueActive: true };
      }

      return {
        ...state,
        phase: "miniGame",
        miniGame: scene ? MINI_GAME_LABELS[scene] : action.sceneName,
        tutorialActive: true,
      };
    }
    case "progressChanged":
      return { ...state, progress: { ...state.progress, ...action.progress } };
  }
}

/** What covers `GameScreen`, as `WaitingScreen` props; `null` means the fair scene is visible. */
export type Overlay = {
  /** Distinct per overlay so React remounts `WaitingScreen` when one replaces another. */
  kind: "hub" | "tutorial" | "progress";
  statusText?: string;
  character?: "zeca" | "carnyWise";
  title?: string;
  progress?: MiniGameProgress;
};

/**
 * Picks the overlay for the current phase. While the server connection itself is down,
 * nothing on screen can be trusted to be current, so the hub overlay covers everything,
 * whatever `phase` was.
 */
export function overlayFor(state: FairPhaseState, connectionStatus: ConnectionStatus): Overlay | null {
  if (connectionStatus !== "connected") {
    return { kind: "hub", statusText: CONNECTION_STATUS_TEXT[connectionStatus] };
  }

  switch (state.phase) {
    case "waiting":
      return { kind: "hub" };
    case "hub":
      return state.dialogueActive ? { kind: "hub", statusText: DIALOGUE_STATUS_TEXT } : null;
    case "miniGame": {
      const title = state.miniGame ? miniGameTitle(state.miniGame) : undefined;

      return state.tutorialActive
        ? { kind: "tutorial", character: "carnyWise", statusText: TUTORIAL_STATUS_TEXT, title }
        : { kind: "progress", character: "carnyWise", statusText: PROGRESS_STATUS_TEXT, title, progress: state.progress };
    }
  }
}
