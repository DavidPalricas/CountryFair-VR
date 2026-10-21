import { useMemo } from "react";
import { useFrame } from "@react-three/fiber";
import { useGLTF } from "@react-three/drei";
import { InstancedMesh, Matrix4, Mesh, PropertyBinding, type Object3D } from "three";
import { withLowCostMaterials } from "./Performance/lowCostMaterials";

/** Expects public/models/FerrisWheel.glb. Its baked rotation clips are no longer used (see below). */
const Ferris_WHEEL_MODEL = "/models/FerrisWheel.glb";

const MODEL_SCALE = 1;

/*
  Node names as exported from Blender. GLTFLoader runs them through sanitizeNodeName, which
  drops the dots ("SM_FerrisWheel_02.001" -> "SM_FerrisWheel_02001"), so the conversion is
  left to three, as in FrisbeeProps.
*/
const WHEEL_NODE = PropertyBinding.sanitizeNodeName("SM_FerrisWheel_02.001");

/*
  Exact duplicates of the wheel (same geometry, transform and colours), each carrying one
  extra cabin that sits on top of SM_FerrisWheel_02.014 — left over in the Blender file. They
  added 8 draw calls of geometry drawn over itself, with no visible effect.
*/
const DUPLICATE_NODES = ["SM_FerrisWheel_02.019", "SM_FerrisWheel_02.021"].map(PropertyBinding.sanitizeNodeName);

/*
  The .glb clips turn the wheel once around its local X every 10 seconds (positive angle) and
  every cabin the opposite way, so they stay upright. Reproduced here in code instead of
  running 21 AnimationMixer tracks.
*/
const SECONDS_PER_TURN = 10;

type FerrisWheelProps = {
    /** World-space position of the wheel base. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
    /** Multiplier on the turning speed; below 1 the wheel turns slowly, as a real one does. */
    animationSpeed?: number;
};

type WheelParts = {
    root: Object3D;
    wheel: Object3D;
    /** Empty transforms where each cabin hangs; their meshes were moved into `cabinMeshes`. */
    cabins: Object3D[];
    /** One InstancedMesh per cabin primitive, drawing all cabins at once. */
    cabinMeshes: InstancedMesh[];
};

/**
 * Splits the cloned model into what this component animates: the wheel node and its cabins.
 *
 * The 16 cabins share the same geometry (to within 0.00002), so they are drawn as instances —
 * 2 draw calls for all of them instead of 32. Each cabin node is kept as an empty transform
 * (the hinge on the rim) and its meshes are removed; the instance matrices are rebuilt every
 * frame from wheel × cabin.
 */
function buildWheel(scene: Object3D): WheelParts {
    const root = withLowCostMaterials(scene.clone());

    for (const name of DUPLICATE_NODES) {
        root.getObjectByName(name)?.removeFromParent();
    }

    const wheel = root.getObjectByName(WHEEL_NODE);

    if (wheel === undefined) {
        throw new Error(`FerrisWheel: no "${WHEEL_NODE}" em ${Ferris_WHEEL_MODEL}.`);
    }

    // The wheel's own primitives are Mesh children of the wheel node; every other child is a
    // cabin (a Group holding one Mesh per primitive).
    const cabins = wheel.children.filter((child) => !(child instanceof Mesh));
    const template = cabins[0].children.filter((child): child is Mesh => child instanceof Mesh);

    const cabinMeshes = template.map((mesh) => {
        const instanced = new InstancedMesh(mesh.geometry, mesh.material, cabins.length);
        instanced.name = `${mesh.name} (cabins)`;
        // The geometry's bounding sphere covers one cabin, not the whole rim.
        instanced.frustumCulled = false;
        return instanced;
    });

    for (const cabin of cabins) {
        cabin.clear();
    }

    // In the root's space, like the wheel itself, so instance matrix = wheel × cabin.
    root.add(...cabinMeshes);

    return { root, wheel, cabins, cabinMeshes };
}

const cabinMatrix = new Matrix4();

/**
 * The fair's landmark at the back of the scene, turning continuously.
 *
 * Note it takes up a large footprint on the ground — see the BIG_TOPS layout notes in
 * `GameScreen`, where no tent is allowed to enter the box it occupies.
 */
export function FerrisWheel({
    position,
    rotation = [0, 0, 0],
    scale = 1,
    animationSpeed = 1,
}: FerrisWheelProps) {
    const { scene } = useGLTF(Ferris_WHEEL_MODEL);
    const parts = useMemo(() => buildWheel(scene), [scene]);

    useFrame(({ clock }) => {
        const angle = (clock.elapsedTime * animationSpeed / SECONDS_PER_TURN) * Math.PI * 2;

        parts.wheel.rotation.x = angle;
        parts.wheel.updateMatrix();

        parts.cabins.forEach((cabin, index) => {
            // Counter-rotation: the cabin hangs upright whatever the wheel's angle.
            cabin.rotation.x = -angle;
            cabin.updateMatrix();

            cabinMatrix.multiplyMatrices(parts.wheel.matrix, cabin.matrix);

            for (const mesh of parts.cabinMeshes) {
                mesh.setMatrixAt(index, cabinMatrix);
            }
        });

        for (const mesh of parts.cabinMeshes) {
            mesh.instanceMatrix.needsUpdate = true;
        }
    });

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={parts.root} scale={MODEL_SCALE} />
        </group>
    );
}

useGLTF.preload(Ferris_WHEEL_MODEL);
