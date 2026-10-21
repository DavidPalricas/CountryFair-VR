/**
 * Pure ordering logic: no React and no Three, so it can be tested without mounting a scene
 * and wired to Colyseus later without touching the components. It is the equivalent of
 * Unity's `PlaceHolderManager.UpdateElementPosition()`.
 */

import { DROP_RADIUS, TENT_NAMES, TENT_SLOTS, type MiniGameType } from "./tentSlots";

/** The array index is the slot index: `order[2]` is the tent occupying slot 3. */
export type TentOrder = MiniGameType[];

/**
 * Returns the index of the slot whose centre is within DROP_RADIUS in X of the point, or
 * null when the drop landed in the dead zone between slots. This replaces the Unity
 * collider `OnTriggerEnter`/`Exit` pair, which does not exist here because R3F brings no
 * physics.
 *
 * Only X is considered because the drag is locked to the row line (see `useTentDrag`): all
 * slots share the same Z, so the Z distance is always zero.
 */
export function slotIndexAtX(x: number): number | null {
    for (let slot = 0; slot < TENT_SLOTS.length; slot++) {
        if (Math.abs(x - TENT_SLOTS[slot].position[0]) <= DROP_RADIUS) {
            return slot;
        }
    }

    return null;
}

/**
 * Swaps `tent` with whoever occupies `targetSlot`.
 *
 * Returns the SAME reference when nothing changes, so React's setState does not trigger a
 * needless render. An unknown tent or an out-of-range slot is a no-op for the same reason.
 */
export function swapIntoSlot(order: TentOrder, tent: MiniGameType, targetSlot: number): TentOrder {
    const currentSlot = order.indexOf(tent);

    if (currentSlot === -1 || targetSlot < 0 || targetSlot >= order.length) {
        return order;
    }

    if (currentSlot === targetSlot) {
        return order;
    }

    const swapped = [...order];
    swapped[currentSlot] = order[targetSlot];
    swapped[targetSlot] = tent;

    return swapped;
}

/**
 * Inverse of the tent id -> 1-based slot map `GameScreen.sendFairState` builds: rebuilds a
 * `TentOrder` from the `"updateTentsOrder"` payload Unity's `PlaceHolderManager.GetFairState()`
 * broadcasts. Slots absent from `fairState` keep whatever tent `current` already has there.
 *
 * Unity's `MINI_GAMES` enum serialises as SCREAMING_SNAKE_CASE (`element.miniGame.ToString()`),
 * while `MiniGameType` is lowercase, so the key is lower-cased here the same way
 * `PlaceHolderManager.OnOtherManagerUpdate()` lower-cases both sides before comparing.
 *
 * The payload comes off the network, so nothing in it is trusted: unknown tents and slots that
 * are not a whole number in range are skipped. Each entry is applied as a swap (`swapIntoSlot`)
 * rather than written over the slot, so the result is always a permutation of `current` — a
 * duplicate or partial payload can never put one tent in two slots (duplicate React keys in
 * `GameScreen`) or lose one. For a complete, valid payload the swaps land exactly on its order,
 * because a tent already placed is never displaced by a later entry for a different slot.
 *
 * Returns the SAME reference when nothing changes, like `swapIntoSlot`.
 */
export function orderFromFairState(fairState: unknown, current: TentOrder): TentOrder {
    if (typeof fairState !== "object" || fairState === null) {
        console.warn("Ignoring invalid tent order:", fairState);
        return current;
    }

    let next = current;

    Object.entries(fairState).forEach(([key, slot]) => {
        const type = key.toLowerCase();
        const index = Number(slot) - 1;

        if (!isMiniGameType(type) || !Number.isInteger(index)) {
            console.warn(`Ignoring invalid tent order entry ${key}: ${slot}`);
            return;
        }

        next = swapIntoSlot(next, type, index);
    });

    return next;
}

function isMiniGameType(type: string): type is MiniGameType {
    return Object.hasOwn(TENT_NAMES, type);
}
