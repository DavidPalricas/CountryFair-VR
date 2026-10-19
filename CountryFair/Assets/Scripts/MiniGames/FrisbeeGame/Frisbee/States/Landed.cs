using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee after it has landed and stopped on the ground.
/// Decides whether the throw scored (by overlapping a <c>ScoreArea</c>) and raises the matching event.
/// After a scored throw outside the tutorial it waits for the dog to bring it back ("RetrievedByDog"); otherwise it
/// returns straight to the player ("ReturnToPlayer").
/// </summary>
public class Landed: FrisbeeState
{
    /// <summary>FSM transition fired when the dog picks up the landed frisbee.</summary>
    private const string RetrievedByDogTransition = "RetrievedByDog";

    /// <summary>Frisbee mesh; its bounds give the radius used to detect an overlapping score area.</summary>
    [SerializeField]
    private MeshRenderer _frisbeeMeshRenderer;

    /// <summary>Event invoked with the score area's points when the frisbee lands in a score area outside the tutorial.
    /// </summary>
    /// <remarks>
    /// Wired to the score system, the DDA and the dog (<see cref="DogIdle.PlayerScored"/>, <see cref="GoToTarget.IsToChooseNewTarget"/>),
    /// which then picks a new position.
    /// </remarks>
    [Header("Landed Events")]

    [SerializeField]
    private UnityEvent <int> _playerScored;
    
    /// <summary>Wired to the audio manager to play the point-scored sound.</summary>
    [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects> _scoreSoundEffectEvent;

    /// <summary>Raised when the frisbee lands outside every score area.</summary>
    [SerializeField]
    private UnityEvent _playerMissed;

    /// <summary>
    /// Sound sent through <see cref="_scoreSoundEffectEvent"/> when the frisbee lands in a score area, also during the
    /// tutorial (fixed; not editable in the Inspector).
    /// </summary>
    private readonly AudioManager.GameSoundEffects _scoreSoundEffect = AudioManager.GameSoundEffects.PointScored;

    /// <summary>Points of the score area the frisbee landed in, read from <see cref="ScoreAreaProperties.ScorePoints"/>.</summary>
    private int _scorePoints = 1;

    /// <summary>
    /// Initializes the state by setting up physics component references and validating the mesh renderer.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();

        if (_frisbeeMeshRenderer == null)
        {
            Debug.LogError("MeshRenderer component is not assigned in Landed script.");
        }
    }


    /// <summary>No extra setup for this state; only runs the base <see cref="State.LateStart"/>.</summary>
    public override void LateStart()
    {
        base.LateStart();
    }

    /// <summary>
    /// Called when entering the Landed state.
    /// Stops the frisbee and resolves the throw.
    /// </summary>
    /// <remarks>
    /// In a score area during the tutorial: "ReturnToPlayer" immediately. In a score area outside the tutorial: raises
    /// <see cref="_playerScored"/> and waits here until the dog brings it back (<see cref="FrisbeeGivenByDog"/>).
    /// Outside any score area: raises <see cref="_playerMissed"/> and fires "ReturnToPlayer".
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        _rigidbody.linearVelocity = Vector3.zero;

        if (FrisbeeOnScoreArea())
        {
            if (TutorialActive)
            {
                _fsm.ChangeState(ReturnToPlayerTransition);

                return;
            }

            _playerScored.Invoke(_scorePoints);

            return;
        }

        _playerMissed.Invoke();

        _fsm.ChangeState(ReturnToPlayerTransition);
    }

    /// <summary>
    /// Called every frame while in the Landed state.
    /// Currently no per-frame logic needed while waiting for dog retrieval.
    /// </summary>
    public override void Execute()
    {
         base.Execute();
    }
    
    /// <summary>
    /// Called when exiting the Landed state, either on "ReturnToPlayer" or when the dog retrieves the frisbee.
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }

    /// <summary>
    /// Called by the event <see cref="GiveFrisbeeToPlayer._frisbeeGivenToPlayer"/> when the dog gives the frisbee back to the player.
    /// Triggers the transition "RetrievedByDog" to the <see cref="OnPlayerFront"/> state.
    /// </summary>
    public void FrisbeeGivenByDog()
    {   
        if (_fsm.CurrentState == this)
        {
            _fsm.ChangeState(RetrievedByDogTransition);
        }
    }

    /// <summary>
    /// True if a <c>ScoreArea</c>-tagged collider overlaps the frisbee; in that case also plays the area's score
    /// animation and sound and stores its points in <see cref="_scorePoints"/>.
    /// </summary>
    /// <remarks>Only the first 10 overlapping colliders are inspected.</remarks>
    private bool FrisbeeOnScoreArea()
    {   
        float frisbeeRadius = _frisbeeMeshRenderer.bounds.extents.magnitude;

        Collider[] overlapResults = new Collider[10];

        Physics.OverlapSphereNonAlloc(transform.position, frisbeeRadius, overlapResults);

        foreach (Collider collider in overlapResults)
        {
            if (collider != null && collider.gameObject.CompareTag(ScoreAreaProperties.ScoreAreaTag))
            {    
                GameObject scoreArea = collider.gameObject;

                if (!scoreArea.TryGetComponent<ScoreAreaAnimations>(out var scoreAreaAnimations))
                {
                    Debug.LogError("ScoreAreaAnimations component is missing on the Score Area object.");

                    return false;
                }

                if (!scoreArea.TryGetComponent<ScoreAreaProperties>(out var scoreAreaProperties))
                {
                    Debug.LogError("ScoreAreaProperties component is missing on the Score Area object.");

                    return false;
                }

                scoreAreaAnimations.ScoreAnimation();
                _scoreSoundEffectEvent.Invoke(_scoreSoundEffect);

                _scorePoints = scoreAreaProperties.ScorePoints;

                return true;
            }
        }

        return false;
    }
}