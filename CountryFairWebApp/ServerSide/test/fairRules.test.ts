import assert from "assert";
import { FairState } from "../src/schemas/FairSchema.js";
import { applyDialogueFinished, applyProgress, applySceneChange, applyTentOrder, applyTutorialFinished, isHubScene } from "../src/rooms/fairRules.js";

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

    it("tracks the scene, and re-arms the dialogue/tutorial flags on every scene load", () => {
        const state = new FairState();

        assert.deepStrictEqual([state.sceneName, state.dialogueActive, state.tutorialActive], ["", true, true]);

        applyDialogueFinished(state);
        applySceneChange(state, "ArcheryGame");
        applyTutorialFinished(state);

        assert.deepStrictEqual([state.sceneName, state.dialogueActive, state.tutorialActive], ["archerygame", false, false]);

        applySceneChange(state, "frisbeegame");
        assert.strictEqual(state.tutorialActive, true);

        applySceneChange(state, "countryfair");
        assert.deepStrictEqual([state.sceneName, state.dialogueActive], ["countryfair", true]);
    });

    it("applyTentOrder stores lower-cased keys and merges partial orders", () => {
        const state = new FairState();

        applyTentOrder(state, { ARCHERY: "1", FRISBEE: "2", FISHING: "3", DUCKGAME: "4" });
        applyTentOrder(state, { archery: "2", frisbee: "1" });

        assert.deepStrictEqual(state.tentOrders.toJSON(), { archery: "2", frisbee: "1", fishing: "3", duckgame: "4" });
    });
});
