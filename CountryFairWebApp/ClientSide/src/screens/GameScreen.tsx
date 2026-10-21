import { useCallback, useEffect, useRef, useState } from "react";
import { Canvas } from "@react-three/fiber";
import { Vector3 } from "three";
import { Plane } from "../GameProps/Plane";
import {GameLight} from "../GameProps/GameLight";
import { MiniGameTent } from "../GameProps/Tents/MiniGameTent";
import { TentPlaceHolder } from "../GameProps/Tents/TentPlaceHolder";
import { TENT_ROW_Z, TENT_SLOTS, TENT_Y, VIEWER_POSITION, type MiniGameType } from "../GameProps/Tents/tentSlots";
import { orderFromFairState, slotIndexAtX, swapIntoSlot, type TentOrder } from "../GameProps/Tents/tentOrder";
import { getRoom } from "../network/client";
import { BigTop, BigTopInstances } from "../GameProps/BigTop";
import { TentInstances } from "../GameProps/Tents/Tent";
import { TentRibbonInstances } from "../GameProps/Tents/TentRibbon";
import { DuckInstances } from "../GameProps/MiniGamesProps/DuckProps";
import {FerrisWheel} from "../GameProps/FerrisWheel";
import{Camera} from "../GameProps/Camera";
import { Text3D } from "../GameProps/Text3D";
import { DebugOverlay, DebugProbe } from "../components/DebugOverlay";
import "./GameScreen.css";

/** Left-to-right tent order the scene opens with; index is the slot index. */
const INITIAL_ORDER: TentOrder = ["fishing", "archery", "frisbee", "duckgame"];

/**
 * Decorative big tops filling the fair grounds between the ferris wheel (z = -10) and the
 * mini-game row (z = 9) — without them the middle of the screen was nothing but grass.
 * There are only four playable tents, and `BigTop` clones share geometry and materials with
 * the already-loaded model, so each of these costs a draw call rather than a new model.
 *
 * The ferris wheel takes up more ground than it looks: rotated 90 degrees and scaled to 0.4
 * it spans x from -10 to 10 and z from -13 to -4.5, and no tent may enter that box. Hence
 * the two rear ones sit beyond |x| = 12 and the rest stand in front of it, leaving the
 * central corridor clear.
 *
 * Scales run from 0.8 to nearly 2 (the BigTop base height is 4 metres) and the tallest is
 * also the nearest: that size difference is what gives depth to otherwise flat terrain.
 *
 * `position` is [x, z] in world units — y comes from TENT_Y; `rotationY` is in radians.
 */
const BIG_TOPS = [
    { position: [-15.4, 0.0], rotationY: 0.5, scale: 1.2 },
    { position: [15.0, -10.4], rotationY: -0.45, scale: 1.1 },
    { position: [-12.5, -3.0], rotationY: 0.75, scale: 1.9 },
    { position: [15.4, -1.0], rotationY: -1.0, scale: 1.8 },
    { position: [-15.0, 5.0], rotationY: 1.1, scale: 1.85 },
    { position: [15, 6.0], rotationY: -0.9, scale: 1.95 },
] as const;

/*
  Test switches for GPU-limited devices (tablets), set through the page URL, e.g.
  http://192.168.137.1:5173/?debug&dpr=1&noaa — `dpr` caps the render resolution and `noaa`
  turns antialiasing off. Both default to the usual behaviour when absent.
*/
const URL_PARAMS = new URLSearchParams(location.search);
const MAX_DPR = Number(URL_PARAMS.get("dpr")) || undefined;
const ANTIALIAS = !URL_PARAMS.has("noaa");

/** Footer shown in the bottom-right corner. */
const COPYRIGHT = "© 2026 David Palricas";

/**
 * The fair scene: the therapist reorders the mini-game tents by dragging them along the row.
 *
 * This screen plays the role of Unity's `PlaceHolderManager` — it owns the tent order and
 * knows which tent is being dragged. Unity's `Dictionary<element, placeholder>` is just the
 * `order` array here, where the array index *is* the slot index. It also plays the role of
 * `ConnectToWebApp.UpdateTentsOrder()`: every time the order changes (and once on mount, like
 * `PlaceHolderManager.Start()`), it sends the same `"updateTentsOrder"` message to the room.
 *
 * The reverse direction mirrors `PlaceHolderManager.OnOtherManagerUpdate()`: the same
 * `"updateTentsOrder"` message, broadcast by the server when the patient reorders the tents in
 * the headset, is applied here to keep this screen's `order` in sync.
 *
 * `active` is false while an overlay (waiting, dialogue, mini-game) covers the scene, which is
 * most of a session. The canvas then only renders on demand — when something changes, such as
 * a tent order arriving from the headset — instead of 60 times a second behind the overlay.
 * That idle rendering kept the tablet hot, and a hot tablet throttles its CPU/GPU, so the scene
 * was already slower by the time the therapist got to it.
 */
