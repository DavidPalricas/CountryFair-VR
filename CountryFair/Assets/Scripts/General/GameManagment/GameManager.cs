using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Singleton that stores cross-scene session state flags for the current play session.
/// All flags default to false and are never persisted to disk; they reset when the process restarts.
/// </summary>
public class GameManager 
{
    private static GameManager _instance;

    /// <summary>Name of the hub scene, loaded when a mini-game session ends.</summary>
    public const string HubScene = "CountryFair";
    /// <summary>Name of the Archery mini-game scene.</summary>
    public const string ArcheryScene = "ArcheryGame";
    /// <summary>Name of the Frisbee mini-game scene.</summary>
    public const string FrisbeeScene = "FrisbeeGame";

    /// <summary>Tag of the floor colliders; a projectile touching it counts as landed/missed in both mini-games.</summary>
    public const string GroundTag = "Ground";
    /// <summary>Tag of the trigger volumes bounding the play area; a projectile entering one counts as missed.</summary>
    public const string OutOfBoundsTag = "OutOfBounds";
    /// <summary>Tag of the object holding each mini-game's <see cref="MiniGameManager"/>.</summary>
    public const string GameManagerTag = "GameManager";

    /// <summary>True once the hub-world intro dialogue sequence has been completed.</summary>
    public bool IntroCompleted { get; set; } = false;
    /// <summary>True once the Frisbee mini-game tutorial has been completed.</summary>
    public bool FrisbeeTutorialCompleted { get; set; } = false;
    /// <summary>True once the Archery mini-game tutorial has been completed.</summary>
    public bool ArcheryTutorialCompleted { get; set; } = false;
    /// <summary>True when returning from the Frisbee mini-game so the hub shows the session-complete dialogue.</summary>
    public bool FrisbeeSessionCompleted { get; set; } = false;
    /// <summary>True when returning from the Archery mini-game so the hub shows the session-complete dialogue.</summary>
    public bool ArcherySessionCompleted { get; set; } = false;

    private GameManager() { }

    /// <summary>Returns the singleton instance, creating it on first call.</summary>
    public static GameManager GetInstance()
    {
        _instance ??= new GameManager();

        return _instance;
    }


    /// <summary>
    /// Mini-game of the active scene, matched by exact scene name against <see cref="ArcheryScene"/> and
    /// <see cref="FrisbeeScene"/>.
    /// </summary>
    /// <returns>The running mini-game, or null in the hub or any other scene.</returns>
    public static OrderableTentElement.MINI_GAMES? GetCurrentMiniGame()
    {
        return SceneManager.GetActiveScene().name switch
        {
            ArcheryScene => OrderableTentElement.MINI_GAMES.ARCHERY,
            FrisbeeScene => OrderableTentElement.MINI_GAMES.FRISBEE,
            _ => null
        };
    }

    /// <summary>
    /// Loads the scene for <paramref name="miniGame"/> and notifies <see cref="ConnectToWebApp"/> of the scene
    /// change. Only <see cref="OrderableTentElement.MINI_GAMES.ARCHERY"/> and
    /// <see cref="OrderableTentElement.MINI_GAMES.FRISBEE"/> have a scene implemented; other values log a warning
    /// and are ignored.
    /// </summary>
    /// <param name="miniGame">The mini-game to load.</param>
    public void GoToMiniGame(OrderableTentElement.MINI_GAMES miniGame)
    {

        string sceneName;

        switch (miniGame)
        {
            case OrderableTentElement.MINI_GAMES.ARCHERY:
               sceneName = ArcheryScene;
                break;

            case OrderableTentElement.MINI_GAMES.FRISBEE:
                sceneName = FrisbeeScene;
                break;

            default:
                Debug.LogWarning($"MiniGame '{miniGame}' is not implemented yet.");
                return;
        }

        SceneManager.LoadScene(sceneName);
        ConnectToWebApp.Instance.UpdateScene(sceneName.ToLower());
    }
}