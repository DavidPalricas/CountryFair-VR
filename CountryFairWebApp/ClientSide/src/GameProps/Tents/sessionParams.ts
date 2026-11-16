/**
 * Pure session-parameter logic (no React, no Three), the web equivalent of Unity's
 * `SessionParamsMenu` + `SessionClampedParameter`. Ranges, defaults and labels must match the
 * Unity panel (`SessionParams.prefab`) and the server's `messageValidation.ts`.
 */

import { TENT_NAMES, type MiniGameType } from "./tentSlots";

/** Unity's `SessionParamsMenu.DIFFICULTY_CHANGE`, lower-cased; the order matches the dropdown. */
export const DIFFICULTY_CHANGES = ["decrease", "keep", "increase"] as const;

export type DifficultyChange = (typeof DIFFICULTY_CHANGES)[number];

/** Portuguese labels of the difficulty options, the same words as the Unity dropdown. */
export const DIFFICULTY_LABELS: Record<DifficultyChange, string> = {
    decrease: "Baixar",
    keep: "Manter",
    increase: "Subir",
};

/** Portuguese labels of the affected arm options (Unity's `AffectedArmOptions`). */
export const ARM_LABELS = { left: "Esquerdo", right: "Direito" } as const;

/** An integer parameter changed with `-` / `+` buttons (Unity's `SessionClampedParameter`). */
export type ClampedRange = { min: number; max: number; step: number };

export const REPETITIONS_RANGE: ClampedRange = { min: 1, max: 5, step: 1 };
export const SESSION_TIME_RANGE: ClampedRange = { min: 1, max: 15, step: 1 };

/** Per-tent session parameters (the affected arm is shared, so it is kept apart). */
export type TentParams = {
    repetitions: number;
    sessionTimeMinutes: number;
    difficultyChange: DifficultyChange;
};

/** Values every panel starts with, the same as the Unity panel's defaults. */
export const DEFAULT_PARAMS: TentParams = {
    repetitions: REPETITIONS_RANGE.min,
    sessionTimeMinutes: SESSION_TIME_RANGE.min,
    difficultyChange: "keep",
};

export type TentParamsByTent = Record<MiniGameType, TentParams>;

/** One `DEFAULT_PARAMS` copy per tent. */
export function initialTentParams(): TentParamsByTent {
    return {
        fishing: { ...DEFAULT_PARAMS },
        archery: { ...DEFAULT_PARAMS },
        frisbee: { ...DEFAULT_PARAMS },
        duckgame: { ...DEFAULT_PARAMS },
    };
}

/** Moves `value` one step up (`direction = 1`) or down (`-1`), clamped to the range. */
export function stepValue(value: number, range: ClampedRange, direction: 1 | -1): number {
    return Math.min(range.max, Math.max(range.min, value + direction * range.step));
}

/** Payload of the `"updateTentParams"` message, shared by Unity, the server and this client. */
export type TentParamsMessage = TentParams & { miniGame: MiniGameType };

function isMiniGameType(type: string): type is MiniGameType {
    return Object.hasOwn(TENT_NAMES, type);
}

function isDifficultyChange(value: string): value is DifficultyChange {
    return (DIFFICULTY_CHANGES as readonly string[]).includes(value);
}

function isInRange(value: unknown, range: ClampedRange): value is number {
    return typeof value === "number" && Number.isInteger(value) && value >= range.min && value <= range.max;
}

/**
 * Validated `{ miniGame, params }` from an `"updateTentParams"` message or a `state.tentParams`
 * entry, or `null`. The server already validates, but the payload comes off the network, so it
 * is checked again, like `orderFromFairState` does for the tent order.
 */
export function parseTentParams(miniGame: unknown, data: unknown): { miniGame: MiniGameType; params: TentParams } | null {
    if (typeof miniGame !== "string" || typeof data !== "object" || data === null) {
        return null;
    }

    const type = miniGame.toLowerCase();
    const { repetitions, sessionTimeMinutes, difficultyChange } = data as Record<string, unknown>;

    if (!isMiniGameType(type) || typeof difficultyChange !== "string") {
        return null;
    }

    const difficulty = difficultyChange.toLowerCase();

    if (!isDifficultyChange(difficulty) || !isInRange(repetitions, REPETITIONS_RANGE) || !isInRange(sessionTimeMinutes, SESSION_TIME_RANGE)) {
        return null;
    }

    return { miniGame: type, params: { repetitions, sessionTimeMinutes, difficultyChange: difficulty } };
}

/**
 * Returns `all` with `miniGame`'s parameters replaced, or the SAME reference when they are
 * already equal (like `swapIntoSlot`), so an echo or a repeated state patch costs no render.
 */
export function withTentParams(all: TentParamsByTent, miniGame: MiniGameType, params: TentParams): TentParamsByTent {
    const current = all[miniGame];

    if (current.repetitions === params.repetitions
        && current.sessionTimeMinutes === params.sessionTimeMinutes
        && current.difficultyChange === params.difficultyChange) {
        return all;
    }

    return { ...all, [miniGame]: { ...params } };
}
