using UnityEngine.Events;

/// <summary>
/// State representing the frisbee after it has landed and stopped on the ground.
/// Handles the landing event invocation and disabling of trajectory visualization.
/// Waits for the dog to retrieve the frisbee before transitioning to the OnPlayerHand state.
/// </summary>
public class Landed: FrisbeeState
{   
    /// <summary>Event invoked when the frisbee has successfully landed on the ground.</summary>
    public UnityEvent frisbeeLanded;

    /// <summary>
    /// Initializes the state by setting up physics component references.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();
    }

    /// <summary>
    /// Called when entering the Landed state.
    /// Invokes the frisbeeLanded event to notify other systems and disables the trajectory visualization.
    /// </summary>
    public override void Enter()
   {
        base.Enter();

        frisbeeLanded.Invoke();
        _trajectoryLine.enabled = false;
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
    /// Called when exiting the Landed state (when dog retrieves the frisbee).
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }

    /// <summary>
    /// Called by the dog when it retrieves the frisbee.
    /// Transitions to the GivenByDog state to handle the return process.
    /// </summary>
    public void FrisbeeGivenByDog()
    {   
        fSM.ChangeState("GivenByDog");
    }
}