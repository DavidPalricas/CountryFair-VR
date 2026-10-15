using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State where the dog navigates to a randomly selected new target position.
/// The target position is generated within a semicircular area in front of the player at an adaptive distance.
/// </summary>
/// <remarks>
/// This state reads the adaptive <c>DogDistance</c> PlayerPrefs value (written by <see cref="FrisbeeGameManager"/>) to determine the dog's distance from the player.
/// The target position is randomly chosen within a 180-degree arc in front of the player and validated on the NavMesh.
/// A temporary GameObject is created to represent the target position for rotation purposes.
/// </remarks>
public class GoToTarget : DogState{
    /// <summary>
    /// Reference to a temporary GameObject's transform representing the new target position.
    /// Created in <see cref="Enter"/> and destroyed in <see cref="Exit"/>.
    /// </summary>
    private Transform _newTargetTransform;
    
    /// <summary>
    /// Whether <see cref="Enter"/> picks a new position (true, after a score) or returns to <see cref="DogState._currentTargetPos"/> (false, after a miss).
    /// </summary>
    /// <remarks>Set via Inspector by the frisbee's scored/missed events (<see cref="Landed"/>).</remarks>
    public bool IsToChooseNewTarget { get; set; } = false;


    /// <summary>
    /// Runs the base dog setup.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
    }

    /// <summary>
    /// Picks the first target position once the FSM is initialized.
    /// </summary>
    public override void LateStart()
    {
       base.LateStart();

        _currentTargetPos = ChooseNewTargetPos();
    }

    /// <summary>
    /// Called when entering the GoToTarget state.
    /// Picks a new target position (or reuses the previous one, see <see cref="IsToChooseNewTarget"/>) and navigates to it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates a new target position using <see cref="ChooseNewTargetPos"/> and instantiates a temporary
    /// GameObject to represent that position for rotation purposes.
    /// </para>
    /// <para>
    /// The temporary GameObject is necessary for the <see cref="DogState.RotateDogTowardsTarget"/> method
    /// to have a transform reference to calculate rotation direction.
    /// </para>
    /// </remarks>
    public override void Enter()
   {
        base.Enter();

        Vector3 targetPos =  IsToChooseNewTarget ? ChooseNewTargetPos() : _currentTargetPos;

        GameObject target = new("TargetPosition");

        target.transform.position = targetPos;
        _newTargetTransform = target.transform;

        _agent.SetDestination(targetPos);

        RotateDogTowardsTarget(_newTargetTransform);
   }

    /// <summary>
    /// Called every frame while in the GoToTarget state.
    /// </summary>
    /// <remarks>
    /// When the dog stops, it turns to face the player and sets <c>Speed</c> to 0; once the Animator switches to the idle
    /// clip, the "PositionReached" transition to <see cref="DogIdle"/> fires.
    /// </remarks>
    public override void Execute()
    {
        base.Execute();

        if (IsPlayingNewAnimation())
        {
            _fsm.ChangeState("PositionReached");

            return;
        }  

        if (DogStoped())
        {   RotateDogTowardsTarget(_playerTransform);
            animator.SetFloat("Speed", 0f);
        }
    }
    

    /// <summary>
    /// Called when exiting the GoToTarget state.
    /// Destroys the temporary target GameObject to clean up the scene.
    /// </summary>
    /// <remarks>
    /// The temporary target GameObject created in <see cref="Enter"/> is no longer needed
    /// once the state is exited, so it's destroyed to prevent clutter.
    /// </remarks>
    public override void Exit()
    {  
        base.Exit();

        Destroy(_newTargetTransform.gameObject);
    }

     /// <summary>
    /// Calculates a new random target position for the dog within a semicircular area in front of the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm:
    /// <list type="number">
    /// <item><description>Reads the adaptive <c>DogDistance</c> PlayerPrefs value; if it is not set yet, returns the dog's current position</description></item>
    /// <item><description>Generates a random angle between -90° and +90° relative to the player's forward direction</description></item>
    /// <item><description>Calculates a position at the specified distance in the random direction</description></item>
    /// <item><description>Validates the position is on the NavMesh using NavMesh.SamplePosition</description></item>
    /// <item><description>If invalid, recursively calls itself until a valid position is found</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This ensures the dog always positions itself in a walkable area in front of the player.
    /// </para>
    /// </remarks>
    /// <returns>A valid Vector3 position on the NavMesh within the semicircular area.</returns>
    private Vector3 ChooseNewTargetPos()
    {
        float dogDistance = PlayerPrefs.GetFloat("DogDistance", float.MaxValue);

        if (dogDistance == float.MaxValue)
        {
            return transform.position;
        }
     
        const float MinAngle = -90f * Mathf.Deg2Rad;
        const float MaxAngle = 90f * Mathf.Deg2Rad;

        float randomAngle = Utils.RandomValueInRange(MinAngle, MaxAngle);
      
       Quaternion rotation = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0);

       Vector3 randomDirection = rotation * _playerTransform.forward;
    
       Vector3 targetPosition = _playerTransform.position + randomDirection * dogDistance;

        const  float NavmeshSampleRadius = 200f;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, NavmeshSampleRadius, NavMesh.AllAreas))
        {   
            return hit.position;
        }

        Debug.LogWarning("Failed to find a new target position retrying.");

        return ChooseNewTargetPos();
    }
}
