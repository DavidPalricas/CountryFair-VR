import {Client } from "colyseus";
import {CountryFairRoom} from "./CountryFairRoom.js";
import {FairState } from "../schemas/FairSchema.js";
import {isGameSnapshot, isProgressUpdate, isSceneUpdate, sanitizeCheatCode, sanitizeTentOrder} from "./messageValidation.js";
import {applyDialogueFinished, applyGameSnapshot, applyProgress, applySceneChange, applyTentOrder, applyTutorialFinished} from "./fairRules.js";


/**
 * Room for the plaza scene, where the patient picks the order of the mini-game tents.
 *
 * Registered as `fairsceneroom` (see `app.config.ts`) and joined by one Unity headset build
 * (`platform: "game"`) and up to three React clients (`platform: "web"`).
 * Its synced state is a {@link FairState}.
 */
export class FairSceneRoom extends CountryFairRoom {
  // Narrows `this.state` from `CountryFairRoom`'s untyped `object` down to `FairState`,
  // without making the base class generic (which would break its own `broadcast()` typing).
  declare state: FairState;

  onCreate() {
      this.state = new FairState();

      // Every handler below first checks the sender's platform and the payload shape (see
      // `messageValidation.ts`) and silently drops the message otherwise, so a malformed or
      // misdirected message can neither throw nor reach the other client.

      // Both sides can reorder the tents (wrist menu / world on the headset, drag on the web).
      // Only the validated copy is relayed, never the raw payload.
      this.onMessage("updateTentsOrder", (client, data) => {
            if (!this.isSenderAllowed(client, "updateTentsOrder", ["web", "game"])) return;

            const order = sanitizeTentOrder(data);

            if (order === null) {
                  console.warn(`Ignoring invalid "updateTentsOrder" from client ${client.sessionId}:`, data);
                  return;
            }

            applyTentOrder(this.state, order);

            // Relay the room's merged order, not the sender's payload: with several clients
            // dragging at once the payloads can be stale or partial, and relaying each one raw
            // leaves every client holding a different order than `state.tentOrders`. The sender
            // is skipped: its own keys were just applied, so the merge never contradicts it.
            this.broadcast("updateTentsOrder", this.state.tentOrders.toJSON(), { except: client });
      });

      // Sent by the web client when the therapist types a cheat code there. Relayed to the
      // game client only (the web sender is excluded via `except`, and it's a two-client room),
      // which feeds it into `CheatCodes.OnWebCheatCode()` — the same `CheckCheatCode()` gating
      // used for keyboard-typed cheats (see `ConnectToWebApp.receiveCheatCode` in Unity).
      this.onMessage("cheatCode", (client, data) => {
            if (!this.isSenderAllowed(client, "cheatCode", ["web"])) return;

            const code = sanitizeCheatCode(data);

            if (code === null) {
                  console.warn(`Ignoring invalid "cheatCode" from client ${client.sessionId}:`, data);
                  return;
            }

            this.broadcast("cheatCode", code, { except: client })
      });

      // Relayed from the game once a UIDialog (game intro or session-completed message) is
      // dismissed — the tents on screen do not reflect the fair state while one is up, so the
      // web client assumes a dialogue is playing by default as soon as it gets "gamejoined"
      // (matching what the game shows right after connecting: the intro) and waits for this
      // message to drop that assumption. No matching "started" message: the game is the
      // source of truth for when a new dialogue begins, so it simply skips sending this until
      // the next one finishes. The mini-game tutorial has its own, separate
      // "playerFinishedTutorial" message below (Unity sends them distinctly).
      this.onMessage("playerFinishedDialogue", (client) => {
            if (!this.isSenderAllowed(client, "playerFinishedDialogue", ["game"])) return;

            applyDialogueFinished(this.state);

            this.broadcast("playerFinishedDialogue", {}, { except: client })
      });

      // Relayed once the mini-game tutorial is dismissed (skipped or completed) — the
      // mini-game equivalent of "playerFinishedDialogue" above, kept separate because Unity
      // sends them as distinct messages (`ConnectToWebApp.PlayerFinishedTutorial()`).
      this.onMessage("playerFinishedTutorial", (client) => {
            if (!this.isSenderAllowed(client, "playerFinishedTutorial", ["game"])) return;

            applyTutorialFinished(this.state);

            this.broadcast("playerFinishedTutorial", {}, { except: client })
      });

      // Sent by `ConnectToWebApp.UpdateScene()` on every scene load (plaza or mini-game). This
      // is what tells the web client which screen to show — and, for a mini-game scene, which
      // one to name (see `classifyScene`/`MINI_GAME_LABELS` on the web client). Score/streak/
      // goal are reset whenever the new scene isn't the plaza, so a mini-game's tutorial never
      // opens on the previous session's numbers. For the plaza, the game also says whether a
      // dialogue is about to play, so the web shows the fair right away when none is.
      this.onMessage("updateScene", (client, data) => {
            if (!this.isSenderAllowed(client, "updateScene", ["game"])) return;

            if (!isSceneUpdate(data)) {
                  console.warn(`Ignoring invalid "updateScene" from client ${client.sessionId}:`, data);
                  return;
            }

            applySceneChange(this.state, data.sceneName, data.dialogueActive);

            this.broadcast("updateScene", {
                  sceneName: data.sceneName,
                  dialogueActive: this.state.dialogueActive,
            }, { except: client })
      });

      // Sent by `ConnectToWebApp.SyncGameState()` every time the game (re)joins. `updateScene`
      // only fires on a scene *change*, so without this the room would never learn the scene
      // the game started in, and would keep a stale one after the headset restarted. The web
      // clients already got `"gamejoined"` (from `onJoin`, before this arrives), so they are
      // sent the corrected snapshot too.
      this.onMessage("syncGameState", (client, data) => {
            if (!this.isSenderAllowed(client, "syncGameState", ["game"])) return;

            if (!isGameSnapshot(data)) {
                  console.warn(`Ignoring invalid "syncGameState" from client ${client.sessionId}:`, data);
                  return;
            }

            applyGameSnapshot(this.state, data);

            this.broadcast("syncGameState", {
                  sceneName: this.state.sceneName,
                  dialogueActive: this.state.dialogueActive,
                  tutorialActive: this.state.tutorialActive,
            }, { except: client });
      });

      // Score/streak/session goal while a mini-game is being played. Written to `state`
      // (rather than just relayed) so a web client that (re)connects mid-session gets the
      // current values immediately instead of waiting for the next change — see FairState.
      this.onMessage("updatePlayerProgressOnMiniGame", (client, data) => {
            if (!this.isSenderAllowed(client, "updatePlayerProgressOnMiniGame", ["game"])) return;

            if (!isProgressUpdate(data)) {
                  console.warn(`Ignoring invalid "updatePlayerProgressOnMiniGame" from client ${client.sessionId}:`, data);
                  return;
            }

            applyProgress(this.state, data);
      });
    }
 /**
  * Admits a client and, whenever the game and a web client are both present, hands the fair state to that web client.
  *
  * `options.platform` must be `"web"` or `"game"`:
  * - it is normalized (trimmed, lower-cased) once, and only the normalized value is stored;
  * - a missing or unknown platform throws, which Colyseus turns into a failed join on the client;
  * - if that platform's slot is held by a dropped client still inside its reconnection window,
  *   the newcomer takes the slot over (see `takeOverDroppedSlot`);
  * - otherwise a client whose platform has no free slot (1 game, 3 web) is dropped with `client.leave()`, which Colyseus also reports as a failed join (`early_leave`).
  *
  * `"gamejoined"` carries the current {@link FairState} and goes to each web client: all of
  * them when the game arrives, or just the newcomer when the game is already there. That message
  * is what flips the React client from the waiting screen to the game screen.
  */
 onJoin(client: Client, options: any) {
    const clientPlatform = this.normalizePlatform(options?.platform);

    if (clientPlatform === null) {
       throw new Error(`Invalid platform: ${options?.platform}`);
    }

    // Before the "room full" check: a dropped client keeps the pair counted as complete.
    this.takeOverDroppedSlot(clientPlatform);

    if (this.isPlatformFull(clientPlatform)) {
      console.warn(`Rejecting client ${client.sessionId} because all ${clientPlatform} slots are taken.`);
      client.leave();
      return;
    }

    this.clientPlatforms[client.sessionId] = clientPlatform;

    if (clientPlatform === "game") {
      this.gameClientId = client.sessionId;
    }

    // `state` is created once in `onCreate` and never replaced here, so a client that
    // (re)joins mid-session receives the current score/streak/goal (see FairState).

    if (this.gameClientId === '') {
      return;
    }

    if (clientPlatform === "game") {
      // The game just arrived: every web client already waiting gets the state.
      console.log('Game joined. Streaming the game state to the web devices');

      this.clients.forEach((other) => {
        if (this.platformOf(other) === "web") {
          console.log(`Sending game state to client ${other.sessionId}`);
          other.send("gamejoined", this.state);
        }
      });
    } else {
      // A web device joined a session whose game is already connected.
      console.log(`Sending game state to client ${client.sessionId}`);
      client.send("gamejoined", this.state);
    }
   }
}
