using System.Linq;
using UnityEngine;

/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Implements <c>miss</c>/<c>score</c> for the frisbee and adds the <c>dog</c> cheat (score in the dog's catch area).
/// </summary>
public class FrisbeeCheatCodes : MiniGameCheatCodes
{   
    /// <summary>The dog's catch area; target of the <c>dog</c> cheat and excluded when picking an extra score area.</summary>
    [Header ("Dog Dependencies")]
    [SerializeField]
    private Transform _dogScoreAreaTransform;

    /// <summary>Dog idle state; only validated here.</summary>
    [SerializeField]
    private DogIdle _dogIdleState;
    
    /// <summary>
    /// Reference to the frisbee's OnPlayerFront state component.
    /// </summary>
    [Header("Frisbee Dependencies")]

    [SerializeField]
    private OnPlayerFront _frisbeePlayerFrontState;
    
    /// <summary>
    /// Reference to the frisbee's Landed state component.
    /// </summary>
    [SerializeField]
    private Landed _frisbeeLandedState = null;
     
    /// <summary>
    /// Transform of the frisbee game object.
    /// </summary>
    [SerializeField]
    private Transform _frisbeeTransform = null;
    
    /// <summary>
    /// Finite State Machine component controlling the frisbee's behavior.
    /// </summary>
    [SerializeField]
    private FSM _frisbeeFSM = null;

    /// <summary>
    /// Validates the references, registers the <c>dog</c> cheat and reads whether the Frisbee tutorial is completed.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        if (_frisbeeFSM == null)
        {
            Debug.LogError("FSM component not found on Frisbee GameObject.");
            return;
        }

        if (_dogIdleState == null)
        {
            Debug.LogError("DogIdle component reference not assigned in the inspector.");
            return;
        }

        if (_frisbeePlayerFrontState == null)
        {
            Debug.LogError("OnPlayerFront component not found.");
            return;
        }

        if (_frisbeeLandedState == null)
        {
            Debug.LogError("Landed component not found.");
            return;
        }

        if (_dogScoreAreaTransform == null)
        {
            Debug.LogError("DogScoreArea Transform not assigned.");
            return;
        }

        // Register frisbee-specific cheat
        RegisterCheat("dog", () => ForceScorePoint(true));

        _tutorialCompleted = GameManager.GetInstance().FrisbeeTutorialCompleted;
    }

    /// <summary>
    /// Throws the frisbee from where it is (as a left-hand release); it falls short and counts as a miss.
    /// </summary>
    protected override void OnMissCheat()
    {
        _frisbeePlayerFrontState.ThrowFrisbee(false);
    }

    /// <summary>
    /// Teleports the frisbee to a random extra score area, forcing a score point.
    /// </summary>
    protected override void OnScoreCheat()
    {
        ForceScorePoint(false);
    }

    /// <summary>
    /// Forces the frisbee to score a point by teleporting it to a score area.
    /// Unparents the frisbee, moves it onto the area and fires "ForcedPoint".
    /// The transition only fires if the frisbee is in the OnPlayerFront state and the dog's score area is active;
    /// otherwise the frisbee is still moved.
    /// </summary>
    /// <param name="isDog">True for the dog's catch area, false for a random extra score area.</param>
    private void ForceScorePoint(bool isDog)
    {
        _frisbeeTransform.parent = null;
        _frisbeeTransform.localPosition = isDog ? _dogScoreAreaTransform.position : GetExtraScoreAreaPosition();

        if (_frisbeeFSM.CurrentState == _frisbeePlayerFrontState && _dogScoreAreaTransform.gameObject.activeSelf)
        {
            _frisbeeFSM.ChangeState("ForcedPoint");
        }
    }


    /// <summary>Position of a random <c>ScoreArea</c>-tagged object other than the dog's catch area, or <see cref="Vector3.zero"/> if none exist.</summary>
    private Vector3 GetExtraScoreAreaPosition()
    {
        GameObject[] scoreAreas = GameObject.FindGameObjectsWithTag("ScoreArea")
                                  .Where(scoreArea => scoreArea != _dogScoreAreaTransform.gameObject)
                                  .ToArray();

        if (scoreAreas.Length == 0)
        {
            Debug.LogError("No Extra ScoreArea objects found in the scene.");
            return Vector3.zero;
        }

        return scoreAreas[Utils.RandomValueInRange(0, scoreAreas.Length)].transform.position;
    }
}