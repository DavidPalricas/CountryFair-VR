using UnityEngine;

/// <summary>
/// Animal state that plays the eating animation and reduces hunger.
/// Runs for a random duration, then calls <see cref="AnimalUtility.DecideNextAction"/> to choose the next state.
/// </summary>
public class AnimalEat: AnimalState
{
    /// <summary>Animator trigger and FSM transition name of this action.</summary>
    private const string GoEatTrigger = "GoEat";

    /// <summary>Amount hunger decreases each time the animal enters the eating state (0–1).</summary>
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _hungerRecoveryRate = 0.1f;

    /// <summary>Minimum eating duration in seconds.</summary>
    [Header("Eating Time Settings")]
    [SerializeField]
    private float _minTimeToEat = 30f;

    /// <summary>Maximum eating duration in seconds.</summary>
    [SerializeField]
    private float _maxTimeToEat = 60f;


    /// <summary>Absolute <see cref="Time.time"/> at which eating ends and the next action is decided.</summary>
    private float _timeToEat;

    /// <summary>Registers the <c>"GoEat"</c> trigger, chosen when hunger is the highest stat.</summary>
    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction(GoEatTrigger, () => _animalUtility.stats.Hunger);
    }

    /// <summary>Picks a random eating duration and recovers hunger up front.</summary>
    public override void Enter()
    {
        base.Enter();

        _timeToEat = Utils.RandomValueInRange(_minTimeToEat, _maxTimeToEat) + Time.time;

        _animalUtility.stats.Hunger = Mathf.Max(_animalUtility.stats.Hunger - _hungerRecoveryRate, 0f);
    }


    /// <summary>
    /// When the eating time is over, decides the next action (which sets its Animator trigger); once the
    /// Animator has switched clips, fires that same name as the FSM transition (<c>"GoEat"</c>,
    /// <c>"GoWalk"</c> or <c>"GoIdle"</c>).
    /// </summary>
    public override void Execute()
    {
        base.Execute();

        if (IsPlayingNewAnimation())
        {
             _fsm.ChangeState(_transitionName);
             return;
        }

        if (Time.time >= _timeToEat && !_readyToTransition)
        {
            _readyToTransition = true;
            _transitionName = _animalUtility.DecideNextAction(animator);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}