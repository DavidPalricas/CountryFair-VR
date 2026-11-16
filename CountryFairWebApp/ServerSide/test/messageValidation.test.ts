import assert from "assert";
import {
    MAX_CHEAT_CODE_LENGTH,
    isAffectedArmUpdate,
    isProgressUpdate,
    isSceneUpdate,
    sanitizeCheatCode,
    sanitizeConfigState,
    sanitizeTentOrder,
    sanitizeTentParams,
} from "../src/rooms/messageValidation.js";

describe("messageValidation", () => {
    describe("sanitizeConfigState", () => {
        it("normalizes Unity's survivor name to the file id", () => {
            assert.deepStrictEqual(sanitizeConfigState({ survivor: "Rute Pereira", configured: true }), { survivor: "rutepereira", configured: true, isLast: false });
            assert.deepStrictEqual(sanitizeConfigState({ survivor: "TiagoBastos", configured: false, isLast: true, extra: 1 }), { survivor: "tiagobastos", configured: false, isLast: true });
        });

        it("rejects a non-boolean isLast", () => {
            assert.strictEqual(sanitizeConfigState({ survivor: "Rute Pereira", configured: true, isLast: "yes" }), null);
        });

        it("rejects unknown survivors (the id becomes an image URL) and bad types", () => {
            assert.strictEqual(sanitizeConfigState({ survivor: "../secret", configured: true }), null);
            assert.strictEqual(sanitizeConfigState({ survivor: "Ana", configured: true }), null);
            assert.strictEqual(sanitizeConfigState({ survivor: "Rute Pereira", configured: "yes" }), null);
            assert.strictEqual(sanitizeConfigState({ survivor: "Rute Pereira" }), null);
            assert.strictEqual(sanitizeConfigState(null), null);
        });
    });

    describe("sanitizeTentParams", () => {
        it("lower-cases Unity's enum names and drops extra fields", () => {
            const params = { miniGame: "DuckGame", repetitions: 3, sessionTimeMinutes: 10, difficultyChange: "INCREASE", extra: 1 };

            assert.deepStrictEqual(sanitizeTentParams(params), {
                miniGame: "duckgame", repetitions: 3, sessionTimeMinutes: 10, difficultyChange: "increase",
            });
        });

        it("rejects out-of-range, non-integer and unknown values", () => {
            const valid = { miniGame: "archery", repetitions: 1, sessionTimeMinutes: 15, difficultyChange: "keep" };

            assert.notStrictEqual(sanitizeTentParams(valid), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, repetitions: 0 }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, repetitions: 6 }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, repetitions: "3" }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, sessionTimeMinutes: 16 }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, sessionTimeMinutes: 2.5 }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, difficultyChange: "harder" }), null);
            assert.strictEqual(sanitizeTentParams({ ...valid, miniGame: "carousel" }), null);
            assert.strictEqual(sanitizeTentParams(null), null);
        });
    });

    describe("isAffectedArmUpdate", () => {
        it("requires a boolean isLeftArmAffected", () => {
            assert.strictEqual(isAffectedArmUpdate({ isLeftArmAffected: false }), true);
            assert.strictEqual(isAffectedArmUpdate({ isLeftArmAffected: "left" }), false);
            assert.strictEqual(isAffectedArmUpdate({}), false);
            assert.strictEqual(isAffectedArmUpdate(true), false);
        });
    });

    describe("sanitizeTentOrder", () => {
        it("accepts Unity's full SCREAMING_SNAKE_CASE map and keeps its keys", () => {
            const order = { ARCHERY: "1", FRISBEE: "2", FISHING: "3", DUCKGAME: "4" };

            assert.deepStrictEqual(sanitizeTentOrder(order), order);
        });

        it("accepts a partial map and numeric slots, re-emitting slots as strings", () => {
            assert.deepStrictEqual(sanitizeTentOrder({ archery: 2 }), { archery: "2" });
        });

        it("rejects two tents in the same slot", () => {
            assert.strictEqual(sanitizeTentOrder({ archery: "1", frisbee: "1" }), null);
        });

        it("rejects the same tent twice, ignoring case", () => {
            assert.strictEqual(sanitizeTentOrder({ archery: "1", ARCHERY: "2" }), null);
        });

        it("rejects unknown tents, out-of-range slots and non-objects", () => {
            assert.strictEqual(sanitizeTentOrder({ carousel: "1" }), null);
            assert.strictEqual(sanitizeTentOrder({ archery: "5" }), null);
            assert.strictEqual(sanitizeTentOrder({ archery: "1.5" }), null);
            assert.strictEqual(sanitizeTentOrder({ archery: true }), null);
            assert.strictEqual(sanitizeTentOrder({}), null);
            assert.strictEqual(sanitizeTentOrder(["1"]), null);
            assert.strictEqual(sanitizeTentOrder("archery"), null);
            assert.strictEqual(sanitizeTentOrder(null), null);
        });
    });

    describe("sanitizeCheatCode", () => {
        it("trims the code", () => {
            assert.strictEqual(sanitizeCheatCode("  frisbee "), "frisbee");
        });

        it("rejects empty, too long and non-string codes", () => {
            assert.strictEqual(sanitizeCheatCode("   "), null);
            assert.strictEqual(sanitizeCheatCode("x".repeat(MAX_CHEAT_CODE_LENGTH + 1)), null);
            assert.strictEqual(sanitizeCheatCode(3), null);
            assert.strictEqual(sanitizeCheatCode(undefined), null);
        });
    });

    describe("isSceneUpdate", () => {
        it("requires a non-empty sceneName string", () => {
            assert.strictEqual(isSceneUpdate({ sceneName: "archerygame" }), true);
            assert.strictEqual(isSceneUpdate({ sceneName: " " }), false);
            assert.strictEqual(isSceneUpdate({ sceneName: 1 }), false);
            assert.strictEqual(isSceneUpdate({}), false);
            assert.strictEqual(isSceneUpdate(undefined), false);
        });

        it("accepts an optional boolean dialogueActive", () => {
            assert.strictEqual(isSceneUpdate({ sceneName: "countryfair", dialogueActive: false }), true);
            assert.strictEqual(isSceneUpdate({ sceneName: "countryfair", dialogueActive: "no" }), false);
        });
    });

    describe("isProgressUpdate", () => {
        it("requires score, goal and streak as non-negative integers", () => {
            assert.strictEqual(isProgressUpdate({ score: 3, goal: 10, streak: 0 }), true);
            assert.strictEqual(isProgressUpdate({ score: "3", goal: 10, streak: 0 }), false);
            assert.strictEqual(isProgressUpdate({ score: -1, goal: 10, streak: 0 }), false);
            assert.strictEqual(isProgressUpdate({ score: 1.5, goal: 10, streak: 0 }), false);
            assert.strictEqual(isProgressUpdate({ score: 3, goal: 10 }), false);
            assert.strictEqual(isProgressUpdate(null), false);
        });
    });
});
