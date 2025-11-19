using UnityEngine;


public class CatchFrisbee : DogState
{  
    private Transform _frisbeeTransform;

    protected override void Awake()
    {  
        base.Awake();
    }

    public override void LateStart()
    {    //It does not need to call base.Start() becuase we don't need the _playerTransform or _gameManager here

        _frisbeeTransform = GameObject.FindGameObjectWithTag("Frisbee").transform;

        if (_frisbeeTransform == null)
        {
            Debug.LogError("Frisbee GameObject not found in the scene.");
        }
    }

    public override void Enter()
   {
        base.Enter();

        Vector3 frisbeePos = _frisbeeTransform.position;

        _agent.SetDestination(frisbeePos);
   }

    public override void Execute()
    {
         base.Execute();

        RotateDogTowardsTarget(_frisbeeTransform);

         if (DogStoped())
         {  
            _frisbeeTransform.gameObject.SetActive(false);
            fSM.ChangeState("FrisbeeCaught");
         }
    }

    public override void Exit()
    {   
        base.Exit();
    }
}
