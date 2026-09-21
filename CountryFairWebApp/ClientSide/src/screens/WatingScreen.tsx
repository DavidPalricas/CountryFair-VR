import zecaImg from '../assets/imgs/Zeca.png'
import carnyWiseImg from '../assets/imgs/CarnyWise.png'
import '../App.css'

/** Default text: shown while the headset has not joined the room yet. */
const DEFAULT_STATUS_TEXT = "Esperando que ligue ao jogo";

/** Default signboard title, used everywhere except the mini-game overlays (see `title` below). */
const DEFAULT_TITLE = "Bem vindo ao Country Fair VR";

/** Which host character illustrates the overlay. Zeca hosts the fair hub, Carny Wise hosts everything mini-game-related (tutorial, live progress) — matching who greets the patient on each screen in the headset. */
type Character = "zeca" | "carnyWise";

const CHARACTER_IMAGES: Record<Character, string> = {
    zeca: zecaImg,
    carnyWise: carnyWiseImg,
};

const CHARACTER_ALT: Record<Character, string> = {
    zeca: "Zeca Bigodes",
    carnyWise: "Carny Wise",
};

/** Live mini-game numbers shown in place of the waiting dots (see `progress` below). */
export type MiniGameProgress = {
    score: number;
    streak: number;
    sessionGoal: number;
};

type WaitingScreenProps = {
    /**
     * Overrides the status line under the signboard title. Lets `App` reuse this same overlay
     * (host character + wooden signboard + pulsing dots) for other "nothing to do on this
     * screen right now" moments, such as the survivor being on a game dialogue, without
     * duplicating markup.
     */
    statusText?: string;
    /** Host character shown next to the status text/progress. Defaults to Zeca (fair hub). */
    character?: Character;
    /**
     * Overrides the signboard title, e.g. "Mini Jogo: Frisbee" — lets the mini-game overlay
     * name which game is loaded in place of the generic hub greeting.
     */
    title?: string;
    /**
     * When set, replaces the waiting dots with the current mini-game score/streak/session
     * goal — this is how the overlay doubles as the "player is playing" screen instead of
     * navigating to a separate page (and paying its model-loading cost) just to show numbers.
     */
    progress?: MiniGameProgress;
};

/**
 * Overlay shown whenever the fair scene has nothing else to show the therapist right now.
 *
 * Purely presentational — `App` decides when it is visible, which `statusText`/`character` to
 * pass, and whether to pass `progress`. Zeca Bigodes and Carny Wise are the same host
 * characters that appear in the VR game itself, so the patient and therapist see a consistent
 * guide on both devices: Zeca for the fair hub, Carny Wise for anything mini-game-related.
 */
export function WaitingScreen({ statusText = DEFAULT_STATUS_TEXT, character = "zeca", title = DEFAULT_TITLE, progress }: WaitingScreenProps) {
   return (
     <div className="App">
        <div className="info">
            <div className="signboard">
            <h1 className="signboard__title">{title}</h1>

            <div className="signboard__body">
                <img src={CHARACTER_IMAGES[character]} alt={CHARACTER_ALT[character]} />

                <div className="signboard__status">
                <h1>{statusText}</h1>

                {progress ? (
                    <div className="minigame-progress">
                        <div className="minigame-progress__row">
                            <span className="minigame-progress__label">Pontuação</span>
                            <span className="minigame-progress__value">{progress.score}</span>
                        </div>
                        <div className="minigame-progress__row">
                            <span className="minigame-progress__label">Sequência</span>
                            <span className="minigame-progress__value">{progress.streak}</span>
                        </div>
                        <div className="minigame-progress__row">
                            <span className="minigame-progress__label">Objetivo da sessão</span>
                            <span className="minigame-progress__value">{progress.sessionGoal}</span>
                        </div>
                    </div>
                ) : (
                    <div className="waiting-dots" aria-hidden="true">
                        <span />
                        <span />
                        <span />
                    </div>
                )}
                </div>
            </div>
            </div>
        </div>
    </div>
   )
}
