import { useState } from "react";
import "./ConfigCard.css";

/** Survivor ids with an image in `public/survivors/`; mirrors `SURVIVOR_FILES` on the server. */
const SURVIVOR_NAMES: Record<string, string> = {
    rutepereira: "Rute Pereira",
    joanasantos: "Joana Santos",
    tiagobastos: "Tiago Bastos",
};

type ConfigCardProps = {
    /** Lower-cased survivor id reported by the game (`state.survivorFile`); `""` before it reports one. */
    survivor: string;
    /** The tents match the survivor's file (`state.configCompleted`). */
    completed: boolean;
    /** This is the last survivor of the test: the button ends the configuration instead of advancing (`state.isLastConfig`). */
    isLast: boolean;
    onNext: () => void;
};

/**
 * What the therapist needs next to the tents in the configuration scene, drawn on the grass in the
 * middle of the screen: the survivor's clinical file while the tents are being configured, and,
 * once they match it, the button that moves on to the next configuration in its place.
 *
 * Both come from the game (`ConfigSessionManager` reports them, the server keeps them in
 * `FairState`), so this is only a view: the file is the same image shown on the headset's clipboard.
 * The file is a landscape page sized to the strip of grass; it opens enlarged when clicked, for when
 * the text is still too small (a tablet).
 */
export function ConfigCard({ survivor, completed, isLast, onNext }: ConfigCardProps) {
    const [enlarged, setEnlarged] = useState(false);
    const name = SURVIVOR_NAMES[survivor];

    // Unknown or not reported yet: nothing to show (the id is also a file name, so it is checked).
    if (name === undefined) {
        return null;
    }

    if (completed) {
        return (
            <div className="config-card config-card--centered">
                <button type="button" className="config-card__next" onClick={onNext}>
                    {isLast ? "Terminar configuração de pacientes" : "Avançar para a próxima configuração"}
                </button>
            </div>
        );
    }

    const src = `/survivors/${survivor}.png`;

    return (
        <>
            <div className="config-card">
                <button
                    type="button"
                    className="config-card__file"
                    onClick={() => setEnlarged(true)}
                    aria-label={`Ampliar o ficheiro de ${name}`}
                >
                    <img src={src} alt={`Ficheiro do sobrevivente ${name}`} draggable={false} />
                </button>
            </div>

            {enlarged && (
                <div className="config-card__backdrop" onClick={() => setEnlarged(false)}>
                    <img className="config-card__enlarged" src={src} alt={`Ficheiro do sobrevivente ${name}`} />
                </div>
            )}
        </>
    );
}
