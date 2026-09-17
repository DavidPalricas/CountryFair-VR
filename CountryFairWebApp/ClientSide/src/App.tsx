import { useEffect, useState } from "react";
import { getStateCallbacks } from "@colyseus/sdk";
import { getRoom } from "./network/client";
import { WaitingScreen, type MiniGameProgress } from "./screens/WatingScreen";
import { GameScreen } from "./screens/GameScreen";
import { classifyScene, MINI_GAME_LABELS } from "./screens/miniGameScene";

/** Status line shown while the survivor is on the hub's own dialogue (intro, session-completed
 * message, or a future per-mini-game cutscene) instead of looking at the fair — the tents on
 * screen would not reflect what the headset shows during that time, so the therapist gets
 * this instead of a stale/empty scene. Deliberately generic: which dialogue is playing does
 * not change what the therapist needs to do (wait), so the game only signals when one ends.
 */
const DIALOGUE_STATUS_TEXT = "O sobrevivente está a interagir com um diálogo do jogo";

/** Status line shown while `tutorialActive` is assumed true for the mini-game in `miniGame` — spells out that it's specifically the tutorial, not just which mini-game is loaded. */
const tutorialStatusText = (miniGame: string) => `O sobrevivente está a fazer o tutorial de ${miniGame}`;

/** Which top-level screen the fair state is in. Drives which overlay (if any) covers `GameScreen`. */
type Phase = "waiting" | "hub" | "miniGame";

const EMPTY_PROGRESS: MiniGameProgress = { score: 0, streak: 0, sessionGoal: 0 };

/**
 * Root of the companion web client.
 *
 * Joins the Colyseus `fairsceneroom` on mount and waits for the server's `"gamejoined"`
 * message, which the room only sends once the Unity headset has joined too — that is the
 * signal to hide the waiting overlay and reveal the fair scene underneath. If the headset
 * later drops (`CountryFairRoom.onLeave` broadcasts `"gameDisconnected"`), the client brings
 * the overlay back until the game rejoins and sends `"gamejoined"` again.
 *
 * The game shows its intro dialogue right after connecting, so `dialogueActive` starts `true`
 * on every `"gamejoined"` and this same overlay is kept up (now with `DIALOGUE_STATUS_TEXT`
 * instead of the connection message) until the game relays `"playerFinishedDialogue"` — there
 * is no matching "started" message; the game is the source of truth for when the *next*
 * dialogue begins and is expected to simply not send this until it ends.
 *
 * `"updateScene"` (`ConnectToWebApp.UpdateScene`, sent on every scene load) is what actually
 * drives `phase`: `classifyScene` tells the fair hub apart from the four mini-games. Moving
 * into a mini-game scene sets `phase` to `"miniGame"` and `miniGame` to its label (`MINI_GAME_
 * LABELS`) — there is nothing meaningful to render in the 3D view for a mini-game (it only
 * knows the fair hub), so the overlay stays up for as long as `phase` is `"miniGame"`. It
 * starts on the tutorial assumption (`tutorialActive`, mirroring `dialogueActive` in the hub —
 * the tutorial is a UIDialog too, just relayed through its own `"playerFinishedTutorial"`
 * message instead of `"playerFinishedDialogue"`) and switches to the live score/streak/
 * session-goal panel once that arrives. Moving back to the hub scene re-arms `dialogueActive`
 * (same assume-a-dialogue-is-up default as `"gamejoined"`), because a mini-game session that
 * just finished plays a congratulations dialogue there — `CountryFairDialogue` only knows
 * which case it is once its `Awake()` runs, after the scene (and this message) already
 * loaded. When nothing is going to play, `CountryFairDialogue` sends `"playerFinishedDialogue"`
 * immediately instead of a separate "no dialogue" message — from the web's perspective an
 * instantly-finished dialogue and no dialogue at all are the same thing: stop waiting now.
 *
 * `GameScreen` is mounted from the very first render instead of being swapped in on
 * `"gamejoined"`: its `<Canvas>` and every `useGLTF`/`useTexture` model start loading (and,
 * once loaded, sit parsed and GPU-uploaded) while the therapist is still looking at the
 * waiting screen, so there is no visible pop-in the moment the headset connects. It is kept
 * mounted (not just visually hidden by `display: none`) when hidden again — on disconnect or
 * while a mini-game overlay covers it — so a later reveal does not pay the loading cost again.
 */
