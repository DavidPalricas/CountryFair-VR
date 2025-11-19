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
        
        const float MIN_ANGLE = -90f * Mathf.Deg2Rad;
        const float MAX_ANGLE = 90f * Mathf.Deg2Rad;

        float randomAngle = Utils.RandomValueInRange(MIN_ANGLE, MAX_ANGLE);
      
       Quaternion rotation = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0);

       Vector3 randomDirection = rotation * _playerTransform.forward;
    
       Vector3 targetPosition = _playerTransform.position + randomDirection * dogDistance;

        const  float NAVMESH_SAMPLE_RADIUS = 200f;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, NAVMESH_SAMPLE_RADIUS, NavMesh.AllAreas))
        {   
            
            Debug.Log("New target position chosen at: " + hit.position);
            return hit.position;
        }

        Debug.LogWarning("Failed to find a new target position retrying.");

        return ChooseNewTargetPos();
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

        Destroy(_newTargetTransform.gameObject);
    }
}
