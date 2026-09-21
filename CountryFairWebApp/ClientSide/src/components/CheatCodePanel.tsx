import { useState, type FormEvent } from "react";
import { getRoom } from "../network/client";
import "./CheatCodePanel.css";

type CheatCodePanelProps = {
    /**
     * True while there is no game client connected to the room yet (the initial waiting
     * screen), so a cheat code would have nowhere to go. The panel stays mounted (keeping its
     * `code` state) but is hidden and non-interactive instead of being unmounted.
     */
    disabled?: boolean;
};

/**
 * Floating panel that lets the therapist send a cheat code from the web app, mirroring what
 * typing the same code on the headset's keyboard does.
 *
 * Sends a `"cheatCode"` message to the room (`FairSceneRoom.ts` relays it to the game client
 * only), which Unity's `ConnectToWebApp` turns into `receiveCheatCode`, consumed by
 * `CheatCodes.OnWebCheatCode()` — the exact same `CheckCheatCode()` verification and gating
 * (tutorial/intro requirements, etc.) used for keyboard-entered codes.
 *
 * The input field is always visible (no toggle button to reveal it first) so it does not
 * compete with the fair scene for attention; it is not tied to `phase` in `App.tsx` beyond
 * `disabled` because a cheat code can be relevant in the hub, a mini-game's tutorial or a
 * mini-game session alike.
 */
export function CheatCodePanel({ disabled = false }: CheatCodePanelProps) {
    const [code, setCode] = useState("");

    const sendCode = (event: FormEvent) => {
        event.preventDefault();

        if (disabled) {
            return;
        }

        const trimmed = code.trim();

        if (!trimmed) {
            return;
        }

        getRoom()
            .then((room) => room.send("cheatCode", trimmed))
            .catch((err) => console.error("Falha ao enviar o cheat code:", err));

        setCode("");
    };

    return (
        <div className={`cheat-panel${disabled ? " cheat-panel--disabled" : ""}`} aria-hidden={disabled}>
            <form className="cheat-panel__form" onSubmit={sendCode}>
                <input
                    type="text"
                    className="cheat-panel__input"
                    placeholder="cheat code"
                    value={code}
                    onChange={(event) => setCode(event.target.value)}
                    autoComplete="off"
                    spellCheck={false}
                    disabled={disabled}
                    tabIndex={disabled ? -1 : undefined}
                />
                <button type="submit" className="cheat-panel__send" disabled={disabled} tabIndex={disabled ? -1 : undefined}>
                    Enviar
                </button>
            </form>
        </div>
    );
}
