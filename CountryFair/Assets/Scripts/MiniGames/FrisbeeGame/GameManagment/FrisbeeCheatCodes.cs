using UnityEngine;

/// <summary>
/// Manages cheat code detection and activation for the Frisbee mini-game.
/// Listens for keyboard input and triggers specific actions when valid cheat codes are entered.
/// </summary>
public class FrisbeeCheatCodes : CheatCodes
{   
    /// <summary>
    /// Reference to the frisbee's OnPlayerFront state component.
    /// </summary>
    private OnPlayerFront _frisbeePlayerFrontState;

    /// <summary>
    /// Reference to the frisbee's Landed state component.
    /// </summary>
    private Landed _frisbeeLandedState = null;

    /// <summary>
    /// Transform of the score area where points are registered.
    /// </summary>
     private Transform _scoreAreaTransform = null;

    /// <summary>
    /// Transform of the frisbee game object.
    /// </summary>
     private Transform _frisbeeTransform = null;

    /// <summary>
    /// Finite State Machine component controlling the frisbee's behavior.
    /// </summary>
     private FSM _frisbeeFSM = null;

    /// <summary>
    /// Initializes component references and calculates maximum cheat code length.
    /// Finds and caches references to the Frisbee and ScoreArea game objects and their components.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Start()
    {   
        base.Start();

        GameObject frisbee = GameObject.FindGameObjectWithTag("Frisbee");

        if (frisbee == null)
        {
            Debug.LogError("Frisbee GameObject not found.");
            return;
        }

        _frisbeeTransform = frisbee.transform;


        _frisbeeFSM = frisbee.GetComponent<FSM>();

        if (_frisbeeFSM == null)
        {
            Debug.LogError("FSM component not found on Frisbee GameObject.");

            return;
        }

        _frisbeePlayerFrontState = frisbee.GetComponent<OnPlayerFront>();

        if (_frisbeePlayerFrontState == null)
        {
            Debug.LogError("OnPlayerFront component not found.");

            return;
        }

        _frisbeeLandedState = frisbee.GetComponent<Landed>();

        if (_frisbeeLandedState == null)
        {
            Debug.LogError("Landed component not found.");

            return;
        }

        GameObject scoreArea = GameObject.FindGameObjectWithTag("ScoreArea");


        if (scoreArea == null)
        {
            Debug.LogError("ScoreArea GameObject not found.");
            return;
        }

        _scoreAreaTransform = scoreArea.transform;
    }

    /// <summary>
    /// Activates the specified cheat code and clears the input buffer.
    /// Executes different actions based on the cheat code provided.
    /// </summary>
    /// <param name="cheatCode">The cheat code string to activate.</param>
    protected override void ActivateCheat(string cheatCode){
       base.ActivateCheat(cheatCode);

        switch (cheatCode)
        {
            case "throw":
                _frisbeePlayerFrontState.ThrowFrisbee(false);
                break;

            case "miss":
                  ForceScorePoint();      
                break;
            default:
                Debug.LogError("Invalid cheat code: " + cheatCode);
                break;
        }
    }

    /// <summary>
    /// Forces the frisbee to score a point by teleporting it to the score area.
    /// Sets the frisbee as a child of the score area and triggers the scoring state.
    /// Only works if the frisbee is in the OnPlayerFront state and the score area is active.
    /// </summary>
    private void ForceScorePoint()
    {
        _frisbeeTransform.parent = _scoreAreaTransform;
        _frisbeeTransform.localPosition = Vector3.zero;
        
        if (_frisbeeFSM.CurrentState == _frisbeePlayerFrontState && _scoreAreaTransform.gameObject.activeSelf)
        {
            _frisbeeFSM.ChangeState("ForcedPoint");
            return;
        }
    }
}
