
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// Makes an NPC wander the NavMesh by repeatedly picking random destinations within a fixed radius.
/// Used for ambient crowd NPCs in the CountryFair hub world.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class WanderingPerson: MonoBehaviour
{
    /// <summary>Radius in world units within which random walk destinations are sampled.</summary>
    [SerializeField]
    private float _walkRadius = 10f;

    /// <summary>NPC's Animator; only checked for assignment, the walk animation itself is driven by the Animator Controller.</summary>
    [SerializeField]
    private Animator _animator;


    private NavMeshAgent _agent;


    /// <summary>
    /// Caches the agent and gives it a random avoidance priority so crowds of NPCs don't deadlock
    /// when they all have the same priority.
    /// </summary>
    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator reference is missing in Walkable Person.");
            return;
        }

        _agent = GetComponent<NavMeshAgent>();

        const int AvoidancePriorityMaxValue = 99;
        _agent.avoidancePriority = Random.Range(0, AvoidancePriorityMaxValue);
    }

    private void Start()
    {
        ChooseRandomDestination();
    }

    /// <summary>Picks a new destination as soon as the current one is reached, so the NPC never stops.</summary>
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

    /// <summary>
    /// Sends the agent to a random NavMesh point within <see cref="_walkRadius"/>. The sampling result is not
    /// checked, so a failed sample sends the agent to an invalid (infinite) position.
    /// </summary>
    private void ChooseRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _walkRadius;
        randomDirection += transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, _walkRadius, NavMesh.AllAreas);
        _agent.SetDestination(navHit.position);
    }
}