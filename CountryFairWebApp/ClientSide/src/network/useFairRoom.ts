import { useEffect, useReducer } from "react";
import { getStateCallbacks } from "@colyseus/sdk";
import { subscribeRoom, useConnectionStatus, type ConnectionStatus } from "./client";
import { fairReducer, initialFairPhase, type FairPhaseState, type SceneSnapshot } from "../screens/fairPhase";

/**
 * Connects the room to `fairReducer`: every message and synced field the web client cares
 * about becomes a `FairAction`, and the decisions about what they mean live in the reducer.
 *
 * `subscribeRoom` runs the setup on every room this page joins — the first one, and each
 * fresh one that replaces a room lost for good. A brief drop the SDK resumes keeps the same
 * room, so none of it re-runs and the phase simply carries on.
 * No leave() on cleanup: the room is a module-level singleton and is meant to survive
 * re-mounts. Leaving here would drop the seat the server reserved for this platform.
 */
export function useFairRoom(): FairPhaseState & { connectionStatus: ConnectionStatus } {
  const [state, dispatch] = useReducer(fairReducer, initialFairPhase);
  const connectionStatus = useConnectionStatus();

  useEffect(() => subscribeRoom((room) => {
    console.log("Connected to room:", room.name);
    dispatch({ type: "roomJoined" });

    const $ = getStateCallbacks(room);
    const unsubscribers = [
      $(room.state).listen("score", (score: number) => {
        dispatch({ type: "progressChanged", progress: { score } });
      }),
      $(room.state).listen("streak", (streak: number) => {
        dispatch({ type: "progressChanged", progress: { streak } });
      }),
      $(room.state).listen("sessionGoal", (sessionGoal: number) => {
        dispatch({ type: "progressChanged", progress: { sessionGoal } });
      }),

      room.onMessage("gamejoined", () => {
        console.log("Received gamejoined message");
        // Read from the synced state rather than the message payload: it is what the room
        // remembers of the scene/dialogue/tutorial, so joining mid-session lands on the right screen.
        const { sceneName, dialogueActive, tutorialActive } = room.state;
        dispatch({ type: "gameJoined", snapshot: { sceneName, dialogueActive, tutorialActive } });
      }),
      // The game's own report of where it is, sent right after it (re)joins. Arrives after
      // "gamejoined", so it corrects whatever the room remembered from before.
      room.onMessage("syncGameState", (snapshot: SceneSnapshot) => {
        console.log("Received syncGameState message:", snapshot);
        dispatch({ type: "gameJoined", snapshot });
      }),
      room.onMessage("gameDisconnected", () => {
        console.log("Received gameDisconnected message");
        dispatch({ type: "gameDisconnected" });
      }),
      room.onMessage("playerFinishedDialogue", () => {
        console.log("Received playerFinishedDialogue message");
        dispatch({ type: "dialogueFinished" });
      }),
      room.onMessage("playerFinishedTutorial", () => {
        console.log("Received playerFinishedTutorial message");
        dispatch({ type: "tutorialFinished" });
      }),
      room.onMessage("updateScene", (data: { sceneName: string; dialogueActive?: boolean }) => {
        console.log("Received updateScene message:", data);
        dispatch({ type: "sceneChanged", sceneName: data.sceneName, dialogueActive: data.dialogueActive });
      }),
    ];

    return () => unsubscribers.forEach((unsubscribe) => unsubscribe());
  }), []);

  return { ...state, connectionStatus };
}
