using UnityEngine;
using UnityEngine.AI;

public class GoToNewTarget : DogState{
    private Transform _newTargetTransform;

    protected override void Awake()
    {   
        base.Awake();
    }

    public override void LateStart()
    {
       base.LateStart();
    }

    private Vector3 ChooseNewTargetPos()
    {
        float dogDistance = _gameManager.AdaptiveParameters["DogDistance"];

        Vector3 targetPosition = _playerTransform.position + _playerTransform.forward * dogDistance;

        const  float NAVMESH_SAMPLE_RADIUS = 200f;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, NAVMESH_SAMPLE_RADIUS, NavMesh.AllAreas))
        {   

            return hit.position;
        }

        Debug.LogError("Could not find a new position for the dog.");

        return _agent.transform.position;
    }

    public override void Enter()
   {
        base.Enter();
      
        Vector3 newTargetPos = ChooseNewTargetPos();

        GameObject target = new("TargetPosition");
        target.transform.position = newTargetPos;
        _newTargetTransform = target.transform;

        _agent.SetDestination(newTargetPos);
   }

    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_newTargetTransform);

        if (DogStoped())
         {  
            fSM.ChangeState("PositionReached");
         }
    }

    public override void Exit()
    {  
        base.Exit();

        RotateDogTowardsTarget(_playerTransform);

        Destroy(_newTargetTransform.gameObject);
    }
}
