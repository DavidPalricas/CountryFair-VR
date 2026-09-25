import { useEffect, useMemo } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { Group, PropertyBinding, Quaternion, Vector3 } from "three";
import { clone as skeletonClone } from "three/examples/jsm/utils/SkeletonUtils.js";

/** Expects public/models/miniGamesProps/Frisbee/Dog.glb, with the idle clip below. */
const DOG_MODEL = "/models/miniGamesProps/Frisbee/Dog.glb";

/** Expects public/models/miniGamesProps/Frisbee/Frisbee.glb. */
const FRISBEE_MODEL = "/models/miniGamesProps/Frisbee/Frisbee.glb";

/* Clip name exactly as it comes out of the .glb — the typo ("Animatiom") is in the file
   exported from Blender, not here. The other clips are JumpAnimation, WalkAnimation and the
   ZZ_BACKUP_* copies of the original animations. */
const DOG_IDLE_CLIP = "IdleAnimatiom";

/*
  The head bone — the Unity project's "Head", where the frisbee is parented to "End", a child
  of Head. End (a leaf bone, weighted to no vertex) is not exported to the .glb, so we follow
  the head instead, which gives the same motion: End is rigid inside Head and has no
  animation channels of its own. Older exports called this bone "Leg.006"; it was renamed to
  "Head" (now with Jaw, Ear.L/R and Eye.L/R as children) when the dog animations were redone.

  The sanitizeNodeName call is not decorative: GLTFLoader runs every name through it and it
  eats the dots, so a name like "Leg.006" becomes "Leg006" in the loaded scene. We keep the
  Blender name here and let three do the conversion.
*/
const DOG_HEAD_BONE = PropertyBinding.sanitizeNodeName("Head");

/*
  The frisbee's transform in Head space, copied from the Unity DogFrisbeeTent prefab
  (Head → End → Frisbee) so the web dog holds it exactly like the game one. Conversion from
  Unity to this .glb:
    - Unity's bone space is 100x smaller than the .glb's (Head sits at y = 0.0355 in Unity and
      y = 3.552 here), so positions are multiplied by 100;
    - Unity is left-handed: x is negated in positions and the quaternion becomes (x, -y, -z, w);
    - End is not exported (see above), so its offset (0, 2.832, 0) is folded into the position.
  The scale is Unity's localScale as is: Unity imports the FBX vertices in centimetres (x0.01),
  which cancels out with the x100 between the two bone spaces. It is non-uniform — the Unity
  frisbee was flattened by hand — and applies to the mesh in its Blender axes (disc axis
  along Z), see FRISBEE_MESH_TO_BLENDER.
*/
const FRISBEE_IN_HEAD = {
    position: new Vector3(-3.3826344, 1.6673964 + 2.8319359, -2.4903363),
    quaternion: new Quaternion(0.29818416, 0.12103002, 0.32128957, -0.8906239),
    scale: new Vector3(0.47773552, 0.47773546, 0.03071158),
};

/* glTF exports the mesh Y-up; this turns it back to Blender's Z-up, the axes Unity's scale and
   rotation were authored against. */
const FRISBEE_MESH_TO_BLENDER = Math.PI / 2;


type FrisbeeProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Non-uniform scale, unlike the other mini-game props. */
    scale?: [number, number, number];
};

/**
 * Display of the frisbee mini-game in front of its tent: the dog looping its idle animation
 * with the frisbee held in its mouth.
 *
 * The frisbee is not a child of the dog in the scene graph but added to the head bone at
 * mount, so it follows the animation instead of staying put relative to the body.
 */
export function FrisbeeProps({ position, rotation = [0, 0, 0], scale = [1, 1, 1] }: FrisbeeProps) {
    const { scene: dogScene, animations: dogAnimations } = useGLTF(DOG_MODEL);
    const { scene: frisbeeScene } = useGLTF(FRISBEE_MODEL);

    // The dog is skinned: scene.clone() does not rebind the bones and every copy collapsed
    // into the same position. SkeletonUtils.clone fixes that.
    const dog = useMemo(() => skeletonClone(dogScene), [dogScene]);

    /*
      holder carries the Unity transform (in Head space); inside it the mesh is turned back to
      its Blender axes. The .glb's own node transform (a Y rotation and a Y squash baked in at
      export) is reset, since the Unity scale already flattens the disc.
    */
    const frisbee = useMemo(() => {
        const mesh = frisbeeScene.clone();

        mesh.traverse((node) => {
            if (node !== mesh) {
                node.position.set(0, 0, 0);
                node.quaternion.identity();
                node.scale.set(1, 1, 1);
            }
        });

        mesh.rotation.x = FRISBEE_MESH_TO_BLENDER;

        const holder = new Group();
        holder.position.copy(FRISBEE_IN_HEAD.position);
        holder.quaternion.copy(FRISBEE_IN_HEAD.quaternion);
        holder.scale.copy(FRISBEE_IN_HEAD.scale);
        holder.add(mesh);

        return holder;
    }, [frisbeeScene]);

    // The root is the clone and not the original scene: each instance needs its own mixer
    // bound to its own bones, otherwise the animation only took on one of them.
    const { actions } = useAnimations(dogAnimations, dog);

    /*
      The animation moves the bones and not the model's root node, so parenting the frisbee to
      `dog` left it motionless relative to the body — it has to hang from the head bone. add()
      (not attach()) keeps FRISBEE_IN_HEAD as the local transform in bone space.

      useEffect and not useLayoutEffect: when effects re-run (Vite hot reload), useAnimations'
      own useEffect cleanup calls mixer.stopAllAction(). A layout effect plays the idle before
      that cleanup and the dog froze; a passive effect declared after useAnimations always
      runs after it.
    */
    useEffect(() => {
        const idle = actions[DOG_IDLE_CLIP];

        if (!idle) {
            console.warn(`FrisbeeProps: clip "${DOG_IDLE_CLIP}" não existe em ${DOG_MODEL}.`);
        } else {
            idle.reset().play();
        }

        const head = dog.getObjectByName(DOG_HEAD_BONE);

        if (head === undefined) {
            console.warn(`FrisbeeProps: bone "${DOG_HEAD_BONE}" não existe em ${DOG_MODEL}.`);
        } else {
            head.add(frisbee);
        }

        return () => {
            head?.remove(frisbee);
            idle?.stop();
        };
    }, [actions, dog, frisbee]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={dog} position={[0, 0.1, 0]} scale={0.13} rotation={[0, -Math.PI / 1.5, 0]} />
        </group>
    );
}

useGLTF.preload(DOG_MODEL);
useGLTF.preload(FRISBEE_MODEL);
