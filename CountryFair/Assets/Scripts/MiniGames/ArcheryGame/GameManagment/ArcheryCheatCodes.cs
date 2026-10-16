using UnityEngine;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Archery-specific cheat code handler. Extends <see cref="MiniGameCheatCodes"/> with miss/score
/// cheats that operate on the <see cref="Arrow"/> component — launching the arrow with zero force
/// (miss) or teleporting it onto the correct balloon (score).
/// </summary>
public class ArcheryCheatCodes : MiniGameCheatCodes
{
    /// <summary>
    /// Launch force used by the <c>score</c> cheat. It only needs to make the arrow leave the bow, because the arrow is
    /// then teleported onto the balloon.
    /// </summary>
    [Header("Archery Cheat Code Stuff")]
    [SerializeField]
    private float _launchForce = 5f;

    /// <summary>
    /// Raised with (force, optional target position) to fire the arrow. Wired in the Inspector to <see cref="Arrow.Launch"/>.
    /// </summary>
    [SerializeField]
    private UnityEvent<float, Vector3?> _launchArrow;


    /// <summary>
    /// Registers the shared cheats and reads whether the archery tutorial is already completed,
    /// which gates every cheat except <c>return</c> and <c>tutorial</c>.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _tutorialCompleted = GameManager.GetInstance().ArcheryTutorialCompleted;
    }

    /// <summary>
    /// Launches the arrow with no force, simulating a miss.
    /// </summary>
    protected override void OnMissCheat()
    {
        _launchArrow.Invoke(0f, null);
    }

    /// <summary>
    /// Launches the arrow and teleports it to the correct balloon position, guaranteeing a score.
    /// </summary>
    protected override void OnScoreCheat()
    {
        Vector3 targetPosition = GetCorrectBalloonPosition();

        _launchArrow.Invoke(_launchForce, targetPosition);
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