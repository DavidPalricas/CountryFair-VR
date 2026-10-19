using UnityEngine;

/// <summary>
/// Dog FSM state for the mid-air catch. The jump itself is the Animator clip triggered by the previous state; this
/// state waits for the Animator to move on and then transitions to <c>CatchFrisbee</c>.
/// </summary>
/// <remarks>
/// <see cref="_jumpDuration"/>, <see cref="_jumpPower"/> and <see cref="_jumpNumbers"/> are not used by the current code.
/// </remarks>
public class Jump: DogState
{
    /// <summary>FSM transition fired when the jump ends and the dog goes to catch the frisbee.</summary>
    private const string CatchFrisbeeTransition = "CatchFrisbee";

    /// <summary>Validates the Animator reference on top of the base dog setup.</summary>
     protected override void Awake()
    {   
        base.Awake();

        if (animator == null)
        {
            Debug.LogError("Animator reference is null in Jump state.");
            return;
        }
    }
    
    public override void Enter()
    {
        base.Enter();
    }

    /// <summary>
    /// Once the Animator leaves the clip that was playing on <see cref="Enter"/>, restores the locomotion <c>Speed</c>
    /// parameter and fires the <c>"CatchFrisbee"</c> transition.
    /// </summary>
    public override void Execute()
    {
        base.Execute();
        
        if (IsPlayingNewAnimation())
        {   
            animator.SetFloat(SpeedParam, 1f);
            _fsm.ChangeState(CatchFrisbeeTransition);

            return;
        }
    }



    public override void Exit()
    {
       base.Exit();
    }    
}