using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

/// <summary>
/// Archery-specific cheat code handler. Extends <see cref="MiniGameCheatCodes"/> with miss/score
/// cheats that operate on the <see cref="Arrow"/> component — launching the arrow with zero force
/// (miss) or teleporting it onto the correct balloon (score).
/// </summary>
public class ArcheryCheatCodes : MiniGameCheatCodes
{
    /// <summary>Transform used to reposition the arrow during the score cheat.</summary>
    [Header("Arrow Dependencies")]
    [SerializeField]
    [FormerlySerializedAs("arrowTransform")]
    private Transform _arrowTransform = null;

    /// <summary>Arrow component used to trigger launches via cheat commands.</summary>
    [SerializeField]
    [FormerlySerializedAs("arrowComponent")]
    private Arrow _arrowComponent = null;

    /// <summary>
    /// Validates the arrow references and reads whether the archery tutorial is already completed,
    /// which gates every cheat except <c>return</c> and <c>tutorial</c>.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        if (_arrowComponent == null)
        {
            Debug.LogError("Arrow component reference is not assigned in the inspector.");
            return;
        }

        if (_arrowTransform == null)
        {
            Debug.LogError("Arrow Transform reference is not assigned in the inspector.");
            return;
        }

        _tutorialCompleted = GameManager.GetInstance().ArcheryTutorialCompleted;
    }

    /// <summary>
    /// Launches the arrow with no force, simulating a miss.
    /// </summary>
    protected override void OnMissCheat()
    {
        _arrowComponent.Launch(0f);
    }

    /// <summary>
    /// Launches the arrow and teleports it to the correct balloon position, guaranteeing a score.
    /// </summary>
    protected override void OnScoreCheat()
    {
        // Random value of launch force, to ensure the arrow is not kinematic
        _arrowComponent.Launch(5f);
        _arrowTransform.position = GetCorrectBalloonPosition() + new Vector3(0, 0f, 0);
    }

    /// <summary>
    /// Position of the first balloon whose color matches <c>BalloonColorToScore</c>; also re-enables its collider in case
    /// it is in the faded phase. Returns <see cref="Vector3.zero"/> if none exists.
    /// </summary>
    private Vector3 GetCorrectBalloonPosition()
    {
        string balloonColorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();
        
        BalloonArcheryGame balloon = FindObjectsByType<BalloonArcheryGame>(FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.GetBalloonColorName().ToLower() == balloonColorToScore);

        if (balloon == null)
        {
            Debug.LogError($"No balloon found with color '{balloonColorToScore}'");
            return Vector3.zero;
        }

        Transform targetBalloonTransform = balloon.transform;

        balloon.Collider.enabled = true;

        return targetBalloonTransform.position;
    }
}