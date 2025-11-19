using UnityEngine;
using UnityEngine.Events;

public class GiveFrisbeeToPlayer : DogState
{  
   [SerializeField]
    private GameObject frisbeeInDogMouth;

    private Vector3 _deliverPosition;

    private Transform _deliverFrisbeeTransform;

    private bool _playerScored = false;

    public UnityEvent frisbeeGivenToPlayer;
   
    protected override void Awake()
    {  
        base.Awake();

        if(frisbeeInDogMouth == null)
        {
            Debug.LogError("Frisbee reference not set in ChooseNewPosition state.");

            return;
        }

        frisbeeInDogMouth.SetActive(false);    
    }

    public override void LateStart()
    {    // It does not need to call base.Start() becuase we don't need the _playerTransform here

        _deliverFrisbeeTransform = GameObject.FindGameObjectWithTag("DeliverFrisbee").transform;

        if (_deliverFrisbeeTransform == null)
        {
            Debug.LogError("DeliverFrisbee GameObject not found in the scene.");
        }
    }

    public override void Enter()
   {
        base.Enter();

        frisbeeInDogMouth.SetActive(true);

        _deliverPosition = _deliverFrisbeeTransform.position;

        _agent.SetDestination(_deliverPosition);
   }

    public override void Execute()
    {
         base.Execute();

         RotateDogTowardsTarget(_deliverFrisbeeTransform);

         if (DogStoped())
         {  
            frisbeeGivenToPlayer.Invoke();

            string transitionName = _playerScored ? "PlayerScored" : "PlayerMissed";

            fSM.ChangeState(transitionName);
            
            // Reset the flag for the next time (overrides to not use an if statement)
            _playerScored = false;
         }
    }

    public override void Exit()
    {
        base.Exit();

        frisbeeInDogMouth.SetActive(false);
    }
    

    public void PlayerScored()
    {
        _playerScored = true;
    }
}
