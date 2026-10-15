using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Animal state that navigates to a random nearby NavMesh position and reduces boredom.
/// Uses a <see cref="NavMeshAgent"/> with a randomized avoidance priority so herded animals don't all cluster.
/// Transitions to the next state once the destination is reached.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk: AnimalState
{
    /// <summary>Amount boredom decreases each time the animal enters the walk state (0–1).</summary>
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _boredomRecoveryRate = 0.01f;

    private NavMeshAgent _agent;


    /// <summary>Avoidance priority configured on the agent, restored on <see cref="Enter"/> after <see cref="Exit"/> forced it to 0.</summary>
    private int _agentDefaultPriority;

    /// <summary>Caches the agent and its Inspector-configured avoidance priority.</summary>
    protected override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        _agentDefaultPriority = _agent.avoidancePriority;
    }

    /// <summary>Registers the <c>"GoWalk"</c> trigger, chosen when boredom is the highest stat.</summary>
    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction("GoWalk", () => _animalUtility.stats.Boredom);
    }

    /// <summary>Resumes the agent with its normal priority, sends it to a random nearby point and recovers boredom.</summary>
    public override void Enter()
    {
        base.Enter();

        _agent.avoidancePriority = _agentDefaultPriority;

        _agent.isStopped = false;

        SetRandomDestination();

        _boredomStat = Mathf.Max(_boredomStat - _boredomRecoveryRate, 0f);

        UpdateStats();
    }

    /// <summary>
    /// When the destination is reached, decides the next action (which sets its Animator trigger); once the
    /// Animator has switched clips, fires that same name as the FSM transition (<c>"GoEat"</c>,
    /// <c>"GoWalk"</c> or <c>"GoIdle"</c>).
    /// </summary>
    public override void Execute()
    {
        base.Execute();

        if (IsPlayingNewAnimation())
        {
             _fsm.ChangeState(_transitionName);
             return;
        }

        if (AnimalStoped() && !_readyToTransition)
        {
             _readyToTransition = true;

            _transitionName = _animalUtility.DecideNextAction(animator);

        }
    }

    /// <summary>Stops the agent and gives it the highest avoidance priority so walking animals steer around it.</summary>
    public override void Exit()
    {
        base.Exit();

         _agent.isStopped = true;

        // When is stopped has max prioty to avoid being pushed by other agents, but when is walking it has a random priority to avoid all agents having the same priority and getting stuck.
         _agent.avoidancePriority = 0;
    }


    /// <summary>Samples a NavMesh point within a 10-unit radius; if none is found the agent keeps its previous destination.</summary>
    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    private bool AnimalStoped()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }
}