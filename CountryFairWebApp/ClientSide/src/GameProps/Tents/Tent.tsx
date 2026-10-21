import { useGLTF } from "@react-three/drei";
import { createInstancedModel } from "../Performance/createInstancedModel";
import{TentBg} from "./TentBg";
import { FairWorker } from "../FairWorker";


/** Expects public/models/Tent.glb. */
const TENT_MODEL = "/models/Tent.glb";

// The model was exported at ~790 units tall (obj2gltf), so it has to be brought down to the
// metre scale the rest of the scene works in.
const MODEL_SCALE = 0.002;

/* The four mini-game tents are drawn as instances: 2 draw calls for all of them. */
const TentModel = createInstancedModel(TENT_MODEL, 8);

/** Must wrap every <Tent> in the scene (see GameScreen). */
export const TentInstances = TentModel.Provider;

type TentProps = {
    /** World-space position of the tent base. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Uniform multiplier applied on top of MODEL_SCALE. */
    scale?: number;
};

/**
 * The tent shell itself: the GLTF model, an opaque backdrop that closes off the interior,
 * and a fair worker standing beside it. Purely visual — the drag behaviour lives in
 * `MiniGameTent`, which wraps this.
 */
export function Tent({ position = [0, 0, 0], rotation = [0, 0, 0], scale = 1 }: TentProps) {
    return (
        <group position={position} rotation={rotation} scale={scale}>
            {/* hittable: the tent body is what the therapist grabs to start a drag. */}
            <TentModel.Instance hittable scale={MODEL_SCALE} />
            <TentBg position={[0, 0.5, 0]} dimensions={[1.4, 1, 1.3]} />
            <FairWorker position={[-1, 0, 1]} rotation={[0, 0, 0]} scale={0.35} />
        </group>
    );
}


useGLTF.preload(TENT_MODEL);
