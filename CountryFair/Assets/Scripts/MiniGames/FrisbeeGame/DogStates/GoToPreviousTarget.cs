
using UnityEngine;

public class GoToPreviousTarget : DogState{
    private  Transform _previousTargetTransform;

    protected override void Awake()
    {   
        base.Awake();
    }

    public override void LateStart()
    {
        base.LateStart();
    }

    public override void Enter()
   {
        base.Enter();
       
        _previousTargetTransform =  new GameObject("PreviousTarget").transform;
        _previousTargetTransform.position = _gameManager.currentTargetPos;

        _agent.SetDestination(_previousTargetTransform.position);
   }

    public override void Execute()
    {
        base.Execute();

        RotateDogTowardsTarget(_previousTargetTransform);

        if (DogStoped())
         {  
            fSM.ChangeState("PositionReached");
         }

    }

    public override void Exit()
    {  
        base.Exit();

       // Then destroy the temporary target object becuase it is only needed for the dog rotating towards it
        Destroy(_previousTargetTransform.gameObject);

    }
}
