import { useEffect } from "react";
import { useThree } from "@react-three/fiber";

/**
 * Draws a single frame as soon as the scene has mounted, for a canvas that starts out with
 * `frameloop="never"` behind the waiting screen.
 *
 * `<Canvas>` wraps its children in one Suspense boundary, so this only mounts once every
 * `useGLTF`/`useTexture` asset has loaded. That one frame is what uploads the geometry and
 * textures to the GPU and compiles the shaders; without it the first reveal would pay that
 * cost and stutter. After it, nothing is drawn until the scene is revealed, because the frames
 * would only ever be seen behind the overlay.
 */
export function WarmUpFrame() {
    const advance = useThree((state) => state.advance);

    useEffect(() => {
        advance(performance.now());
    }, [advance]);

    return null;
}
