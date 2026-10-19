using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State where the dog navigates to a randomly selected new target position.
/// The target position is generated within a semicircular area in front of the player at an adaptive distance.
/// </summary>
/// <remarks>
/// This state reads the adaptive <c>DogDistance</c> PlayerPrefs value (written by <see cref="FrisbeeGameManager"/>) to determine the dog's distance from the player.
/// The target position is randomly chosen within a 180-degree arc in front of the player and validated on the NavMesh.
/// </remarks>
public class GoToTarget : DogState{
    /// <summary>FSM transition fired when the dog reaches its new position.</summary>
    private const string PositionReachedTransition = "PositionReached";

    /// <summary>Maximum number of random positions tried by <see cref="ChooseNewTargetPos"/> before it gives up.</summary>
    private const int MaxTargetPosAttempts = 10;
    
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
    public override void Enter()
   {
        base.Enter();

        Vector3 targetPos =  IsToChooseNewTarget ? ChooseNewTargetPos() : _currentTargetPos;

        _agent.SetDestination(targetPos);

        RotateDogTowardsTarget(targetPos);
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
            _fsm.ChangeState(PositionReachedTransition);

            return;
        }  

        if (_agent.HasReachedDestination())
        {   RotateDogTowardsTarget(_playerTransform);
            animator.SetFloat(SpeedParam, 0f);
        }
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
    /// <item><description>If invalid, retries with a new random angle, up to <see cref="MaxTargetPosAttempts"/> times</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If every attempt fails, the dog keeps its current position instead of looping forever.
    /// </para>
    /// </remarks>
    /// <returns>A valid Vector3 position on the NavMesh within the semicircular area, or the dog's current position if none was found.</returns>
    private Vector3 ChooseNewTargetPos()
    {
        float dogDistance = PlayerPrefs.GetFloat(FrisbeeGameManager.DogDistanceKey, float.MaxValue);

        if (dogDistance == float.MaxValue)
        {
            return transform.position;
        }
     
        const float MinAngle = -90f * Mathf.Deg2Rad;
        const float MaxAngle = 90f * Mathf.Deg2Rad;
        const  float NavmeshSampleRadius = 200f;

        for (int attempt = 0; attempt < MaxTargetPosAttempts; attempt++)
        {
            float randomAngle = Utils.RandomValueInRange(MinAngle, MaxAngle);

            Quaternion rotation = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0);

            Vector3 randomDirection = rotation * _playerTransform.forward;

            Vector3 targetPosition = _playerTransform.position + randomDirection * dogDistance;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, NavmeshSampleRadius, NavMesh.AllAreas))
            {   
                return hit.position;
            }
        }

        Debug.LogWarning($"Failed to find a new target position after {MaxTargetPosAttempts} attempts, keeping the current one.");

        return transform.position;
    }
}
