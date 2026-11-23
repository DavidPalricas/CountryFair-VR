
 import { Schema, type, MapSchema } from "@colyseus/schema";

 /**
  * Session parameters of one tent, mirroring Unity's `SessionParamsMenu` (and the matching
  * fields of `TentConfigData`). Stored per tent in {@link FairState.tentParams}.
  */
 export class TentParams extends Schema {
   /** Number of repetitions, 1 to 5 (`SessionParamsMenu._repetitions`). */
   @type("number") repetitions: number = 1;
   /** Maximum session time in minutes, 1 to 15 (`SessionParamsMenu._sessionTime`). */
   @type("number") sessionTimeMinutes: number = 1;
   /** `SessionParamsMenu.DIFFICULTY_CHANGE` lower-cased: `"decrease"`, `"keep"` or `"increase"`. */
   @type("string") difficultyChange: string = "keep";
 }

 /**
  * Synced state of the plaza scene ({@link FairSceneRoom}) — the network contract between
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
    * Reset to 0 whenever `"updateScene"` reports a non-plaza scene, so a new mini-game never
    * opens on the previous one's numbers while its tutorial is still playing.
    */
   @type("number") score: number = 0;
   @type("number") streak: number = 0;
   @type("number") sessionGoal: number = 0;

   /**
    * Where the survivor is, so a web client that joins mid-session shows the right screen
    * instead of waiting for the next `"updateScene"` / `"playerFinished*"` message (which only
    * fire on a change). Written by `applySceneChange`, `applyDialogueFinished` and
    * `applyTutorialFinished` in `fairRules.ts`.
    *
    * - `sceneName`: last scene the game reported, lower-cased; `""` until it reports one.
    * - `dialogueActive`: a plaza dialogue (intro / session completed) is assumed to be up. True
    *   until the game says it finished, and re-armed on every return to the plaza.
    * - `tutorialActive`: same, for the mini-game tutorial; re-armed on every mini-game load.
    */
   @type("string") sceneName: string = "";
   @type("boolean") dialogueActive: boolean = true;
   @type("boolean") tutorialActive: boolean = true;

   /**
    * Session parameters of each tent in the configuration scene (tent id, lower-cased ->
    * {@link TentParams}), written by `"updateTentParams"` (see `applyTentParams`). Like
    * `tentOrders`, this is the room's memory of the last values either side reported, so a web
    * client that (re)joins shows them right away. A tent is absent until someone reports it.
    */
   @type({ map: TentParams }) tentParams: MapSchema<TentParams> = new MapSchema<TentParams>();

   /**
    * Affected arm of the survivor, shared by every tent (Unity keeps it in the static
    * `SessionParamsMenu.s_isLeftArmAffected`). Written by `"updateAffectedArm"`.
    */
   @type("boolean") isLeftArmAffected: boolean = true;

   /**
    * Progress of the session configuration test, reported by the game on `"updateConfigState"`
    * (Unity's `ConfigSessionManager`): which survivor's file the tents are being configured
    * against, and whether the tents already match it.
    *
    * - `survivorFile`: lower-cased id of the survivor (`"rutepereira"`...), `""` until reported.
    *   It is also the file name of the image the web client shows (`public/survivors/<id>.png`).
    * - `configCompleted`: the tents match the survivor's file, so the web client swaps the file
    *   for the "advance" button. Cleared by `"nextConfiguration"` and by the game on any mismatch.
    */
   @type("string") survivorFile: string = "";
   @type("boolean") configCompleted: boolean = false;

   /**
    * The survivor in `survivorFile` is the last one of the test: the web client's button then ends
    * the configuration instead of advancing to the next one. Reported with `"updateConfigState"`.
    */
   @type("boolean") isLastConfig: boolean = false;

   /**
    * The therapist ended the configuration (pressed the button on the last survivor). The game quits
    * right after, so the web clients keep the waiting screen but say the configuration is over.
    * Set by `applyNextConfiguration`, cleared when the game joins the room again.
    */
   @type("boolean") configFinished: boolean = false;
  }
