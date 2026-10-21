import assert from "assert";
import {
    MAX_CHEAT_CODE_LENGTH,
    isProgressUpdate,
    isSceneUpdate,
    sanitizeCheatCode,
    sanitizeTentOrder,
} from "../src/rooms/messageValidation.js";

describe("messageValidation", () => {
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
            assert.strictEqual(sanitizeCheatCode("  giant "), "giant");
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
