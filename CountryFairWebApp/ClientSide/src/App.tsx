import { useEffect, useState } from "react";
import { getRoom } from "./network/client";
import { WaitingScreen } from "./screens/WatingScreen";
import { GameScreen } from "./screens/GameScreen";

/**
 * Status line shown while the survivor is on a game dialogue (intro, session-completed
 * message, or a future per-mini-game cutscene) instead of looking at the fair — the tents on
 * screen would not reflect what the headset shows during that time, so the therapist gets
 * this instead of a stale/empty scene. Deliberately generic: which dialogue is playing does
 * not change what the therapist needs to do (wait), so the game only signals when one ends.
 */
const DIALOGUE_STATUS_TEXT = "O sobrevivente está a interagir com um diálogo do jogo";

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
 * `GameScreen` is mounted from the very first render instead of being swapped in on
 * `"gamejoined"`: its `<Canvas>` and every `useGLTF`/`useTexture` model start loading (and,
 * once loaded, sit parsed and GPU-uploaded) while the therapist is still looking at the
 * waiting screen, so there is no visible pop-in the moment the headset connects. It is kept
 * mounted (not just visually hidden by `display: none`) when hidden again on disconnect, so
 * a later reconnect does not pay the loading cost a second time.
 */
function App() {
  const [phase, setPhase] = useState<"waiting" | "game">("waiting");
  // True while a game dialogue is assumed to be up. Irrelevant during "waiting" (the waiting
  // overlay is already shown for a different reason), so its initial value here doesn't matter.
  const [dialogueActive, setDialogueActive] = useState(true);

  // Matchmaking runs once. `cancelled` guards against the connection resolving after the
  // component is gone (StrictMode mounts twice in development).
  useEffect(() => {
    let cancelled = false;

    getRoom()
      .then((room) => {
        if (cancelled) return;
        console.log("Connected to room:", room.name);
        room.onMessage("gamejoined", () => {
          console.log("Received gamejoined message");
          setPhase("game");
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
      })
      .catch((err) => {
        if (!cancelled) console.error("Falha no matchmaking:", err);
      });

    // No leave() on cleanup: the room is a module-level singleton and is meant to survive
    // re-mounts. Leaving here would drop the seat the server reserved for this platform.
    return () => { cancelled = true };
  }, []);

  return (
    <div className="app-root">
      <GameScreen />
      {(phase === "waiting" || dialogueActive) && (
        <WaitingScreen statusText={phase === "waiting" ? undefined : DIALOGUE_STATUS_TEXT} />
      )}
    </div>
  );
}

export default App;
