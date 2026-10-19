using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State where the dog returns to the player with the frisbee in its mouth.
/// Upon reaching the player, the dog gives the frisbee back and transitions based on whether the player scored.
/// </summary>
/// <remarks>
/// This state activates a visual representation of the frisbee in the dog's mouth.
/// When the dog reaches the player, it invokes the <see cref="_frisbeeGivenToPlayer"/> event, whines happily and
/// triggers the "FrisbeeGiven" transition (to <see cref="GoToTarget"/>). On <see cref="Exit"/> it re-enables the
/// real frisbee, which is the only place where the frisbee is reactivated.
/// </remarks>
public class GiveFrisbeeToPlayer : DogState
{
    /// <summary>FSM transition fired once the frisbee has been handed back to the player.</summary>
    private const string FrisbeeGivenTransition = "FrisbeeGiven";

   /// <summary>
   /// Visual representation of the frisbee held in the dog's mouth.
   /// This GameObject is activated when entering the state and deactivated on exit.
   /// </summary>
   [SerializeField]
    private GameObject _frisbeeInDogMouth;

    /// <summary>The real frisbee, re-enabled in <see cref="Exit"/> after the hand-off.</summary>
    [SerializeField]
    private GameObject _frisbee;

    
    /// <summary>
    /// Event invoked when the dog successfully returns the frisbee to the player.
    /// This event is used to trigger the <see cref="Landed.FrisbeeGivenByDog"/> method to notify that the frisbee has been given back.
    /// </summary>
    [Header("Events")]
    [SerializeField]
    private UnityEvent _frisbeeGivenToPlayer;

    /// <summary>Wired to the audio manager to play the happy whine at the dog when the frisbee is returned.</summary>
    [SerializeField]
    private UnityEvent<AudioManager.GameSoundEffects, GameObject> _happyWhine;

    /// <summary>Sound sent through <see cref="_happyWhine"/> when the dog stops in front of the player and hands back the frisbee (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _happyWhineSoundEffect = AudioManager.GameSoundEffects.DogHappyWhine;

    /// <summary>
    /// Initializes the GiveFrisbeeToPlayer state and validates the frisbee reference.
    /// </summary>
    /// <remarks>
    /// Calls base initialization and ensures <see cref="_frisbeeInDogMouth"/> and <see cref="_frisbee"/> are assigned.
    /// Deactivates the in-mouth frisbee so it only appears when the state is entered.
    /// Logs an error if the frisbee reference is not set in the Inspector.
    /// </remarks>
    protected override void Awake()
    {  
        base.Awake();

        if(_frisbeeInDogMouth == null)
        {
            Debug.LogError("Frisbee reference not set in ChooseNewPosition state.");

            return;
        }

        if (_frisbee == null)
        {
            Debug.LogError("Frisbee reference not set in ChooseNewPosition state.");
            return;
        }

        _frisbeeInDogMouth.SetActive(false);    
    }

    /// <summary>
    /// Adds nothing to <see cref="DogState.LateStart"/>.
    /// </summary>
    public override void LateStart()
    {    
        base.LateStart();
    }

    /// <summary>
    /// Called when entering the GiveFrisbeeToPlayer state.
    /// Activates the frisbee visual in the dog's mouth and sets navigation to the player.
    /// </summary>
    /// <remarks>
    /// Makes the frisbee GameObject visible in the dog's mouth and instructs the NavMeshAgent
    /// to navigate to the player's current position.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        _frisbeeInDogMouth.SetActive(true);

        _agent.SetDestination(_playerTransform.position);

        RotateDogTowardsTarget(_playerTransform);
   }

    /// <summary>
    /// Called every frame while in the GiveFrisbeeToPlayer state.
    /// Checks whether the dog has reached the player.
    /// </summary>
    /// <remarks>
    /// When the dog reaches the player:
    /// <list type="number">
    /// <item><description>Invokes the <see cref="_frisbeeGivenToPlayer"/> event</description></item>
    /// <item><description>Plays the happy whine</description></item>
    /// <item><description>Triggers the "FrisbeeGiven" transition</description></item>
    /// </list>
    /// </remarks>
    public override void Execute()
    {
         base.Execute();

         if (_agent.HasReachedDestination())
         {  
            _frisbeeGivenToPlayer.Invoke();

            _happyWhine.Invoke(_happyWhineSoundEffect, gameObject);

            _fsm.ChangeState(FrisbeeGivenTransition);
         }
    }

    /// <summary>
    /// Called when exiting the GiveFrisbeeToPlayer state.
    /// Hides the in-mouth frisbee and re-enables the real frisbee in front of the player.
    /// </summary>
    public override void Exit()
    {
        base.Exit();

        _frisbeeInDogMouth.SetActive(false);
        _frisbee.SetActive(true);
    }
}
