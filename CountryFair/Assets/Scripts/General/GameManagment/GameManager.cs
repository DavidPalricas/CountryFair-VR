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


    public void GoToMiniGame(OrderableTentElement.MINI_GAMES miniGame)
    {
        switch (miniGame)
        {
            case OrderableTentElement.MINI_GAMES.ARCHERY:
                SceneManager.LoadScene("ArcheryGame");
                return;

            case OrderableTentElement.MINI_GAMES.FRISBEE:
                SceneManager.LoadScene("FrisbeeGame");
                return;

            default:
                Debug.LogWarning($"MiniGame '{miniGame}' is not implemented yet.");
                return;
        }
    }
}