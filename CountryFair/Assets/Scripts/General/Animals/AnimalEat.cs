using UnityEngine;

public class AnimalEat: State
{
    [SerializeField]
    private float minTimeToEat = 30f;

    [SerializeField]
    private float maxTimeToEat = 60f;

    private float _timeToEat;

    public override void Enter()
    {
        base.Enter();

        _timeToEat = Utils.RandomValueInRange(minTimeToEat, maxTimeToEat) + Time.time;
    }


    public override void Execute()
    {
        base.Execute();

        if (Time.time >= _timeToEat)
        {
            fSM.ChangeState("FinishedEating");
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}