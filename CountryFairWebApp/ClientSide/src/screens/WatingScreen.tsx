import zecaImg from '../assets/imgs/Zeca.png'
import '../App.css'

/** Default text: shown while the headset has not joined the room yet. */
const DEFAULT_STATUS_TEXT = "Esperando que ligue ao jogo";

type WaitingScreenProps = {
    /**
     * Overrides the status line under the signboard title. Lets `App` reuse this same overlay
     * (Zeca + wooden signboard + pulsing dots) for other "nothing to do on this screen right
     * now" moments, such as the survivor being on a game dialogue, without duplicating markup.
     */
    statusText?: string;
};

/**
 * Overlay shown whenever the fair scene has nothing to show the therapist right now.
 *
 * Purely presentational — `App` decides when it is visible and which `statusText` to pass.
 * Zeca Bigodes is the same host character that gives the intro dialogue inside the VR game,
 * so the patient and therapist see a consistent guide on both devices.
 */
export function WaitingScreen({ statusText = DEFAULT_STATUS_TEXT }: WaitingScreenProps) {
   return (
     <div className="App">
        <div className="info">
            <div className="signboard">
            <h1 className="signboard__title">Bem vindo ao Country Fair VR</h1>

            <div className="signboard__body">
                <img src={zecaImg} alt="ZecaBigodes" />

                <div className="signboard__status">
                <h1>{statusText}</h1>

                <div className="waiting-dots" aria-hidden="true">
                    <span />
                    <span />
                    <span />
                </div>
                </div>
            </div>
            </div>
        </div>
    </div>
   )
}