function App() {
  const [phase, setPhase] = useState<Phase>("waiting");
  // True while the hub's own dialogue (intro/session-completed) is assumed to be up.
  // Irrelevant during "waiting" (the waiting overlay is already shown for a different reason)
  // or "miniGame" (see `tutorialActive` instead), so its value doesn't matter there.
  const [dialogueActive, setDialogueActive] = useState(true);
  // True while the current mini-game's tutorial is assumed to be up. Only meaningful once
  // `phase` is "miniGame" (see `dialogueActive` for the hub's own dialogues).
  const [tutorialActive, setTutorialActive] = useState(true);
  // Label of the mini-game currently loaded (e.g. "Arco e Flecha"), or null in the hub. Drives
  // the mini-game overlay's status text.
  const [miniGame, setMiniGame] = useState<string | null>(null);
  // Score/streak/session goal for the mini-game currently being played. Kept up to date via
  // the room's synced state (not a message) so a page reload mid-session shows the current
  // numbers immediately instead of the empty defaults until the next change.
  const [progress, setProgress] = useState<MiniGameProgress>(EMPTY_PROGRESS);

  // Matchmaking runs once. `cancelled` guards against the connection resolving after the
  // component is gone (StrictMode mounts twice in development).
  useEffect(() => {
    let cancelled = false;

    getRoom()
      .then((room) => {
        if (cancelled) return;
        console.log("Connected to room:", room.name);

        const $ = getStateCallbacks(room);
        $(room.state).listen("score", (score: number) => {
          setProgress((current) => ({ ...current, score }));
        });
        $(room.state).listen("streak", (streak: number) => {
          setProgress((current) => ({ ...current, streak }));
        });
        $(room.state).listen("sessionGoal", (sessionGoal: number) => {
          setProgress((current) => ({ ...current, sessionGoal }));
        });

        room.onMessage("gamejoined", () => {
          console.log("Received gamejoined message");
          setPhase("hub");
          // The game always shows its intro dialogue right after connecting.
          setDialogueActive(true);
        });
        room.onMessage("gameDisconnected", () => {
          console.log("Received gameDisconnected message");
          setPhase("waiting");
        });
        room.onMessage("playerFinishedDialogue", () => {
          console.log("Received playerFinishedDialogue message");
          setDialogueActive(false);
        });
        room.onMessage("playerFinishedTutorial", () => {
          console.log("Received playerFinishedTutorial message");
          setTutorialActive(false);
        });
        room.onMessage("updateScene", (data: { sceneName: string }) => {
          console.log("Received updateScene message:", data.sceneName);
          const scene = classifyScene(data.sceneName);

          if (scene === "hub") {
            setPhase("hub");
            setMiniGame(null);
            // A just-finished mini-game session may show a congratulations dialogue on
            // return; assume one is up until "playerFinishedDialogue" says otherwise (sent
            // immediately when there is nothing to show — see the note above).
            setDialogueActive(true);
            return;
          }

          setPhase("miniGame");
          setMiniGame(scene ? MINI_GAME_LABELS[scene] : data.sceneName);
          // The tutorial always shows right after entering a mini-game.
          setTutorialActive(true);
        });
      })
      .catch((err) => {
        if (!cancelled) console.error("Falha no matchmaking:", err);
      });

    // No leave() on cleanup: the room is a module-level singleton and is meant to survive
    // re-mounts. Leaving here would drop the seat the server reserved for this platform.
    return () => { cancelled = true };
  }, []);

  const showHubOverlay = phase === "waiting" || (phase === "hub" && dialogueActive);
  const showTutorialOverlay = phase === "miniGame" && tutorialActive;
  const showProgressOverlay = phase === "miniGame" && !tutorialActive;

  return (
    <div className="app-root">
      <GameScreen />
      {showHubOverlay && (
        <WaitingScreen statusText={phase === "waiting" ? undefined : DIALOGUE_STATUS_TEXT} />
      )}
      {showTutorialOverlay && (
        <WaitingScreen character="carnyWise" statusText={miniGame ? tutorialStatusText(miniGame) : undefined} />
      )}
      {showProgressOverlay && (
        <WaitingScreen character="carnyWise" statusText={miniGame ?? undefined} progress={progress} />
      )}
    </div>
  );
}

export default App;
