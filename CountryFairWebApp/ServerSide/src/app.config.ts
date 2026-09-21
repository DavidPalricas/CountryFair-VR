import {defineServer,defineRoom,} from "colyseus";
import {WebSocketTransport} from "@colyseus/ws-transport";
/**
 * Import your Room files
 */
import {FairSceneRoom} from "./rooms/FairSceneRoom.js";


/**
 * Colyseus server definition for the Country Fair companion platform.
 *
 * Both room handlers point at the same {@link FairSceneRoom}: `my_room` is the name the
 * Colyseus tooling (playground, load tests) expects, while `fairsceneroom` is the name the
 * real clients ask for — the React client in `ClientSide/src/network/client.ts` and the
 * Unity headset build both call `joinOrCreate("fairsceneroom", { platform })`.
 */
const server = defineServer({
    /**
     * Custom ping tolerance: the Unity "game" client's main thread stalls for a few seconds
     * right when a UIDialog finishes (Destroy() of the dialogue canvas plus SetActive(true) on
     * PostDialogueElements, which spawns the whole tent-personalization system in one go). The
     * default WebSocketTransport (ping every 3s, drop after 2 missed pongs — ~6-9s tolerance)
     * was mistaking that hitch for a dead client and force-closing the connection mid-session,
     * which then had no way to recover (see ConnectToWebApp.ConnectLoop on the Unity side).
     * Raised to ~24-27s of tolerance; a real dead connection is still caught, just not this fast.
     */
    transport: new WebSocketTransport({ pingInterval: 3000, pingMaxRetries: 8 }),

    /**
     * Define your room handlers:
     */

    rooms: {
        my_room: defineRoom(FairSceneRoom)

}});

server.define("fairsceneroom", FairSceneRoom);

export default server;
