import assert from "assert";
import { FairState } from "../src/schemas/FairSchema.js";
import { applyProgress, applySceneChange, applyTentOrder, isHubScene } from "../src/rooms/fairRules.js";

function stateWithProgress(): FairState {
    const state = new FairState();

    applyProgress(state, { score: 7, goal: 10, streak: 3 });

    return state;
}

describe("fairRules", () => {
    it("isHubScene ignores case and surrounding whitespace", () => {
        assert.strictEqual(isHubScene("CountryFair"), true);
        assert.strictEqual(isHubScene(" countryfair "), true);
        assert.strictEqual(isHubScene("archerygame"), false);
    });

    it("applyProgress copies score, streak and goal into the state", () => {
        const state = stateWithProgress();

        assert.deepStrictEqual([state.score, state.streak, state.sessionGoal], [7, 3, 10]);
    });

    it("applySceneChange resets progress when entering a mini-game", () => {
        const state = stateWithProgress();

        applySceneChange(state, "frisbeegame");

        assert.deepStrictEqual([state.score, state.streak, state.sessionGoal], [0, 0, 0]);
    });

    it("applySceneChange keeps progress when returning to the hub", () => {
        const state = stateWithProgress();

        applySceneChange(state, "countryfair");

        assert.deepStrictEqual([state.score, state.streak, state.sessionGoal], [7, 3, 10]);
    });

    it("applyTentOrder stores lower-cased keys and merges partial orders", () => {
        const state = new FairState();

        applyTentOrder(state, { ARCHERY: "1", FRISBEE: "2", FISHING: "3", DUCKGAME: "4" });
        applyTentOrder(state, { archery: "2", frisbee: "1" });

        assert.deepStrictEqual(state.tentOrders.toJSON(), { archery: "2", frisbee: "1", fishing: "3", duckgame: "4" });
    });
});
