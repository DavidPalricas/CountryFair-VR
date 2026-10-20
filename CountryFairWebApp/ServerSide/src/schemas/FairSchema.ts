
 import { Schema, type, MapSchema } from "@colyseus/schema";

 /**
  * Synced state of the fair hub scene ({@link FairSceneRoom}) — the network contract between
  * the Unity headset (`platform: "game"`) and the React client (`platform: "web"`).
  */
 export class FairState extends Schema {
   /**
    * Tent id -> slot/order value, mirroring the web client's local `TentOrder` (see
    * `ClientSide/src/GameProps/Tents/tentOrder.ts`). `MapSchema` is required instead of a plain
    * `Map`/`Record` so Colyseus can track and patch per-key mutations to clients.
    *
    * Written by `FairSceneRoom`'s `"updateTentsOrder"` handler (see `applyTentOrder` in
    * `fairRules.ts`) with the last order either side reported, keys lower-cased. Both clients
    * still apply reorders from the `"updateTentsOrder"` message itself; this map is the room's
    * memory of the order, so it is already part of the state a (re)joining client receives.
    */
   @type({ map: "string" }) tentOrders: MapSchema<string> = new MapSchema<string>();

   /**
    * Live progress of the mini-game currently being played, written by
    * `FairSceneRoom`'s `"updatePlayerProgressOnMiniGame"` handler. Using synced fields
    * (instead of a plain broadcast message, like `updateTentsOrder`) means a web client that
    * (re)connects mid-session receives the current values immediately on join instead of
    * waiting for the next score/streak change.
    *
    * Reset to 0 whenever `"updateScene"` reports a non-hub scene, so a new mini-game never
    * opens on the previous one's numbers while its tutorial is still playing.
    */
   @type("number") score: number = 0;
   @type("number") streak: number = 0;
   @type("number") sessionGoal: number = 0;
  }
