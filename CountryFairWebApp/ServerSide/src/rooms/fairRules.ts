import type { FairState } from "../schemas/FairSchema.js";
import type { ProgressUpdate } from "./messageValidation.js";

/**
 * Business rules of {@link FairSceneRoom}, as pure state reducers: each one takes the room's
 * {@link FairState} and an already-validated payload (see `messageValidation.ts`) and mutates
 * the state, with no access to clients or the network. Keeping them out of the `onMessage`
 * closures means they can be unit-tested on a bare `new FairState()`, without booting a server.
 */

/** Unity scene name of the fair hub (`ConnectToWebApp.UpdateScene()` sends it lower-cased). */
export const HUB_SCENE = "countryfair";

/** Whether `sceneName` is the fair hub rather than a mini-game. Case- and whitespace-insensitive. */
export function isHubScene(sceneName: string): boolean {
    return sceneName.trim().toLowerCase() === HUB_SCENE;
}

/**
 * Resets score/streak/goal when the player enters a mini-game, so its tutorial never opens on
 * the previous session's numbers. Returning to the hub keeps them, so the web client can still
 * show how the last session ended.
 */
export function applySceneChange(state: FairState, sceneName: string): void {
    if (isHubScene(sceneName)) {
        return;
    }

    state.score = 0;
    state.streak = 0;
    state.sessionGoal = 0;
}

/** Live mini-game progress, as reported by the game. */
export function applyProgress(state: FairState, progress: ProgressUpdate): void {
    state.score = progress.score;
    state.streak = progress.streak;
    state.sessionGoal = progress.goal;
}

/**
 * Records the latest tent order (tent id -> 1-based slot) in the synced state, so the room
 * always holds the last order either side reported. Keys are stored lower-cased, matching the
 * web client's `MiniGameType` ids, whatever casing the sender used. Tents absent from a partial
 * `order` keep their previous slot.
 */
export function applyTentOrder(state: FairState, order: Record<string, string>): void {
    for (const [tent, slot] of Object.entries(order)) {
        state.tentOrders.set(tent.toLowerCase(), slot);
    }
}
