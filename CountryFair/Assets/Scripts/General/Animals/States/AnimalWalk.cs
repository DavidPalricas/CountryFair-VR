using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk: AnimalState
{   
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float boredomRecoveryRate = 0.01f;
    
    private NavMeshAgent _agent;

    protected override void Awake()
    {   
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();

        _agent.isStopped = false;

        SetRandomDestination();
    }

    public override void Execute()
    {
        base.Execute();

        _boredomStat = Mathf.Max(_boredomStat - boredomRecoveryRate, 0f);

        UpdateStats();

        if (AnimalStoped())
        {
            string transitionName = _animalUtility.DecideNextAction();
            fSM.ChangeState(transitionName);
        }
    }


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