export function GameScreen({ active }: { active: boolean }) {
    const [order, setOrder] = useState<TentOrder>(INITIAL_ORDER);
    const [dragging, setDragging] = useState<MiniGameType | null>(null);

    // Shared between the grabbed tent and the placeholder rings; written on every
    // pointermove, so it lives in a ref rather than in state.
    const dragPoint = useRef(new Vector3());

    // Equivalent of Unity's `PlaceHolderManager.GetFairState()`: tent id -> 1-based slot
    // number, matching `TentSlot.number` and the shape the game sends via `ConnectToWebApp`.
    const sendFairState = useCallback((currentOrder: TentOrder) => {
        const fairState: Record<string, string> = {};

        currentOrder.forEach((type, slot) => {
            fairState[type] = String(slot + 1);
        });

        getRoom()
            .then((room) => room.send("updateTentsOrder", fairState))
            .catch((err) => console.error("Falha ao enviar a ordem das tendas:", err));
    }, []);

    // Mirrors Unity's `PlaceHolderManager.Start()`, which reports the opening order too.
    useEffect(() => {
        sendFairState(order);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // Mirrors Unity's `PlaceHolderManager.OnOtherManagerUpdate()`: applies the order the
    // headset reports whenever the patient drags a tent in the world instead of the menu.
    // This message is never our own `sendFairState` echoing back — the room broadcasts with
    // `{ except: client }`, so the sender never receives its own update.
    useEffect(() => {
        let cancelled = false;

        getRoom()
            .then((room) => {
                if (cancelled) return;

                room.onMessage("updateTentsOrder", (fairState: Record<string, string>) => {
                    setOrder((current) => orderFromFairState(fairState, current));
                });
            })
            .catch((err) => console.error("Falha ao receber a ordem das tendas:", err));

        return () => { cancelled = true };
    }, []);

    const handleDragEnd = useCallback((type: MiniGameType, x: number) => {
        const targetSlot = slotIndexAtX(x);

        // null = dropped in the dead zone between slots; the tent's useFrame walks it back to
        // its original slot on its own, because the order did not change.
        if (targetSlot !== null) {
            setOrder((current) => {
                const next = swapIntoSlot(current, type, targetSlot);

                if (next !== current) {
                    sendFairState(next);
                }

                return next;
            });
        }

        setDragging(null);
    }, [sendFairState]);

    return (
        <div className="game-screen">

            <Canvas
                dpr={MAX_DPR ? [1, MAX_DPR] : undefined}
                gl={{ antialias: ANTIALIAS }}
                /* "demand" rather than "never": the hidden scene still renders once when it
                   changes, so models get uploaded and shaders compiled behind the waiting
                   screen, and the order stays current for when it is revealed. */
                frameloop={active ? "always" : "demand"}
            >
                <DebugProbe />
                {/* Looks at the centre of the row so all four tents are framed the same way. */}
                {/* minAspect: the narrowest landscape screen tested (the 2000x1200 tablet) still frames
                    the whole row; on anything narrower, such as a phone held upright, the
                    camera backs away so the four tents keep fitting side by side. */}
                <Camera position={VIEWER_POSITION} lookAt={[0, 1.4, TENT_ROW_Z]} minAspect={1.6} />

                <Text3D position={[0, 4.8, TENT_ROW_Z]} fontSize={0.55} maxWidth={10}>
                    Clique e arraste nas tendas para trocar a ordem delas
                </Text3D>
                {/* Hemisphere (blue sky / green grass) instead of flat ambient light: the colour
                    bounce makes the green and red of the tents far livelier than a white
                    ambientLight managed. */}
                <hemisphereLight args={["#87ceeb", "#4f8f3a", 0.75]} />
                {/* Midday sun with a golden touch, between cold white and full golden-hour yellow. */}
                <GameLight position={[10, 14, 8]} color="#ffedb8" intensity={2.6} />
                {/* Cool fill from the opposite side so the shadows do not read as dead. */}
                <GameLight position={[-12, 6, -6]} color="#bcdfff" intensity={0.5} />

                <Plane position={[0, 0, 0]} size={80} texture="/textures/grass.jpg" textureRepeat={20} />
                <FerrisWheel position={[0, 0, -10]} rotation={[0, Math.PI / 2, 0]} scale={0.4} animationSpeed={0.5} />

                {/* Models repeated across the scene are drawn as GPU instances (one draw call
                    per mesh for every copy); each provider must wrap every use of its model. */}
                <BigTopInstances>
                    {BIG_TOPS.map(({ position, rotationY, scale }) => (
                        <BigTop
                            key={`${position[0]},${position[1]}`}
                            position={[position[0], TENT_Y, position[1]]}
                            rotation={[0, rotationY, 0]}
                            scale={scale}
                        />
                    ))}
                </BigTopInstances>

                <TentInstances>
                    <TentRibbonInstances>
                        <DuckInstances>
                            {order.map((type, slot) => (
                                <MiniGameTent
                                    key={type}
                                    type={type}
                                    slot={slot}
                                    isDragging={dragging === type}
                                    hidden={dragging !== null && dragging !== type}
                                    dragPoint={dragPoint}
                                    onDragStart={setDragging}
                                    onDragEnd={handleDragEnd}
                                />
                            ))}
                        </DuckInstances>
                    </TentRibbonInstances>
                </TentInstances>

                {dragging !== null && TENT_SLOTS.map((slot) => (
                    <TentPlaceHolder
                        key={slot.number}
                        position={slot.position}
                        number={slot.number}
                        dragPoint={dragPoint}
                    />
                ))}

            </Canvas>

            <DebugOverlay />
            <small className="game-screen__copyright">{COPYRIGHT}</small>
        </div>
    );
}
