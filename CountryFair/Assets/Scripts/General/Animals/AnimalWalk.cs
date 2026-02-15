using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk: State
{   
     [Range(0f, 1f)]
     [SerializeField]
     private float percentageOfWalk = 0.3f;

     [Range(0f, 1f)]
     [SerializeField]
     private float percentageOfIdle = 0.2f;

     [Range(0f, 1f)]
     [SerializeField]
     private float percentageOfEat = 0.5f;

    private NavMeshAgent _agent;

    private void Awake()
    {
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

        if (AnimalStoped())
        {
            ChooseToDo();
        }
    }


    private void ChooseToDo()
    {
        float randomValue = Utils.RandomValueInRange(0f, 1f);

        Dictionary<string, float> actions = new()
        {
            { "Walk", percentageOfWalk },
            { "Idle", percentageOfIdle },
            { "Eat", percentageOfEat }
        };

        actions.OrderByDescending(action => action.Value).ThenBy(action => Random.value);
        
        string actionToChoose = "Walk";

        foreach (var action in actions)
        {
            if (randomValue <= action.Value)
            {
                actionToChoose = action.Key;

                break;
            }
        }

        if (actionToChoose == "Walk")
        {
            SetRandomDestination();

            return;
        }

        if (actionToChoose == "Idle")
        {
            fSM.ChangeState("Idle");

            return;
        }

        fSM.ChangeState("Eat");
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