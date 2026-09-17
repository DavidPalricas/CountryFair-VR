using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Singleton that stores cross-scene session state flags for the current play session.
/// All flags default to false and are never persisted to disk; they reset when the process restarts.
/// </summary>
public class GameManager 
{
    private static GameManager s_instance;

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
        s_instance ??= new GameManager();

        return s_instance;
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
               sceneName = "ArcheryGame";
                break;

            case OrderableTentElement.MINI_GAMES.FRISBEE:
                sceneName = "FrisbeeGame";
                break;

            default:
                Debug.LogWarning($"MiniGame '{miniGame}' is not implemented yet.");
                return;
        }

        SceneManager.LoadScene(sceneName);
        ConnectToWebApp.Instance.UpdateScene(sceneName.ToLower());
    }
}