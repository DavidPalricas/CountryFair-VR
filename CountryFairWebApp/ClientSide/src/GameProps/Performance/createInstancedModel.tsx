import { createContext, useContext, useLayoutEffect, useMemo, useRef, type ReactNode } from "react";
import { useFrame, type ThreeElements } from "@react-three/fiber";
import { useGLTF } from "@react-three/drei";
import { Group, InstancedMesh, Matrix4, Mesh, MeshBasicMaterial, type Object3D, type Scene } from "three";
import { toLowCostMaterial } from "./lowCostMaterials";

/** One mesh of the model, drawn for every copy at once, plus where it sits inside the model. */
type Part = { mesh: InstancedMesh; local: Matrix4 };

type Registry = {
    /** Empty groups placed where each copy goes; their world matrices drive the instances. */
    anchors: Set<Object3D>;
    parts: Part[];
    /** Builds an invisible copy of the model that only exists to be hit by pointer raycasts. */
    createHitProxy: () => Object3D;
};

// visible: false on the material (not the object) keeps the mesh out of the render list
// while three's raycaster still tests it.
const HIDDEN_MATERIAL = new MeshBasicMaterial({ visible: false });

const instanceMatrix = new Matrix4();
const providerInverse = new Matrix4();

/** True when the object and every ancestor up to the scene are visible. */
function isRendered(object: Object3D) {
    for (let node: Object3D | null = object; node !== null; node = node.parent) {
        if (!node.visible) {
            return false;
        }

        if ((node as Scene).isScene) {
            return true;
        }
    }

    // Detached from the scene (mid-unmount).
    return false;
}

type InstanceProps = ThreeElements["group"] & {
    /**
     * Keeps pointer events working on this copy. The instanced meshes live at the provider,
     * outside the copy's own group, so an R3F handler on an ancestor (the draggable tent)
     * would no longer be reached by clicks on the model; the invisible proxy restores that.
     */
    hittable?: boolean;
};

/**
 * Turns a GLTF model that appears several times in the scene into one draw call per mesh for
 * all of its copies, instead of one per mesh per copy (BigTop x6: 24 -> 4 draw calls).
 *
 * Returns a `Provider`, rendered once near the scene root around everything that uses the
 * model, and an `Instance`, used wherever a copy goes exactly as the `<primitive>` of a
 * clone was. Instances can sit inside moving or hidden groups: every frame the provider reads
 * each anchor's world matrix and skips anchors with an invisible ancestor, so dragging a tent
 * or hiding it during a drag behaves as before.
 *
 * Materials are converted with `toLowCostMaterial`, like the non-instanced models.
 * `capacity` is the maximum number of simultaneous copies (unused slots cost no draw time).
 */
export function createInstancedModel(url: string, capacity: number) {
    const RegistryContext = createContext<Registry | null>(null);

    function Provider({ children }: { children: ReactNode }) {
        const { scene } = useGLTF(url);
        const root = useRef<Group>(null);
        const warned = useRef(false);

        const registry = useMemo<Registry>(() => {
            scene.updateMatrixWorld(true);
            const sceneInverse = new Matrix4().copy(scene.matrixWorld).invert();
            const parts: Part[] = [];

            scene.traverse((node) => {
                if (!(node instanceof Mesh)) {
                    return;
                }

                const mesh = new InstancedMesh(node.geometry, toLowCostMaterial(node.material), capacity);
                mesh.name = `${node.name} (instanced)`;
                mesh.count = 0;
                // The bounding sphere of an InstancedMesh is not recomputed as instances move,
                // so culling it could drop copies that are on screen. A handful of copies is
                // cheaper to just draw.
                mesh.frustumCulled = false;

                parts.push({ mesh, local: new Matrix4().multiplyMatrices(sceneInverse, node.matrixWorld) });
            });

            const createHitProxy = () => {
                const proxy = scene.clone();

                proxy.traverse((node) => {
                    if (node instanceof Mesh) {
                        node.material = HIDDEN_MATERIAL;
                    }
                });

                return proxy;
            };

            return { anchors: new Set(), parts, createHitProxy };
        }, [scene]);

        useFrame(() => {
            const group = root.current;

            if (group === null) {
                return;
            }

            // Instance matrices are in the provider's space, so its own transform (identity at
            // the scene root) is undone first.
            group.updateWorldMatrix(true, false);
            providerInverse.copy(group.matrixWorld).invert();

            let count = 0;

            for (const anchor of registry.anchors) {
                if (count === capacity) {
                    if (!warned.current) {
                        console.warn(`createInstancedModel: mais de ${capacity} copias de ${url}; as restantes nao sao desenhadas.`);
                        warned.current = true;
                    }
                    break;
                }

                if (!isRendered(anchor)) {
                    continue;
                }

                // Brings the matrix up to date with this frame's useFrame movements (drag,
                // hop) instead of using the one from the previous render.
                anchor.updateWorldMatrix(true, false);

                for (const part of registry.parts) {
                    instanceMatrix.multiplyMatrices(providerInverse, anchor.matrixWorld).multiply(part.local);
                    part.mesh.setMatrixAt(count, instanceMatrix);
                }

                count++;
            }

            for (const part of registry.parts) {
                part.mesh.count = count;
                part.mesh.instanceMatrix.needsUpdate = true;
            }
        });

        return (
            <RegistryContext.Provider value={registry}>
                <group ref={root}>
                    {registry.parts.map((part, index) => (
                        <primitive key={index} object={part.mesh} />
                    ))}
                </group>
                {children}
            </RegistryContext.Provider>
        );
    }

    function Instance({ hittable = false, ...groupProps }: InstanceProps) {
        const registry = useContext(RegistryContext);

        if (registry === null) {
            throw new Error(`<Instance> de ${url} tem de estar dentro do respetivo Provider.`);
        }

        const anchor = useRef<Group>(null);
        const hitProxy = useMemo(() => (hittable ? registry.createHitProxy() : null), [registry, hittable]);

        useLayoutEffect(() => {
            const group = anchor.current;

            if (group === null) {
                return;
            }

            registry.anchors.add(group);
            return () => { registry.anchors.delete(group); };
        }, [registry]);

        return (
            <group ref={anchor} {...groupProps}>
                {hitProxy !== null && <primitive object={hitProxy} />}
            </group>
        );
    }

    return { Provider, Instance };
}
