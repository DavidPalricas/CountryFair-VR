import { useEffect, useReducer } from "react";
import { getStateCallbacks } from "@colyseus/sdk";
import { subscribeRoom, useConnectionStatus, type ConnectionStatus } from "./client";
import { fairReducer, initialFairPhase, type FairPhaseState } from "../screens/fairPhase";

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
        dispatch({ type: "gameJoined" });
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
      room.onMessage("updateScene", (data: { sceneName: string }) => {
        console.log("Received updateScene message:", data.sceneName);
        dispatch({ type: "sceneChanged", sceneName: data.sceneName });
      }),
    ];

    return () => unsubscribers.forEach((unsubscribe) => unsubscribe());
  }), []);

  return { ...state, connectionStatus };
}
