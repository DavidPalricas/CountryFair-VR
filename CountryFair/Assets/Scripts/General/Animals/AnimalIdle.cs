using UnityEngine;

public class AnimalIdle: AnimalState
{
    [SerializeField]
    private float minTimeToIdle = 30f;

    [SerializeField]
    private float maxTimeToIdle = 60f;

    private float _timeIdle;

    public override void Enter()
    {
        base.Enter();

        _timeIdle = Utils.RandomValueInRange(minTimeToIdle, maxTimeToIdle) + Time.time;
    }


    public override void Execute()
    {
        base.Execute();

        if (Time.time >= _timeIdle)
        {
            fSM.ChangeState("StartMoving");
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}