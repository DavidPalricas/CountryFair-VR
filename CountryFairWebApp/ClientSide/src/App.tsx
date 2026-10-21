import { useFairRoom } from "./network/useFairRoom";
import { overlayFor } from "./screens/fairPhase";
import { WaitingScreen } from "./screens/WatingScreen";
import { GameScreen } from "./screens/GameScreen";
import { CheatCodePanel } from "./components/CheatCodePanel";

/**
 * Root of the companion web client. Only renders: what the room says lives in `useFairRoom`,
 * and the rules for which overlay that means live in `fairPhase.ts` (`fairReducer`/`overlayFor`).
 *
 * `GameScreen` is mounted from the very first render instead of being swapped in on
 * `"gamejoined"`: its `<Canvas>` and every `useGLTF`/`useTexture` model start loading (and,
 * once loaded, sit parsed and GPU-uploaded) while the therapist is still looking at the
 * waiting screen, so there is no visible pop-in the moment the headset connects. It is kept
 * mounted (not just visually hidden by `display: none`) when hidden again — on disconnect or
 * while a mini-game overlay covers it — so a later reveal does not pay the loading cost again.
 */
function App() {
  const fair = useFairRoom();
  const overlay = overlayFor(fair, fair.connectionStatus);

  return (
    <div className="app-root">
      <GameScreen active={overlay === null} />
      <CheatCodePanel disabled={fair.connectionStatus !== "connected" || fair.phase === "waiting"} />
      {overlay && (
        <WaitingScreen
          key={overlay.kind}
          statusText={overlay.statusText}
          character={overlay.character}
          title={overlay.title}
          progress={overlay.progress}
        />
      )}
    </div>
  );
}

export default App;
