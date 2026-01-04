
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderingPerson: MonoBehaviour
{   
    [SerializeField]
    private float walkRadius = 10f;


    [SerializeField]
    private Animator animator;


    private NavMeshAgent _agent;


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
        if (DestinationReached())
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
    }
}