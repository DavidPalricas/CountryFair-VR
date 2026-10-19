using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all mini-game managers.
/// Provides the contract for spawning/removing targets, applying difficulty settings,
/// and tracking the current difficulty level via PlayerPrefs.
/// Concrete implementations: <see cref="FrisbeeGameManager"/>, <see cref="ArcheryGameManager"/>.
/// </summary>
[RequireComponent(typeof(MiniGameCheatCodes))]
public abstract class MiniGameManager : MonoBehaviour
{
    /// <summary>PlayerPrefs key of the session score goal; read by <see cref="ScoreAndStreakSystem"/>.</summary>
    public const string SessionGoalKey = "SessionGoal";

    /// <summary>Base number of targets present at difficulty level 0.</summary>
    [Header("Targets Configuration")]
    [SerializeField]
    protected int _targetsCount = 3;

    /// <summary>Fractional number of extra targets added per difficulty level.</summary>
    [SerializeField]
    protected float _targetsPerLevel = 1.5f;

    /// <summary>Number of successful actions the player must complete to finish the session.</summary>
    [SerializeField]
    protected int _sessionScoreGoal = 3;

    /// <summary>Current difficulty level, persisted in PlayerPrefs under a game-specific key.</summary>
    public int DifficultyLevel = 0;

    /// <summary>Live target instances, oldest first. Each game takes them from its own <see cref="ObjectPool"/>.</summary>
    protected readonly List<GameObject> _spawnedTargets = new();

    /// <summary>
    /// Target count the current difficulty asks for. Must be kept up to date: <c>RemoveTarget(target)</c> compares against it
    /// to decide whether a removed target should be replaced.
    /// </summary>
    protected int _currentDesiredCount;

    /// <summary>Publishes the session goal in the <c>SessionGoal</c> PlayerPrefs key read by the score system.</summary>
    protected virtual void Awake()
    {
       PlayerPrefs.SetInt(SessionGoalKey, _sessionScoreGoal);
    }


    /// <summary>Moves one level up or down and re-applies the settings. Wired from <see cref="CarnyWise"/>'s <c>_changeDifficulty</c> event.</summary>
    public abstract void ChangeDifficulty(bool isToIncreaseDiff);

    /// <summary>
    /// Derives the game's target count and complexity parameters from <see cref="DifficultyLevel"/>, saves the level and
    /// grows the game's <see cref="ObjectPool"/> to the new target count.
    /// </summary>
    protected abstract void ApplyDifficultySettings();

    /// <summary>Free spawn position for a new target inside the game's spawn area; gives up after a bounded number of attempts.</summary>
    protected abstract Vector3 GetRandomTargetPosition();

    /// <summary>Adds or removes targets until there are <paramref name="desiredCount"/>, then redistributes their properties (<see cref="UpdateTargetsProperties"/>).</summary>
    protected void SyncTargets(int desiredCount)
    {
        while (_spawnedTargets.Count < desiredCount)
        {
            AddTarget();
        }

        while (_spawnedTargets.Count > desiredCount)
        {
            RemoveTarget();
        }

        UpdateTargetsProperties();
    }

    /// <summary>Redistributes the difficulty modifiers (moving, blinking/transparent) among the live targets according to the current ratios.</summary>
    protected abstract void UpdateTargetsProperties();

    /// <summary>Takes a target from the game's <see cref="ObjectPool"/> at a free position and adds it to <see cref="_spawnedTargets"/>.</summary>
    /// <param name="targetPrefab">Specific prefab to spawn (e.g. a forced balloon color); null lets the game choose.</param>
    protected abstract void AddTarget(GameObject targetPrefab = null);

    /// <summary>Removes the oldest target; used by <see cref="SyncTargets"/> when the difficulty lowers the target count.</summary>
    protected abstract void RemoveTarget();

    /// <summary>Removes <paramref name="target"/> (scored or removed) from the game and returns it to the game's <see cref="ObjectPool"/>.</summary>
    public abstract void RemoveTarget(GameObject target, GameObject targetPrefab = null);

    /// <summary>
    /// Called by the Tutorial system when the tutorial is fully complete.
    /// Implementations should read the persisted difficulty level and apply initial settings.
    /// </summary>
    /// <remarks>Invoked via Inspector by the <c>_tutorialCompleted</c> event of <see cref="Tutorial"/>.</remarks>
    public abstract void TutorialCompleted();

    
    /// <summary>Sets the level to 0 and re-applies the settings (the <c>reset</c> cheat).</summary>
    public virtual void ResetDifficulty()
    {   
        DifficultyLevel = 0;
        ApplyDifficultySettings();
    }
}
