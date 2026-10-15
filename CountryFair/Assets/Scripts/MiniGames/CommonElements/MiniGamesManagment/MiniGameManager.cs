using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all mini-game managers.
/// Provides the contract for spawning/removing targets, applying difficulty settings,
/// and tracking the current difficulty level via PlayerPrefs.
/// Concrete implementations: <see cref="FrisbeeGameManager"/>, <see cref="ArcheryGameManager"/>.
/// </summary>
[RequireComponent(typeof(MiniGameCheatCodes))]
public class MiniGameManager : MonoBehaviour
{
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

    /// <summary>Live target instances, oldest first.</summary>
    protected readonly List<GameObject> _spawnedTargets = new();

    /// <summary>
    /// Target count the current difficulty asks for. Must be kept up to date: <c>DestroyTarget</c> compares against it
    /// to decide whether a destroyed target should be replaced.
    /// </summary>
    protected int _currentDesiredCount;

    /// <summary>Publishes the session goal in the <c>SessionGoal</c> PlayerPrefs key read by the score system.</summary>
    protected virtual void Awake()
    {
       PlayerPrefs.SetInt("SessionGoal", _sessionScoreGoal);
    }


    /// <summary>Moves one level up or down and re-applies the settings. Wired from <see cref="CarnyWise"/>'s <c>_changeDifficulty</c> event.</summary>
    public virtual void ChangeDifficulty(bool isToIncreaseDiff){
        Debug.LogError("ChangeDifficulty should be overridden in derived classes.");
    }

    protected virtual void ApplyDifficultySettings(){
        Debug.LogError("ApplyDifficultySettings should be overridden in derived classes.");
    }

    protected virtual Vector3 GetRandomTargetPosition()
    {
        Debug.LogError("GetRandomTargetPosition should be overridden in derived classes.");
        return Vector3.zero;
    }

    protected virtual void SyncTargets(int desiredCount){
        Debug.LogError("SyncTargets should be overridden in derived classes.");
    }

    protected virtual void UpdateTargetsProperties()
    {
        Debug.LogError("UpdateTargetsProperties should be overridden in derived classes.");
    }

    protected virtual void AddTarget(GameObject targetPrefab = null)
    {
        Debug.LogError("AddTarget should be overridden in derived classes.");
    }

    protected virtual void RemoveTarget()
    {
        Debug.LogError("RemoveTarget should be overridden in derived classes.");
    }

    public virtual void DestroyTarget(GameObject target, GameObject targetPrefab = null)
    {
        Debug.LogError("DestroyTarget should be overridden in derived classes.");
    }

    /// <summary>
    /// Called by the Tutorial system when the tutorial is fully complete.
    /// Implementations should read the persisted difficulty level and apply initial settings.
    /// </summary>
    /// <remarks>Invoked via Inspector by the <c>_tutorialCompleted</c> event of <see cref="Tutorial"/>.</remarks>
    public virtual void TutorialCompleted(){
        Debug.LogError("TutorialCompleted should be overridden in derived classes.");
    }

    
    /// <summary>Sets the level to 0 and re-applies the settings (the <c>reset</c> cheat).</summary>
    public virtual void ResetDifficulty()
    {   
        DifficultyLevel = 0;
        ApplyDifficultySettings();
    }
}
