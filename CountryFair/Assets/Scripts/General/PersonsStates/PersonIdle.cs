using UnityEngine;
public class PersonIdle : PersonBaseState
{   
    [Header("Idle State Settings")]
    [SerializeField]
    private float minIdleDuration = 3f;

    [SerializeField]
    private float maxIdleDuration = 10f;

    [SerializeField]
    private bool canWalk = true;

    private float timeToStartWalking = 0f;

    private float timer = 0f;


    protected override void Awake()
    {
        base.Awake();
    }

    public override void Enter()
    {
        base.Enter();

       timeToStartWalking = Utils.RandomValueInRange(minIdleDuration, maxIdleDuration);

       timer = Time.time + timeToStartWalking;       
    }

    public override void Execute()
    {   
        base.Execute();

        if (canWalk && Time.time >= timer)
        {   
            animator.SetFloat("Speed", 1f);
            fSM.ChangeState("StartWalk");
        }
    }
}