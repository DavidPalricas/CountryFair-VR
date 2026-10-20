import assert from "assert";
import { ColyseusTestServer, boot } from "@colyseus/testing";

import appConfig from "../src/app.config.js";
import { CountryFairRoom } from "../src/rooms/CountryFairRoom.js";
import type { FairSceneRoom } from "../src/rooms/FairSceneRoom.js";

const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

/** Resolves with `true` if `message` reaches `client` within `ms`, `false` otherwise. */
async function receives(client: any, message: string, ms = 300): Promise<boolean> {
    return client.waitForMessage(message, ms).then(() => true, () => false);
}

describe("FairSceneRoom", () => {
    let colyseus: ColyseusTestServer;
    let room: FairSceneRoom;

    before(async () => colyseus = await boot(appConfig));
    after(async () => colyseus.shutdown());

    beforeEach(async () => {
        await colyseus.cleanup();
        room = await colyseus.createRoom<FairSceneRoom>("fairsceneroom", {});
    });

    /** Joins `platform`, with the SDK's own automatic reconnection off so drops stay deterministic. */
    async function join(platform: unknown) {
        const client = await colyseus.connectTo(room, { platform });

        client.reconnection.enabled = false;

        return client;
    }

    /** Sends `type` from `client` and resolves once the room's handler for it has run. */
    async function sendAndHandle(client: any, type: string, payload?: unknown) {
        const handled = room.waitForMessage(type);

        client.send(type, payload);

        await handled;
    }

    /** Joins the web client, then the game, and waits for the web side's `"gamejoined"`. */
    async function pair() {
        const web = await join("web");
        const gameJoined = web.waitForMessage("gamejoined");
        const game = await join("game");

        await gameJoined;

        return { web, game };
    }

    describe("pairing", () => {
        it("sends \"gamejoined\" to the web client once both platforms joined", async () => {
            const { web } = await pair();

            assert.strictEqual(room.clients.length, 2);
            assert.ok(web.sessionId);
        });

        it("normalizes the platform, so \" Game \" takes the game slot (W-S3)", async () => {
            const web = await join("web");
            const gameJoined = web.waitForMessage("gamejoined");

            await join(" Game ");
            await gameJoined;
        });

        it("rejects a missing or unknown platform with a failed join (W-S3)", async () => {
            await assert.rejects(colyseus.connectTo(room, {}));
            await assert.rejects(colyseus.connectTo(room, { platform: "tablet" }));
            await assert.rejects(colyseus.connectTo(room, { platform: 42 }));
        });

        it("fails the join of a second client of a platform that already joined", async () => {
            await join("web");

            await assert.rejects(join("web"));
            await wait(100);
            assert.strictEqual(room.clients.length, 1);
        });

        it("fails the join of any client arriving after the pair is complete", async () => {
            await pair();

            await assert.rejects(join("game"));
            await wait(100);
            assert.strictEqual(room.clients.length, 2);
        });
    });

    describe("leaving and reconnecting", () => {
        it("lets the web client rejoin after leaving (W-S1)", async () => {
            const { web } = await pair();

            await web.leave();

            const newWeb = await join("web");

            // The pair is complete again, but "gamejoined" went out during the join itself.
            await wait(50);
            assert.strictEqual(room.clients.length, 2);
            assert.ok(room.clients.some((c) => c.sessionId === newWeb.sessionId));
        });

        it("tells the web client when the game leaves for good", async () => {
            const { web, game } = await pair();
            const disconnected = web.waitForMessage("gameDisconnected");

            await game.leave();
            await disconnected;
        });

        it("keeps a dropped client's slot and lets it resume the same session (W-S7)", async () => {
            const { web, game } = await pair();
            const token = game.reconnectionToken;

            await game.leave(false);
            await wait(50);

            const resumed = await colyseus.sdk.reconnect(token);

            assert.strictEqual(resumed.sessionId, game.sessionId);
            assert.strictEqual(await receives(web, "gameDisconnected"), false);

            // Still the game: its messages are still authorized.
            await sendAndHandle(resumed, "updatePlayerProgressOnMiniGame", { score: 4, goal: 10, streak: 2 });
            assert.strictEqual(room.state.score, 4);
        });

        it("lets a fresh join take over a dropped client's slot, as Unity's retry loop does (W-S7)", async () => {
            const { web, game } = await pair();

            await game.leave(false);
            await wait(50);

            const gameJoined = web.waitForMessage("gamejoined");
            const newGame = await join("game");

            await gameJoined;
            assert.notStrictEqual(newGame.sessionId, game.sessionId);
            assert.strictEqual(await receives(web, "gameDisconnected"), false);
        });

        it("frees the slot once the reconnection window expires (W-S7)", async () => {
            const original = CountryFairRoom.reconnectionSeconds;
            CountryFairRoom.reconnectionSeconds = 0.2;

            try {
                const { web, game } = await pair();
                const disconnected = web.waitForMessage("gameDisconnected");

                await game.leave(false);
                await disconnected;
            } finally {
                CountryFairRoom.reconnectionSeconds = original;
            }
        });
    });

    describe("state", () => {
        it("keeps progress when a client rejoins (W-S2)", async () => {
            const { web, game } = await pair();

            await sendAndHandle(game, "updatePlayerProgressOnMiniGame", { score: 5, goal: 10, streak: 3 });

            await web.leave();
            await join("web");

            assert.deepStrictEqual([room.state.score, room.state.streak, room.state.sessionGoal], [5, 3, 10]);
        });

        it("resets progress when the game enters a mini-game, not the hub", async () => {
            const { game } = await pair();

            await sendAndHandle(game, "updatePlayerProgressOnMiniGame", { score: 5, goal: 10, streak: 3 });

            await sendAndHandle(game, "updateScene", { sceneName: "countryfair" });
            assert.strictEqual(room.state.score, 5);

            await sendAndHandle(game, "updateScene", { sceneName: "archerygame" });
            assert.deepStrictEqual([room.state.score, room.state.streak, room.state.sessionGoal], [0, 0, 0]);
        });

        it("records the tent order in the state (W-S10)", async () => {
            const { web } = await pair();

            await sendAndHandle(web, "updateTentsOrder", { archery: "2", frisbee: "1", fishing: "3", duckgame: "4" });

            assert.deepStrictEqual(room.state.tentOrders.toJSON(), { archery: "2", frisbee: "1", fishing: "3", duckgame: "4" });
        });
    });

    describe("relay and validation (W-S4)", () => {
        it("relays a valid tent order to the other client only", async () => {
            const { web, game } = await pair();
            const received = game.waitForMessage("updateTentsOrder");
            const echoed = receives(web, "updateTentsOrder");

            web.send("updateTentsOrder", { archery: "2", frisbee: "1" });

            assert.deepStrictEqual(await received, { archery: "2", frisbee: "1" });
            assert.strictEqual(await echoed, false);
        });

        it("relays the web's cheat code, trimmed, to the game", async () => {
            const { web, game } = await pair();
            const received = game.waitForMessage("cheatCode");

            web.send("cheatCode", "  giant ");

            assert.strictEqual(await received, "giant");
        });

        it("relays the game's dialogue/tutorial/scene messages to the web", async () => {
            const { web, game } = await pair();

            const dialogue = web.waitForMessage("playerFinishedDialogue");
            game.send("playerFinishedDialogue");
            await dialogue;

            const tutorial = web.waitForMessage("playerFinishedTutorial");
            game.send("playerFinishedTutorial");
            await tutorial;

            const scene = web.waitForMessage("updateScene");
            game.send("updateScene", { sceneName: "frisbeegame", extra: "dropped" });
            assert.deepStrictEqual(await scene, { sceneName: "frisbeegame" });
        });

        it("ignores invalid payloads", async () => {
            const { web, game } = await pair();

            web.send("updateTentsOrder", { archery: "1", frisbee: "1" });
            web.send("cheatCode", 42);
            game.send("updateScene", {});
            game.send("updatePlayerProgressOnMiniGame", { score: "lots", goal: 10, streak: 0 });

            assert.strictEqual(await receives(game, "updateTentsOrder"), false);
            assert.strictEqual(await receives(game, "cheatCode", 50), false);
            assert.strictEqual(await receives(web, "updateScene", 50), false);
            assert.strictEqual(room.state.score, 0);
            assert.strictEqual(room.state.tentOrders.size, 0);
        });

        it("ignores messages from a platform not allowed to send them", async () => {
            const { web, game } = await pair();

            web.send("updatePlayerProgressOnMiniGame", { score: 9, goal: 10, streak: 9 });
            web.send("updateScene", { sceneName: "archerygame" });
            web.send("playerFinishedDialogue");
            game.send("cheatCode", "giant");

            assert.strictEqual(await receives(game, "updateScene"), false);
            assert.strictEqual(await receives(game, "playerFinishedDialogue", 50), false);
            assert.strictEqual(await receives(web, "cheatCode", 50), false);
            assert.strictEqual(room.state.score, 0);
        });
    });
});
