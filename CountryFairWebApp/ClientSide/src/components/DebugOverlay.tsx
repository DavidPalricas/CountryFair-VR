import { useEffect, useRef, useState } from "react";
import { useFrame, useThree } from "@react-three/fiber";

/** True when the page was opened with `?debug`, e.g. http://192.168.137.1:5173/?debug */
export const DEBUG = new URLSearchParams(location.search).has("debug");

/** Lines shared between the in-canvas probe and the DOM panel (no React state: written from event handlers). */
const listeners = new Set<(line: string) => void>();

function log(line: string) {
    const stamp = new Date().toLocaleTimeString();
    listeners.forEach((listener) => listener(`${stamp} ${line}`));
}

/**
 * Lives inside the <Canvas>: reports WebGL context loss/restoration and the GPU resource
 * counts, which is what is needed to tell "the GPU gave up" apart from "the scene threw".
 */
export function DebugProbe() {
    const gl = useThree((state) => state.gl);

    useEffect(() => {
        if (!DEBUG) {
            return;
        }

        const canvas = gl.domElement;
        const context = gl.getContext();
        const debugInfo = context.getExtension("WEBGL_debug_renderer_info");

        log(`GPU: ${debugInfo ? context.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) : "unknown"}`);
        log(`DPR ${window.devicePixelRatio} · canvas ${canvas.width}x${canvas.height} · maxTex ${context.getParameter(context.MAX_TEXTURE_SIZE)}`);

        const report = (label: string) => log(`${label} · geo ${gl.info.memory.geometries} tex ${gl.info.memory.textures} calls ${gl.info.render.calls}`);

        const onLost = (event: Event) => { event.preventDefault(); report("WEBGL CONTEXT LOST"); };
        const onRestored = () => report("webgl context restored");
        // The S Pen behaves differently from a finger (it hovers, and may cancel the gesture),
        // so every pointer event in the gesture is logged with its type and coordinates.
        const describe = (event: PointerEvent) =>
            `${event.type} ${event.pointerType} #${event.pointerId} (${event.clientX.toFixed(0)},${event.clientY.toFixed(0)}) btn ${event.buttons}`;
        const onPointer = (event: PointerEvent) => report(describe(event));
        const pointerTypes = ["pointerdown", "pointerup", "pointercancel", "lostpointercapture"] as const;

        canvas.addEventListener("webglcontextlost", onLost);
        canvas.addEventListener("webglcontextrestored", onRestored);
        pointerTypes.forEach((type) => canvas.addEventListener(type, onPointer));
        report("canvas ready");

        return () => {
            canvas.removeEventListener("webglcontextlost", onLost);
            canvas.removeEventListener("webglcontextrestored", onRestored);
            pointerTypes.forEach((type) => canvas.removeEventListener(type, onPointer));
        };
    }, [gl]);

    // A NaN in the camera or a zero-sized canvas also blanks the whole scene without any
    // error, so both are checked every frame and reported once each time they start.
    const camera = useThree((state) => state.camera);
    const size = useThree((state) => state.size);
    const broken = useRef(false);

    useFrame(() => {
        if (!DEBUG) {
            return;
        }

        const elements = [...camera.matrixWorld.elements, ...camera.projectionMatrix.elements];
        const isBroken = elements.some((value) => !Number.isFinite(value)) || size.width === 0 || size.height === 0;

        if (isBroken && !broken.current) {
            const p = camera.position;
            log(`CAMERA BROKEN pos (${p.x.toFixed(1)},${p.y.toFixed(1)},${p.z.toFixed(1)}) size ${size.width}x${size.height}`);
        }

        broken.current = isBroken;
    });

    return null;
}

/** Temporary on-screen log (only with `?debug`) so a tablet can be diagnosed without USB debugging. */
export function DebugOverlay() {
    const [lines, setLines] = useState<string[]>([]);

    useEffect(() => {
        if (!DEBUG) {
            return;
        }

        const push = (line: string) => setLines((current) => [...current.slice(-14), line]);
        const onError = (event: ErrorEvent) => log(`ERROR ${event.message}`);
        const onRejection = (event: PromiseRejectionEvent) => log(`REJECTION ${String(event.reason)}`);

        listeners.add(push);
        window.addEventListener("error", onError);
        window.addEventListener("unhandledrejection", onRejection);

        // Console errors/warnings (three.js reports shader and texture failures this way).
        const originalError = console.error;
        const originalWarn = console.warn;
        console.error = (...args) => { log(`console.error ${args.map(String).join(" ").slice(0, 160)}`); originalError(...args); };
        console.warn = (...args) => { log(`console.warn ${args.map(String).join(" ").slice(0, 160)}`); originalWarn(...args); };

        return () => {
            listeners.delete(push);
            window.removeEventListener("error", onError);
            window.removeEventListener("unhandledrejection", onRejection);
            console.error = originalError;
            console.warn = originalWarn;
        };
    }, []);

    if (!DEBUG) {
        return null;
    }

    return (
        <pre style={{
            position: "fixed", top: 0, left: 0, zIndex: 9999, margin: 0, padding: "4px 6px",
            maxWidth: "100%", background: "rgba(0,0,0,.75)", color: "#7CFC00",
            font: "11px/1.3 monospace", whiteSpace: "pre-wrap", pointerEvents: "none",
        }}>
            {lines.join("\n")}
        </pre>
    );
}
