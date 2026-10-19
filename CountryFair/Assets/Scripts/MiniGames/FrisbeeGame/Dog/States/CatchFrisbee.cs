using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State where the dog navigates to and catches the thrown frisbee.
/// The dog moves toward the frisbee's position, and upon arrival, deactivates it and transitions to the next state.
/// </summary>
/// <remarks>
/// This state is triggered when the frisbee is thrown and the dog needs to retrieve it.
/// Upon successfully reaching the frisbee, this state deactivates the frisbee GameObject and triggers the transition "FrisbeeCaught"
/// to the <see cref="GiveFrisbeeToPlayer"/> state.
/// </remarks>
public class CatchFrisbee : DogState
{
    /// <summary>FSM transition fired once the dog has picked up the frisbee.</summary>
    private const string FrisbeeCaughtTransition = "FrisbeeCaught";

    /// <summary>Raised when the dog starts running to the frisbee (wired in the Inspector).</summary>
    [SerializeField]
    private UnityEvent _catchFrisbee;
    
    /// <summary>The thrown frisbee; its position is the destination and it is hidden once the dog reaches it.</summary>
    [SerializeField]
    private Transform _frisbeeTransform;
    
    /// <summary>
    /// Runs the base dog setup and validates the frisbee reference.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();

        if (_frisbeeTransform == null)
        {
            Debug.LogError("Frisbee GameObject not found in the scene.");

            return;
        }
    }

    /// <summary>Adds nothing to the base; the frisbee reference is assigned in the Inspector.</summary>
    public override void LateStart()
    {
        base.LateStart();     
    }

    /// <summary>
    /// Called when entering the CatchFrisbee state.
    /// Faces the frisbee, sends the agent to its current position, barks and raises <see cref="_catchFrisbee"/>.
    /// </summary>
    public override void Enter()
   {
        base.Enter();

        RotateDogTowardsTarget(_frisbeeTransform);

        SetFrisbeeDestination();

        Bark();

        _catchFrisbee.Invoke();
   }

    /// <summary>Sets the agent's destination to where the frisbee is now (it is not re-targeted if the frisbee moves).</summary>
   private void SetFrisbeeDestination()
    {
        Vector3 frisbeePos = _frisbeeTransform.position;

        _agent.SetDestination(frisbeePos);
    }

    /// <summary>
    /// Called every frame while in the CatchFrisbee state.
    /// Checks whether the dog has reached the frisbee.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the dog stops at the frisbee's location:
    /// <list type="number">
    /// <item><description>Deactivates the frisbee GameObject (simulating the catch)</description></item>
    /// <item><description>Triggers the transition "FrisbeeCaught" to the <see cref="GiveFrisbeeToPlayer"/> state</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Execute()
    {
         base.Execute();

         if (DogStoped())
         {  
            _frisbeeTransform.gameObject.SetActive(false);
            _fsm.ChangeState(FrisbeeCaughtTransition);
         }
    }

    /// <summary>
    /// Called when exiting the CatchFrisbee state.
    /// </summary>
    /// <remarks>
    /// Performs base cleanup for state exit. No additional cleanup is required for this state.
    /// </remarks>
    public override void Exit()
    {   
        base.Exit();
    }
}
