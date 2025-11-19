using UnityEngine;
using UnityEngine.Events;

public class GiveFrisbeeToPlayer : DogState
{  
   [SerializeField]
    private GameObject frisbeeInDogMouth;

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
    {    
        base.LateStart();
    }

    public override void Enter()
   {
        base.Enter();

        frisbeeInDogMouth.SetActive(true);

        _agent.SetDestination(_playerTransform.position);
   }

    public override void Execute()
    {
         base.Execute();

         RotateDogTowardsTarget(_playerTransform);

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
