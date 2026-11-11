import type { MiniGameType } from "../GameProps/Tents/tentSlots";

/**
 * Name shown on the mini-game overlay (`WaitingScreen`'s `statusText`) while that mini-game is
 * loaded — distinct from `TENT_NAMES` (`tentSlots.ts`), which labels the tents on the fair
 * signboard ("Tenda de Arco e Flecha") rather than the mini-game itself ("Arco e Flecha").
 * Kept complete for all four mini-games even though `classifyScene` below cannot resolve
 * fishing/duckgame yet — see its comment.
 */
export const MINI_GAME_LABELS: Record<MiniGameType, string> = {
    archery: "Arco e Flecha",
    frisbee: "Frisbee",
    fishing: "Pescar",
    duckgame: "Jogo do Pato",
};

/**
 * Lower-cased Unity scene name for each mini-game that actually loads one today, mirroring
 * `GameManager.GoToMiniGame` exactly (`case ARCHERY: sceneName = "ArcheryGame"`, etc.) — the
 * same method whose `SceneManager.LoadScene` makes `ConnectToWebApp.OnSceneLoaded` report it. Fishing and
 * duckgame hit that switch's `default` case ("not implemented yet") and return before ever
 * loading a scene, so there is no real scene name to match for them
 * yet; add their entry here once `GameManager` grows a case for them.
 */
const MINI_GAME_SCENE_NAMES: Partial<Record<MiniGameType, string>> = {
    archery: "archerygame",
    frisbee: "frisbeegame",
};

/** Lower-cased plaza scene name, matching `GameManager.PlazaScene` ("CountryFair"). */
const HUB_SCENE_NAME = "countryfair";

/**
 * Lower-cased session configuration scene name, matching `GameManager.ConfigScene`
 * ("ConfigSession"): where the survivor reorders the mini-game tents, so it is the only scene
 * whose 3D tents the web client shows.
 */
const CONFIG_SCENE_NAME = "configsession";

/**
 * Classifies a Unity scene name — as sent in the room's `"updateScene"` message
 * (`ConnectToWebApp.OnSceneLoaded`) — into the plaza, the session configuration scene or one of the mini-games
 * `GameManager.GoToMiniGame` actually implements. Lower-cases the input before comparing
 * (`GameManager` already sends it lower-cased, but this stays exact-match-safe either way).
 */
export function classifyScene(sceneName: string): "hub" | "config" | MiniGameType | null {
    const name = sceneName.toLowerCase();

    if (name === HUB_SCENE_NAME) return "hub";
    if (name === CONFIG_SCENE_NAME) return "config";

    for (const [miniGame, scene] of Object.entries(MINI_GAME_SCENE_NAMES) as [MiniGameType, string][]) {
        if (name === scene) return miniGame;
    }

    return null;
}
