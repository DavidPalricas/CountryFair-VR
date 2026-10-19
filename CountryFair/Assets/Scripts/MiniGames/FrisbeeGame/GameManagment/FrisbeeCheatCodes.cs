using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Implements <c>miss</c>/<c>score</c> for the frisbee and adds the <c>dog</c> cheat (score in the dog's catch area).
/// </summary>
public class FrisbeeCheatCodes : MiniGameCheatCodes
{   
    /// <summary>
    /// Raised with the world position of the chosen score area; wired to <see cref="OnPlayerFront.ForceScorePoint"/>.
    /// </summary>
    [Header("Frisbee Cheat Code Stuff")]

    [SerializeField]
    private UnityEvent <Vector3> _forceScorePoint;

    /// <summary>
    /// Raised by the <c>miss</c> cheat; wired to <see cref="OnPlayerFront.ThrowFrisbee"/> with a fixed right-hand
    /// argument, so it only throws if the frisbee was last grabbed with the right hand.
    /// </summary>
    [SerializeField]
    private UnityEvent _throwFrisbee;


    /// <summary>Dog's catch area, found by tag on the first <c>dog</c> cheat and cached afterwards.</summary>
    private Transform _dogScoreAreaTransform = null;

    /// <summary>Gates the <c>miss</c> and <c>score</c> cheats; they do nothing while false.</summary>
    /// <remarks>
    /// Only changed by the dog's idle event <c>DogIdle._readyToCatchChanged</c>, wired in the Inspector 
    /// : true when the dog reaches its target and enters
    /// <see cref="DogIdle"/>, false when it leaves that state.
    /// </remarks>
    public bool IsDogReadyToCatch { get; set; } = false;

    /// <summary>
    /// Registers the shared cheats and the <c>dog</c> cheat, and reads whether the Frisbee tutorial is completed.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        
        // Register frisbee-specific cheat
        RegisterCheat("dog", () => ForceScorePoint(true));

        _tutorialCompleted = GameManager.GetInstance().FrisbeeTutorialCompleted;

    }

    /// <summary>
    /// Throws the frisbee from where it is (as a right-hand release, see <see cref="_throwFrisbee"/>); it falls short and
    /// counts as a miss. Requires <see cref="IsDogReadyToCatch"/>.
    /// </summary>
    protected override void OnMissCheat()
    {   
        if (IsDogReadyToCatch)
        {
            _throwFrisbee.Invoke();
        }
     
    }

    /// <summary>
    /// Teleports the frisbee to a random extra score area, forcing a score point. Requires <see cref="IsDogReadyToCatch"/>.
    /// </summary>
    protected override void OnScoreCheat()
    {   
        if (IsDogReadyToCatch)
        {
            ForceScorePoint(false);
        }
    }

    /// <summary>
    /// Picks the target score area and raises <see cref="_forceScorePoint"/>. The move and the "ForcedPoint"
    /// transition happen in <see cref="OnPlayerFront.ForceScorePoint"/>, which ignores the request unless the frisbee
    /// is waiting in front of the player and the dog is ready to catch.
    /// </summary>
    /// <param name="isDog">True for the dog's catch area, false for a random extra score area.</param>
    private void ForceScorePoint(bool isDog)
    {    
        Vector3 targetPosition;

        if (isDog)
        {   
            targetPosition = _dogScoreAreaTransform  == null ? GetDogSocreAreaPosition() : _dogScoreAreaTransform .position;
            

            if (_dogScoreAreaTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("Dog ScoreArea is not yet active, but the frisbee is not in the OnPlayerFront state. The frisbee will be moved, but no point will be scored.");
            }
        }
        else
        {
            targetPosition = GetExtraScoreAreaPosition();
        }

        _forceScorePoint.Invoke(targetPosition);
    }


    /// <summary>
    /// Finds the <c>ScoreArea</c>-tagged object of type <see cref="ScoreAreaProperties.AreaType.Dog"/>, caches it in
    /// <see cref="_dogScoreAreaTransform"/> and returns its position, or <see cref="Vector3.zero"/> if none is found.
    /// </summary>
    /// <remarks>Tag lookups skip inactive objects, so this only succeeds while the dog is idle and its area is shown.</remarks>
    private Vector3 GetDogSocreAreaPosition()
    {
        GameObject dogScoreArea = GameObject.FindGameObjectsWithTag(ScoreAreaProperties.ScoreAreaTag)
                                  .Where(scoreArea => scoreArea.GetComponent<ScoreAreaProperties>().GetAreaType() == ScoreAreaProperties.AreaType.Dog)
                                  .FirstOrDefault();

        if (dogScoreArea == null)
        {
            Debug.LogError("Dog ScoreArea object not found in the scene.");
            return Vector3.zero;
        }

        _dogScoreAreaTransform = dogScoreArea.transform;

        return dogScoreArea.transform.position;
    }

    /// <summary>Position of a random <c>ScoreArea</c>-tagged object other than the dog's catch area, or <see cref="Vector3.zero"/> if none exist.</summary>
    private Vector3 GetExtraScoreAreaPosition()
    {
        GameObject[] scoreAreas = GameObject.FindGameObjectsWithTag(ScoreAreaProperties.ScoreAreaTag)
                                  .Where(scoreArea => scoreArea.GetComponent<ScoreAreaProperties>().GetAreaType() == ScoreAreaProperties.AreaType.Normal)
                                  .ToArray();

        if (scoreAreas.Length == 0)
        {
            Debug.LogWarning("No Extra ScoreArea objects found in the game, probably using the cheat code when the dog is not yet active");
            return Vector3.zero;
        }

        return scoreAreas[Utils.RandomValueInRange(0, scoreAreas.Length)].transform.position;
    }
}