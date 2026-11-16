import { memo } from "react";
import { Html } from "@react-three/drei";
import {
    ARM_LABELS,
    DIFFICULTY_CHANGES,
    DIFFICULTY_LABELS,
    REPETITIONS_RANGE,
    SESSION_TIME_RANGE,
    stepValue,
    type ClampedRange,
    type DifficultyChange,
    type TentParams,
} from "./sessionParams";
import "./TentParamsPanel.css";

type TentParamsPanelProps = {
    position: [number, number, number];
    params: TentParams;
    isLeftArmAffected: boolean;
    onParamsChange: (params: TentParams) => void;
    onArmChange: (isLeftArmAffected: boolean) => void;
};

/**
 * Web version of Unity's `SessionParamsMenu`: the session parameters panel floating above a
 * tent. Purely controlled — the values live in `GameScreen`, which also sends them to the room,
 * just as the tent order does.
 *
 * Drawn as DOM through drei's `<Html>` rather than as 3D text: `-`/`+` buttons and dropdowns
 * come for free, with real touch targets on the therapist's tablet. Being DOM on top of the
 * canvas, its clicks never reach `useTentDrag`'s canvas listeners, so using the panel never
 * starts dragging the tent under it.
 */
export const TentParamsPanel = memo(function TentParamsPanel({ position, params, isLeftArmAffected, onParamsChange, onArmChange }: TentParamsPanelProps) {
    return (
        <Html
            position={position}
            center
            /* Scales with the distance to the camera, so the panel keeps the tent's width on
               any screen size instead of a fixed pixel size. */
            distanceFactor={12}
            /* drei's default z-index is in the millions, which would put the panels above the
               overlays and the cheat-code panel (z-index 10 / 20). */
            zIndexRange={[5, 0]}
        >
            <div className="tent-params">
                <ClampedParam
                    label="Número de repetições"
                    value={params.repetitions}
                    range={REPETITIONS_RANGE}
                    onChange={(repetitions) => onParamsChange({ ...params, repetitions })}
                />

                <label className="tent-params__row">
                    <span className="tent-params__label">Braço afetado</span>
                    <select
                        className="tent-params__select"
                        value={isLeftArmAffected ? "left" : "right"}
                        onChange={(event) => onArmChange(event.target.value === "left")}
                    >
                        <option value="left">{ARM_LABELS.left}</option>
                        <option value="right">{ARM_LABELS.right}</option>
                    </select>
                </label>

                <ClampedParam
                    label="Tempo de sessão (minutos)"
                    value={params.sessionTimeMinutes}
                    range={SESSION_TIME_RANGE}
                    onChange={(sessionTimeMinutes) => onParamsChange({ ...params, sessionTimeMinutes })}
                />

                <label className="tent-params__row">
                    <span className="tent-params__label">Dificuldade do jogo</span>
                    <select
                        className="tent-params__select"
                        value={params.difficultyChange}
                        onChange={(event) => onParamsChange({ ...params, difficultyChange: event.target.value as DifficultyChange })}
                    >
                        {DIFFICULTY_CHANGES.map((change) => (
                            <option key={change} value={change}>{DIFFICULTY_LABELS[change]}</option>
                        ))}
                    </select>
                </label>
            </div>
        </Html>
    );
});

type ClampedParamProps = {
    label: string;
    value: number;
    range: ClampedRange;
    onChange: (value: number) => void;
};

/** `-` value `+`, with the button that would leave the range disabled (Unity's `SessionClampedParameter.Set`). */
function ClampedParam({ label, value, range, onChange }: ClampedParamProps) {
    return (
        <div className="tent-params__row">
            <span className="tent-params__label">{label}</span>
            <div className="tent-params__stepper">
                <button
                    type="button"
                    className="tent-params__button"
                    disabled={value <= range.min}
                    onClick={() => onChange(stepValue(value, range, -1))}
                    aria-label={`Diminuir ${label}`}
                >
                    -
                </button>
                <span className="tent-params__value">{value}</span>
                <button
                    type="button"
                    className="tent-params__button"
                    disabled={value >= range.max}
                    onClick={() => onChange(stepValue(value, range, 1))}
                    aria-label={`Aumentar ${label}`}
                >
                    +
                </button>
            </div>
        </div>
    );
}
