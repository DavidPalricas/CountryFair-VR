using System;
using UnityEngine;

/// <summary>
/// Animal state that plays the idle animation and reduces fatigue.
/// Runs for a random duration, then calls <see cref="AnimalUtility.DecideNextAction"/> to choose the next state.
/// </summary>
public class AnimalIdle: AnimalState
{
    /// <summary>Amount fatigue decreases each time the animal enters the idle state (0–1).</summary>
    [Header("Recovery Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _fatigueRecoveryRate = 0.01f;

    /// <summary>Minimum idle duration in seconds.</summary>
    [Header("Idle Time Settings")]
    [SerializeField]
    private float _minTimeToIdle = 30f;

    /// <summary>Maximum idle duration in seconds.</summary>
    [SerializeField]
    private float _maxTimeToIdle = 60f;

    /// <summary>Absolute <see cref="Time.time"/> at which idling ends and the next action is decided.</summary>
    private float _timeIdle;

    /// <summary>Registers the <c>"GoIdle"</c> trigger, chosen when fatigue is the highest stat.</summary>
    protected override void RegisterActionInUtility()
    {
        _animalUtility.RegisterAction("GoIdle", () => _animalUtility.stats.Fatigue);
    }

    /// <summary>Picks a random idle duration and recovers fatigue up front.</summary>
    public override void Enter()
    {
        base.Enter();

        _timeIdle = Utils.RandomValueInRange(_minTimeToIdle, _maxTimeToIdle) + Time.time;

        _animalUtility.stats.Fatigue = Mathf.Max(_animalUtility.stats.Fatigue - _fatigueRecoveryRate, 0f);
    }


    /// <summary>
    /// When the idle time is over, decides the next action (which sets its Animator trigger); once the
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

        if (Time.time >= _timeIdle && !_readyToTransition)

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