using Oculus.Platform;
using UnityEngine;

/// <summary>
/// Abstract base class for animal behavior states (Eat, Walk, Idle).
/// On each entry it increments the stats that this activity worsens, then lets
/// <see cref="AnimalUtility.DecideNextAction"/> pick the next state when activity ends.
/// </summary>
/// <remarks>
/// Every state reads and writes the shared <see cref="AnimalUtility.stats"/> directly. The states must not keep their
/// own copies: each one would write its stale values back over the progress made by the other states (e.g. the hunger
/// recovered while eating would be undone by the next walk).
/// </remarks>
[RequireComponent(typeof(AnimalUtility))]
public class AnimalState: AnimatableState
{
    /// <summary>Rate at which hunger increases every time this state is entered (0–1).</summary>
    [Header("Increase Stats Rates")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _hungerIncreaseRate = 0.1f;

    /// <summary>Rate at which boredom increases every time this state is entered (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float _boredomIncreaseRate = 0.01f;

    /// <summary>Rate at which fatigue increases every time this state is entered (0–1).</summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float _fatigueIncreaseRate = 0.01f;

    /// <summary>Never assigned; subclasses use the inherited <see cref="AnimatableState.animator"/> instead.</summary>
    protected Animator _animator;

    /// <summary>Shared stats holder on the same GameObject; also where this state registers its action.</summary>
    protected AnimalUtility _animalUtility;


    /// <summary>
    /// FSM transition chosen by <see cref="AnimalUtility.DecideNextAction"/>. Its Animator trigger is set
    /// immediately, but the FSM only changes state once <see cref="AnimatableState.IsPlayingNewAnimation"/>
    /// reports that the Animator actually switched clips.
    /// </summary>
    protected string _transitionName = string.Empty;


    /// <summary>True once the next action has been decided, so it is not decided again every frame while the Animator transitions.</summary>
    protected bool _readyToTransition = false;

    /// <summary>Caches <see cref="AnimalUtility"/> and registers this state's action.</summary>
    /// <remarks>
    /// The stats are not read here, so it does not matter whether <see cref="AnimalUtility"/>'s Awake (which seeds them)
    /// has run yet; they are first used in <see cref="Enter"/>, after every Awake.
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();

        _animalUtility = GetComponent<AnimalUtility>();

        SetStateProprieties();

        // Each subclass registers its own trigger + stat so AnimalUtility
        // knows which actions are available without any hardcoded list.
        RegisterActionInUtility();
    }

    /// <summary>
    /// Override in subclasses to register the Animator trigger and the stat
    /// this state is responsible for recovering.
    /// Example:  _animalUtility.RegisterAction("GoEat", () => _animalUtility.stats.Hunger);
    /// </summary>
    protected virtual void RegisterActionInUtility() { }

    /// <summary>Raises every stat by its increase rate; subclasses then recover their own stat.</summary>
    public override void Enter()
    {
        base.Enter();

        IncreaseStats();
    }

    public override void Execute()
    {
        base.Execute();
    }

    /// <summary>Clears <see cref="_readyToTransition"/> so the next state can decide its own follow-up action.</summary>
    public override void Exit()
    {
        base.Exit();

        _readyToTransition = false;
    }


    /// <summary>Raises every shared stat by this state's increase rate, capped at 1.</summary>
    private void IncreaseStats()
    {
        _animalUtility.stats.Hunger = Mathf.Min(_animalUtility.stats.Hunger + _hungerIncreaseRate, 1f);
        _animalUtility.stats.Boredom = Mathf.Min(_animalUtility.stats.Boredom + _boredomIncreaseRate, 1f);
        _animalUtility.stats.Fatigue = Mathf.Min(_animalUtility.stats.Fatigue + _fatigueIncreaseRate, 1f);
    }
}