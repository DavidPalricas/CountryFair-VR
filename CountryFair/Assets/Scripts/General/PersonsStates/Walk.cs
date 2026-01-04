
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class Walk: PersonBaseState
{   
    [Header("Walk State Settings")]
    [SerializeField]
    private float walkRadius = 10f;


    [SerializeField]
    private float probToContinueWalking = 0.7f;


    private NavMeshAgent _agent;


    protected override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
    }


    public override void Enter()
    {
        base.Enter();

        ChooseRandomDestination();
    }


    public override void Execute()
    {  
        base.Execute();
        
        if (DestinationReached())
        {   
            float randomNumber = Utils.RandomValueInRange(0f, 1f);

            if (randomNumber <= probToContinueWalking)
            {
                ChooseRandomDestination();

                return;
            }

            animator.SetFloat("Speed", 0f);

            fSM.ChangeState("Rest");
        }
    }


    public override void Exit()
    {
        base.Exit();  
    }

    private bool DestinationReached()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }

    private void ChooseRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, walkRadius, NavMesh.AllAreas);
        _agent.SetDestination(navHit.position);
    }
}