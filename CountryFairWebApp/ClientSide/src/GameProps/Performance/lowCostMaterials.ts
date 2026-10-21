import { Mesh, MeshLambertMaterial, MeshStandardMaterial, type Material, type Object3D } from "three";

/**
 * GLTFLoader turns every glTF material into a MeshStandardMaterial (PBR), or a
 * MeshPhysicalMaterial when the file carries extensions such as KHR_materials_transmission.
 * PBR is the most expensive material per pixel, and transmission (all 13 FerrisWheel
 * materials have transmissionFactor 1) makes three render the opaque scene a second time,
 * into a render target, on every frame. On the therapist's tablet (Adreno 650, ~2M pixels)
 * that was the bulk of the frame time.
 *
 * Every model here is low-poly with flat colours, so Lambert (diffuse only) looks almost the
 * same at a fraction of the cost. Metallic sheen, specular highlights and the see-through
 * effect of transmission are lost — none of them read at the size the props are on screen.
 *
 * One Lambert per source material, cached: clones share materials with the cached GLTF, so
 * they keep sharing the converted ones and three compiles each shader only once.
 */
const converted = new WeakMap<Material, Material>();

function convert(material: Material): Material {
    // MeshPhysicalMaterial extends MeshStandardMaterial, so this covers both.
    if (!(material instanceof MeshStandardMaterial)) {
        return material;
    }

    let lambert = converted.get(material);

    if (lambert === undefined) {
        lambert = new MeshLambertMaterial({
            name: material.name,
            color: material.color,
            map: material.map,
            emissive: material.emissive,
            emissiveMap: material.emissiveMap,
            emissiveIntensity: material.emissiveIntensity,
            normalMap: material.normalMap,
            normalScale: material.normalScale,
            aoMap: material.aoMap,
            aoMapIntensity: material.aoMapIntensity,
            alphaMap: material.alphaMap,
            transparent: material.transparent,
            opacity: material.opacity,
            alphaTest: material.alphaTest,
            side: material.side,
            vertexColors: material.vertexColors,
            depthWrite: material.depthWrite,
            flatShading: material.flatShading,
        });
        converted.set(material, lambert);
    }

    return lambert;
}

/** Lambert counterpart of a mesh's material (or material array); non-PBR materials pass through. */
export function toLowCostMaterial(material: Material | Material[]): Material | Material[] {
    return Array.isArray(material) ? material.map(convert) : convert(material);
}

/** Swaps every PBR material under `root` for its Lambert counterpart, in place; returns `root`. */
export function withLowCostMaterials<T extends Object3D>(root: T): T {
    root.traverse((node) => {
        if (node instanceof Mesh) {
            node.material = toLowCostMaterial(node.material);
        }
    });

    return root;
}
