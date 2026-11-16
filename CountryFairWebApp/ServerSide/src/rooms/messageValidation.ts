/**
 * Payload guards for the messages {@link FairSceneRoom} accepts. Pure functions (no Colyseus,
 * no room state), so they can be unit-tested without booting a server.
 *
 * Every payload arrives straight from a client, so nothing about its shape can be assumed:
 * a guard returns `false` (or `null`) instead of throwing, and the handler drops the message.
 */

/**
 * Tent ids, lower-cased. Mirrors the web client's `MiniGameType` (`tentSlots.ts`) and Unity's
 * `GameManager.MINI_GAMES` enum, which serialises as SCREAMING_SNAKE_CASE.
 */
export const MINI_GAMES: readonly string[] = ["fishing", "archery", "frisbee", "duckgame"];

/**
 * Longest cheat code relayed to the game. Unity's `CheatCodes` keeps a rolling buffer of
 * `_maxCheatLength` (longest registered code, about a dozen characters), so anything longer
 * can never match and is just noise on the wire.
 */
export const MAX_CHEAT_CODE_LENGTH = 32;

/** Inclusive range of the repetitions parameter (Unity's `SessionParamsMenu._repetitions`). */
export const REPETITIONS_RANGE = { min: 1, max: 5 } as const;

/** Inclusive range of the session time parameter, in minutes (`SessionParamsMenu._sessionTime`). */
export const SESSION_TIME_RANGE = { min: 1, max: 15 } as const;

/** Unity's `SessionParamsMenu.DIFFICULTY_CHANGE` values, lower-cased. */
export const DIFFICULTY_CHANGES: readonly string[] = ["decrease", "keep", "increase"];

/** Session parameters of one tent, as relayed by `"updateTentParams"`. */
export interface TentParamsUpdate {
    /** Tent id, lower-cased (one of {@link MINI_GAMES}). */
    miniGame: string;
    repetitions: number;
    sessionTimeMinutes: number;
    /** One of {@link DIFFICULTY_CHANGES}. */
    difficultyChange: string;
}

export interface AffectedArmUpdate {
    isLeftArmAffected: boolean;
}

/**
 * Survivor ids, lower-cased and without spaces (Unity's `StrokeSurvivorProfile.survivorName`,
 * e.g. "Rute Pereira"). A whitelist, not a pattern: the web client builds an image URL from it.
 * Mirrors the files in the web client's `public/survivors/`.
 */
export const SURVIVOR_FILES: readonly string[] = ["rutepereira", "joanasantos", "tiagobastos"];

/** Payload of `"updateConfigState"`. */
export interface ConfigStateUpdate {
    /** One of {@link SURVIVOR_FILES}. */
    survivor: string;
    /** Whether the tents match the survivor's file. */
    configured: boolean;
    /** Whether this is the last survivor of the test. */
    isLast: boolean;
}

export interface SceneUpdate {
    sceneName: string;
    // Whether a plaza dialogue (session-completed message) is about to play. Optional so an
    // older build that only sends `sceneName` still falls back to assuming one is.
    dialogueActive?: boolean;
}

export interface ProgressUpdate {
    score: number;
    goal: number;
    streak: number;
}

export interface GameSnapshot {
    sceneName: string;
    dialogueActive: boolean;
    tutorialActive: boolean;
}

