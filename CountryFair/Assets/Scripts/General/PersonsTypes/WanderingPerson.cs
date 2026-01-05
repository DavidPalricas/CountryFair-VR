
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderingPerson: MonoBehaviour
{   
    [SerializeField]
    private float walkRadius = 10f;

    [SerializeField]
    private float stuckThreshold = 5f;


    [SerializeField]
    private Animator animator;


    private NavMeshAgent _agent;


    private Vector3 previousPosition  = Vector3.zero;

    private float stuckTimer = 0f;


    private void Awake()
    {   
        if (animator == null)
        {
            Debug.LogError("Animator reference is missing in Walkable Person.");
            return;
        }

        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        ChooseRandomDestination();
    }

    private void Update()
    {  
        if (DestinationReached() || IsStuck())
        {   
            ChooseRandomDestination();
        }
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
        stuckTimer = Time.time + stuckThreshold;
    }


    private bool IsStuck()
    {
        if (Time.time > stuckTimer)
        {   
            Vector3 currentPosition = transform.position;
            
            if (previousPosition != Vector3.zero || previousPosition == currentPosition)
            {
                return true;
            }

            previousPosition = currentPosition;
        }

        return false;
    }
}