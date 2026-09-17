import { useState, type FormEvent } from "react";
import { getRoom } from "../network/client";
import "./CheatCodePanel.css";

/**
 * Floating panel that lets the therapist send a cheat code from the web app, mirroring what
 * typing the same code on the headset's keyboard does.
 *
 * Sends a `"cheatCode"` message to the room (`FairSceneRoom.ts` relays it to the game client
 * only), which Unity's `ConnectToWebApp` turns into `receiveCheatCode`, consumed by
 * `CheatCodes.OnWebCheatCode()` — the exact same `CheckCheatCode()` verification and gating
 * (tutorial/intro requirements, etc.) used for keyboard-entered codes.
 *
 * Collapsed by default (a small toggle button) so it does not compete with the fair scene for
 * attention; it is not tied to `phase` in `App.tsx` because a cheat code can be relevant in the
 * hub, a mini-game's tutorial or a mini-game session alike.
 */
export function CheatCodePanel() {
    const [code, setCode] = useState("");
    const [open, setOpen] = useState(false);

    const sendCode = (event: FormEvent) => {
        event.preventDefault();

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
        <div className="cheat-panel">
            <button
                type="button"
                className="cheat-panel__toggle"
                onClick={() => setOpen((current) => !current)}
                aria-expanded={open}
                aria-label={open ? "Fechar painel de cheat codes" : "Abrir painel de cheat codes"}
            >
                {open ? "×" : "⚙"}
            </button>

            {open && (
                <form className="cheat-panel__form" onSubmit={sendCode}>
                    <input
                        type="text"
                        className="cheat-panel__input"
                        placeholder="cheat code"
                        value={code}
                        onChange={(event) => setCode(event.target.value)}
                        autoComplete="off"
                        spellCheck={false}
                    />
                    <button type="submit" className="cheat-panel__send">Enviar</button>
                </form>
            )}
        </div>
    );
}
