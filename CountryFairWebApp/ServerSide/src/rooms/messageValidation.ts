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

export interface SceneUpdate {
    sceneName: string;
}

export interface ProgressUpdate {
    score: number;
    goal: number;
    streak: number;
}

function isObject(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function isNonNegativeInteger(value: unknown): value is number {
    return typeof value === "number" && Number.isInteger(value) && value >= 0;
}

/** `{ sceneName }` with a non-empty string, as sent by `ConnectToWebApp.UpdateScene()`. */
export function isSceneUpdate(data: unknown): data is SceneUpdate {
    return isObject(data) && typeof data.sceneName === "string" && data.sceneName.trim() !== "";
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