function isObject(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function isNonNegativeInteger(value: unknown): value is number {
    return typeof value === "number" && Number.isInteger(value) && value >= 0;
}

/**
 * `{ sceneName, dialogueActive? }` with a non-empty scene name and, if present, a boolean
 * `dialogueActive`, as sent by `ConnectToWebApp.OnSceneLoaded()`.
 */
export function isSceneUpdate(data: unknown): data is SceneUpdate {
    return isObject(data)
        && typeof data.sceneName === "string" && data.sceneName.trim() !== ""
        && (data.dialogueActive === undefined || typeof data.dialogueActive === "boolean");
}

/**
 * `{ sceneName, dialogueActive, tutorialActive }` as sent by `ConnectToWebApp.SyncGameState()`
 * right after the game joins: a non-empty scene name and two booleans.
 */
export function isGameSnapshot(data: unknown): data is GameSnapshot {
    return isObject(data)
        && isSceneUpdate(data)
        && typeof data.dialogueActive === "boolean"
        && typeof data.tutorialActive === "boolean";
}

/**
 * `{ score, goal, streak }` as non-negative integers, as sent by
 * `ConnectToWebApp.UpdatePlayerProgressOnMiniGame()`. Anything else would otherwise be
 * assigned straight into `FairState`'s `@type("number")` fields and fail at encode time.
 */
export function isProgressUpdate(data: unknown): data is ProgressUpdate {
    return isObject(data)
        && isNonNegativeInteger(data.score)
        && isNonNegativeInteger(data.goal)
        && isNonNegativeInteger(data.streak);
}

/** Trimmed cheat code, or `null` when it is not a string, empty or too long. */
export function sanitizeCheatCode(data: unknown): string | null {
    if (typeof data !== "string") {
        return null;
    }

    const code = data.trim();

    return code !== "" && code.length <= MAX_CHEAT_CODE_LENGTH ? code : null;
}

/**
 * Validated copy of a tent id -> 1-based slot map, or `null` when it is invalid. Keys keep
 * the sender's casing (both clients lower-case them on receipt) but must name a known tent,
 * at most once ignoring case; slots must be integers in `1..MINI_GAMES.length` with no two
 * tents in the same slot. A partial map is accepted: the web client's `orderFromFairState`
 * keeps the current tent in any slot the payload leaves out.
 *
 * Values are re-emitted as strings, the shape both Unity (`Dictionary<string, string>`) and
 * the web client (`Record<string, string>`) send and expect.
 */
export function sanitizeTentOrder(data: unknown): Record<string, string> | null {
    if (!isObject(data)) {
        return null;
    }

    const entries = Object.entries(data);

    if (entries.length === 0 || entries.length > MINI_GAMES.length) {
        return null;
    }

    const tents = new Set<string>();
    const slots = new Set<number>();
    const order: Record<string, string> = {};

    for (const [tent, value] of entries) {
        const tentId = tent.toLowerCase();
        const slot = typeof value === "string" || typeof value === "number" ? Number(value) : NaN;

        if (!MINI_GAMES.includes(tentId) || tents.has(tentId)) {
            return null;
        }

        if (!Number.isInteger(slot) || slot < 1 || slot > MINI_GAMES.length || slots.has(slot)) {
            return null;
        }

        tents.add(tentId);
        slots.add(slot);
        order[tent] = String(slot);
    }

    return order;
}

function isIntegerInRange(value: unknown, range: { min: number; max: number }): value is number {
    return typeof value === "number" && Number.isInteger(value) && value >= range.min && value <= range.max;
}

/**
 * Validated copy of a tent's session parameters, or `null` when invalid. `miniGame` and
 * `difficultyChange` are lower-cased (Unity sends its enum names, e.g. `"DuckGame"` / `"KEEP"`);
 * the numbers must be integers inside their ranges. Unknown extra fields are dropped.
 */
export function sanitizeTentParams(data: unknown): TentParamsUpdate | null {
    if (!isObject(data) || typeof data.miniGame !== "string" || typeof data.difficultyChange !== "string") {
        return null;
    }

    const miniGame = data.miniGame.trim().toLowerCase();
    const difficultyChange = data.difficultyChange.trim().toLowerCase();

    if (!MINI_GAMES.includes(miniGame) || !DIFFICULTY_CHANGES.includes(difficultyChange)) {
        return null;
    }

    if (!isIntegerInRange(data.repetitions, REPETITIONS_RANGE) || !isIntegerInRange(data.sessionTimeMinutes, SESSION_TIME_RANGE)) {
        return null;
    }

    return { miniGame, repetitions: data.repetitions, sessionTimeMinutes: data.sessionTimeMinutes, difficultyChange };
}

/**
 * `{ survivor, configured, isLast? }` as sent by `ConnectToWebApp.UpdateConfigState()`: a known
 * survivor (case- and whitespace-insensitive, so "Rute Pereira" is accepted) and two booleans.
 * `isLast` may be left out (an older build), which counts as `false`.
 */
export function sanitizeConfigState(data: unknown): ConfigStateUpdate | null {
    if (!isObject(data) || typeof data.survivor !== "string" || typeof data.configured !== "boolean") {
        return null;
    }

    if (data.isLast !== undefined && typeof data.isLast !== "boolean") {
        return null;
    }

    const survivor = data.survivor.replace(/\s+/g, "").toLowerCase();

    return SURVIVOR_FILES.includes(survivor)
        ? { survivor, configured: data.configured, isLast: data.isLast === true }
        : null;
}

/** `{ isLeftArmAffected }` with a boolean value, as sent by both clients on `"updateAffectedArm"`. */
export function isAffectedArmUpdate(data: unknown): data is AffectedArmUpdate {
    return isObject(data) && typeof data.isLeftArmAffected === "boolean";
}
