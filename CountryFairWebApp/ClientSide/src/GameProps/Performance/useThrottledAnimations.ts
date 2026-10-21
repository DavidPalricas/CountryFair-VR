import { useEffect, useMemo, useRef, useState, type RefObject } from "react";
import { useFrame } from "@react-three/fiber";
import { AnimationMixer, Object3D, type AnimationAction, type AnimationClip } from "three";

/**
 * Rate the skeletal animations are sampled at. Updating a skinned mesh means recomputing every
 * bone on the CPU; at 30 instead of 60 per second that work halves, and on a tablet screen the
 * difference in smoothness of an idle loop is not noticeable.
 */
export const ANIMATION_FPS = 30;

/**
 * Drop-in for drei's `useAnimations` (same lazy `actions` map, same cleanup), except that the
 * mixer only advances every 1 / `fps` seconds instead of every frame. The time skipped is not
 * lost: each update receives everything accumulated since the last one, so clips keep their
 * real speed.
 *
 * `root` is either the object to animate or a ref to it, as in drei.
 */
export function useThrottledAnimations(
    clips: AnimationClip[],
    root: Object3D | RefObject<Object3D | null>,
    fps: number = ANIMATION_FPS,
) {
    // The mixer's own root is never used: every clipAction() below passes the root explicitly.
    const [mixer] = useState(() => new AnimationMixer(new Object3D()));

    // Starts at a random point of the interval, so several animated models do not all land
    // their update on the same frame.
    const pending = useRef(Math.random() / fps);

    const api = useMemo(() => {
        const resolveRoot = () => (root instanceof Object3D ? root : root.current);
        const cache: Record<string, AnimationAction> = {};
        const actions: Record<string, AnimationAction | undefined> = {};

        // Created on first access, like drei: a ref root is only filled after the first commit.
        clips.forEach((clip) => Object.defineProperty(actions, clip.name, {
            enumerable: true,
            configurable: true,
            get() {
                const target = resolveRoot();

                if (target === null) {
                    return undefined;
                }

                return cache[clip.name] ??= mixer.clipAction(clip, target);
            },
        }));

        return { actions, names: clips.map((clip) => clip.name), mixer, cache, resolveRoot };
    }, [clips, mixer, root]);

    useFrame((_, delta) => {
        pending.current += delta;

        if (pending.current >= 1 / fps) {
            mixer.update(pending.current);
            pending.current = 0;
        }
    });

    useEffect(() => {
        return () => {
            mixer.stopAllAction();

            const target = api.resolveRoot();

            for (const [name, action] of Object.entries(api.cache)) {
                if (target !== null) {
                    mixer.uncacheAction(action.getClip(), target);
                }

                // The uncached action is dead inside the mixer; dropping it from the cache makes
                // the next access (StrictMode remount, hot reload) create a fresh one instead of
                // replaying the dead one, which made three throw on `_cacheIndex`.
                delete api.cache[name];
            }
        };
    }, [api, mixer]);

    return api;
}
