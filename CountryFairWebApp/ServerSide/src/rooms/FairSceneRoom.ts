import {Client } from "colyseus";
import {CountryFairRoom} from "./CountryFairRoom.js";
import {FairState } from "../schemas/FairSchema.js";


/**
 * Room for the fair hub scene, where the patient picks the order of the mini-game tents.
 *
 * Registered as `fairsceneroom` (see `app.config.ts`) and joined by exactly two clients:
 * the Unity headset build (`platform: "game"`) and the React client (`platform: "web"`).
 * Its synced state is a {@link FairState}.
 */
export class FairSceneRoom extends CountryFairRoom {
  // Narrows `this.state` from `CountryFairRoom`'s untyped `object` down to `FairState`,
  // without making the base class generic (which would break its own `broadcast()` typing).
  declare state: FairState;

  onCreate() {
      this.state = new FairState();

      this.onMessage("updateTentsOrder", (client, data) => {
            this.broadcast("updateTentsOrder", data, { except: client })
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
            this.broadcast("playerFinishedDialogue", {}, { except: client })
      });

      // Relayed once the mini-game tutorial is dismissed (skipped or completed) — the
      // mini-game equivalent of "playerFinishedDialogue" above, kept separate because Unity
      // sends them as distinct messages (`ConnectToWebApp.PlayerFinishedTutorial()`).
      this.onMessage("playerFinishedTutorial", (client) => {
            this.broadcast("playerFinishedTutorial", {}, { except: client })
      });

      // Sent by `ConnectToWebApp.UpdateScene()` on every scene load (hub or mini-game). This
      // is what tells the web client which screen to show — and, for a mini-game scene, which
      // one to name (see `classifyScene`/`MINI_GAME_LABELS` on the web client). Score/streak/
      // goal are reset whenever the new scene isn't the hub, so a mini-game's tutorial never
      // opens on the previous session's numbers.
      this.onMessage<{ sceneName: string }>("updateScene", (client, data) => {
            if (data.sceneName.toLowerCase() !== "countryfair") {
                  this.state.score = 0;
                  this.state.streak = 0;
                  this.state.sessionGoal = 0;
            }

            this.broadcast("updateScene", data, { except: client })
      });

      // Score/streak/session goal while a mini-game is being played. Written to `state`
      // (rather than just relayed) so a web client that (re)connects mid-session gets the
      // current values immediately instead of waiting for the next change — see FairState.
      this.onMessage("updatePlayerProgressOnMiniGame", (client, data) => {
            this.state.score = data.score;
            this.state.streak = data.streak;
            this.state.sessionGoal = data.goal;
      });
    }
 /**
  * Admits a client and, once both platforms are present, hands the fair state to the web side.
  *
  * `options.platform` must be `"web"` or `"game"`:
  * - an unknown platform throws, which Colyseus turns into a failed join on the client;
  * - a duplicate platform, or any client arriving after the pair is complete, is accepted by
  *   the transport and then dropped with `client.leave()`.
  *
  * When the pair completes, `"gamejoined"` is sent to every client except the headset,
  * carrying the current {@link FairState}. That message is what flips the React client from
  * the waiting screen to the game screen.
  */
 onJoin(client: Client, options: any) {
   if(this.allPlatformsentered) {
      console.warn(`Rejecting client ${client.sessionId} because all platforms have already joined.`);
      client.leave();
      return;
   }

    if (!this.isPlatformValid(options.platform)) {
       throw new Error(`Invalid platform: ${options.platform}`);
    }

    var clientPlatform : string = options.platform;

    if (this.clientsEntered[clientPlatform]) {
      console.warn(`Rejecting client ${client.sessionId} because platform ${clientPlatform} has already joined.`);
      client.leave();
      return;
    }

    this.clientsEntered[clientPlatform] = true;

    if (clientPlatform === "game") {
      this.gameClientId = client.sessionId;
    }

    this.state = new FairState();
    this.message = "Seleciona a ordem das tendas";

    console.log(`Client ${client.sessionId} joined with options:`, options);

    if (Object.values(this.clientsEntered).every(client => client === true)) {
      this.allPlatformsentered = true;
      console.log('All platforms have joined. Streaming the game state to web device');

      this.clients.forEach((client) => {
        if (client.sessionId !== this.gameClientId) {
          console.log(`Sending game state to client ${client.sessionId}`);
          client.send("gamejoined", this.state);
        }
      });
    }
   }
}
