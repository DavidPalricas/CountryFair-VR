using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Represents the idle waiting state of the dog in the Frisbee game.
/// The dog remains stationary at its target position, faces the player, and waits for the frisbee to be thrown.
/// </summary>
/// <remarks>
/// This state activates the score area (the catch zone where the frisbee can land) and the dog's name tag, spawns the
/// floating score areas, and barks at random intervals while waiting. Transitions are driven by the Animator: the
/// <c>Jump</c> trigger set by <see cref="PlayerScored"/> leads to "FrisbeeLanded", and a non-zero <c>Speed</c> set by
/// <see cref="DiffHasChanged"/> leads to "DiffcultyHasDecreased".
/// </remarks>
public class DogIdle : DogState
{   
    /// <summary>
    /// Reference to the score area GameObject representing the catch zone.
    /// This area is activated when the dog is idle and ready to catch, and deactivated when the dog leaves this state.
    /// </summary>
    /// <remarks>
    /// The score area typically contains a collider or trigger that detects when the frisbee lands within the catchable zone.
    /// </remarks>
    [SerializeField]
    private GameObject _scoreArea;

    
    /// <summary>Dog's name tag, only visible while it is waiting.</summary>
    [SerializeField]
    private GameObject _name;
    
    /// <summary>Minimum seconds between impatient barks while waiting for the throw.</summary>
    [Header("Impatient Timer Settings")]
    [SerializeField]
    private float _minImpatientTime = 10f;
    
    /// <summary>Maximum seconds between impatient barks while waiting for the throw.</summary>
    [SerializeField]
    private float _maxImpatientTime = 15f;

    /// <summary>Raised on <see cref="Enter"/> to spawn the floating score areas around the dog.</summary>
    [Header("Idle Events")]
    [SerializeField]
    private UnityEvent _spawnScoreAreas;

    /// <summary>Raised on <see cref="Exit"/> to remove the floating score areas.</summary>
     [SerializeField]
    private UnityEvent _removeScoreAreas;

    /// <summary>
    /// Event invoked when the dog reaches its target position and enters the idle state.
    /// It notifies the frisbee that the dog is ready to catch (see <see cref="OnPlayerFront.DogReachedTarget"/>).
    /// </summary>
    [SerializeField]
    private UnityEvent _positionReached;


    /// <summary>Absolute <see cref="Time.time"/> of the next impatient bark.</summary>
    private float _impatientTimer = 0f;


    /// <summary>Reset on <see cref="Exit"/> but never set to true; currently unused.</summary>
    private bool _playerScored = false;

    /// <summary>
    /// Runs the base dog setup and hides the score area and name tag until the dog is waiting.
    /// </summary>
    /// <remarks>
    /// The name-tag check tests the inherited <c>name</c> property rather than <see cref="_name"/>, so a missing
    /// name tag is not reported here.
    /// </remarks>
    protected override void Awake()
    {   
        base.Awake();

        if (_scoreArea == null)
        {
            Debug.LogError("DogIdle: Score Area GameObject reference is not assigned in the inspector.");

            return;
        }

        if(name == null)
        {
            Debug.LogError("DogIdle: Name GameObject reference is not assigned in the inspector.");

            return;
        }
 
        _scoreArea.SetActive(false);
        _name.SetActive(false);
    }

    /// <summary>
    /// Adds nothing to <see cref="DogState.LateStart"/>.
    /// </summary>
    public override void LateStart()
    {
        base.LateStart();
    }
    
    /// <summary>
    /// Called when entering the DogIdle state.
    /// Barks, starts the impatience timer, shows the score area and name tag, and raises <see cref="_positionReached"/>
    /// and <see cref="_spawnScoreAreas"/>.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        Bark();

        RestImpatientTimer();

        _scoreArea.SetActive(true);
        _name.SetActive(true);

        _positionReached.Invoke();
 
        _spawnScoreAreas.Invoke();
    }

    /// <summary>
    /// Called every frame while in the DogIdle state.
    /// Waits for the Animator to leave the idle clip, then fires "DiffcultyHasDecreased" if <c>Speed</c> &gt; 0 (difficulty
    /// changed, the dog must reposition) or "FrisbeeLanded" otherwise (the jump trigger was set). Barks when the impatience
    /// timer expires.
    /// </summary>
    public override void Execute()
    {
        base.Execute();
        
        if (IsPlayingNewAnimation())
        {    
            if (animator.GetFloat("Speed") > 0f)
            {
                _fsm.ChangeState("DiffcultyHasDecreased");   

                return;
            }
            
            _fsm.ChangeState("FrisbeeLanded");      
            return;
        }


        if (Time.time >= _impatientTimer)
        {
             Bark();
            RestImpatientTimer();
        }
    }

    /// <summary>
    /// Starts the catch when the frisbee lands in a score area: sets the <c>Jump</c> trigger (and <c>Speed</c> 0) so
    /// <see cref="Execute"/> fires "FrisbeeLanded" once the jump clip starts. Ignored if the dog is not idle.
    /// </summary>
    /// <remarks>Invoked via Inspector (FrisbeeGame scene) by the frisbee's scoring event.</remarks>
    public void PlayerScored()
    {   
        if (_fsm.CurrentState == this)
        {
            animator.SetTrigger("Jump");
            animator.SetFloat("Speed", 0f);
        }
    }

    /// <summary>
    /// Makes an idle dog start walking (<c>Speed</c> 1) so <see cref="Execute"/> fires "DiffcultyHasDecreased" and the dog
    /// moves to a new distance. Skipped while the Animator's <c>StopAnim</c> flag is set.
    /// </summary>
    /// <remarks>Invoked via Inspector (FrisbeeGame scene) when the difficulty changes.</remarks>
    public void DiffHasChanged()
    {    
        if (_fsm.CurrentState == this && !animator.GetBool("StopAnim"))
        {
            animator.SetFloat("Speed", 1f);
        } 
    }

    /// <summary>
    /// Called when exiting the DogIdle state.
    /// Hides the name tag and score area and raises <see cref="_removeScoreAreas"/>, so no landing is detected while
    /// the dog is moving or returning.
    /// </summary>
    public override void Exit()
    {
        base.Exit();

        _name.SetActive(false);
        _scoreArea.SetActive(false);

        _removeScoreAreas.Invoke();

        _playerScored = false;
    }

    /// <summary>Schedules the next impatient bark at a random time between the min and max.</summary>
    private void RestImpatientTimer()
    {
        _impatientTimer = Utils.RandomValueInRange(_minImpatientTime, _maxImpatientTime) + Time.time;
    }
}
