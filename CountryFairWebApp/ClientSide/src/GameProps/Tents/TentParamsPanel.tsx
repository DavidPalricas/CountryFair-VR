import { memo, useRef } from "react";
import { Html } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { PerspectiveCamera, Vector3, type Group } from "three";
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
 * Width the panel takes in the world, below the 3.4-unit tent spacing so neighbours never
 * overlap. Wider in portrait, where the tents are tiny and every bit of width counts.
 */
const PANEL_WORLD_WIDTH = 2.3;
const PANEL_WORLD_WIDTH_PORTRAIT = 3.1;
/**
 * `.tent-params` width in CSS px (TentParamsPanel.css), the size the panel is laid out at
 * before zoom. Portrait uses the narrower `.tent-params--portrait` layout: with the same font
 * sizes in fewer px, the text ends up larger once the panel is zoomed to its world width. The
 * labels wrap onto more lines instead, which is fine: a phone held upright has plenty of sky.
 */
const PANEL_CSS_WIDTH = 178;
const PANEL_CSS_WIDTH_PORTRAIT = 112;

const panelWorldPos = new Vector3();
const cameraWorldPos = new Vector3();

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
    const anchorRef = useRef<Group>(null!);
    const panelRef = useRef<HTMLDivElement>(null);
    const lastZoom = useRef(0);

    /*
      Sized by hand instead of drei's `distanceFactor`, for two reasons:
      - drei's scale only depends on the camera distance, not on the viewport height. On a phone
        held upright the camera backs away so the four tents still fit, the tents shrink a lot,
        but the panels barely do: they ended up huge and on top of each other.
      - drei applies it as `transform: scale()`, which blurs the text. CSS `zoom` lays the panel
        out again at the new size, so text stays crisp.
      Pixels per world unit at the panel's distance = viewport height / visible world height.
    */
    useFrame(({ camera, size }) => {
        const panel = panelRef.current;
        if (!panel || !(camera instanceof PerspectiveCamera)) return;

        anchorRef.current.getWorldPosition(panelWorldPos);
        camera.getWorldPosition(cameraWorldPos);
        const visibleHeight = 2 * Math.tan((camera.fov * Math.PI) / 360) * panelWorldPos.distanceTo(cameraWorldPos);
        const pixelsPerUnit = size.height / visibleHeight;

        const isPortrait = size.width < size.height;
        panel.classList.toggle("tent-params--portrait", isPortrait);
        const zoom = isPortrait
            ? (PANEL_WORLD_WIDTH_PORTRAIT * pixelsPerUnit) / PANEL_CSS_WIDTH_PORTRAIT
            : (PANEL_WORLD_WIDTH * pixelsPerUnit) / PANEL_CSS_WIDTH;

        // Only touch the DOM when it changes visibly: a zoom write re-lays out the panel.
        if (Math.abs(zoom - lastZoom.current) > 0.005) {
            lastZoom.current = zoom;
            panel.style.zoom = String(zoom);
        }
    });

    return (
        <group ref={anchorRef} position={position}>
            {/* Not `center`: `position` is the panel's bottom edge (see .tent-params), so a taller
                panel grows up into the sky instead of down over the tent's name. */}
            <Html
                /* drei's default z-index is in the millions, which would put the panels above the
                   overlays and the cheat-code panel (z-index 10 / 20). */
                zIndexRange={[5, 0]}
            >
                <div className="tent-params" ref={panelRef}>
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
                        label="Número de repetições"
                        value={params.repetitions}
                        range={REPETITIONS_RANGE}
                        onChange={(repetitions) => onParamsChange({ ...params, repetitions })}
                    />

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
        </group>
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
