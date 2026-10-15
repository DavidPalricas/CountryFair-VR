using Oculus.Platform;
using UnityEngine;

/// <summary>
/// Abstract base class for animal behavior states (Eat, Walk, Idle).
/// On each entry it increments the stats that this activity worsens, then lets
/// <see cref="AnimalUtility.DecideNextAction"/> pick the next state when activity ends.
/// </summary>
/// <remarks>
/// Each state keeps a local copy of the stats (<see cref="_hungerStat"/>, ...) and pushes it back to the
/// shared <see cref="AnimalUtility.stats"/> through <see cref="UpdateStats"/>.
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

    /// <summary>Local copy of <see cref="AnimalUtility.Stats.Hunger"/>, written back by <see cref="UpdateStats"/>.</summary>
    protected float _hungerStat = 0f;

    /// <summary>Local copy of <see cref="AnimalUtility.Stats.Boredom"/>, written back by <see cref="UpdateStats"/>.</summary>
    protected float _boredomStat = 0f;

    /// <summary>Local copy of <see cref="AnimalUtility.Stats.Fatigue"/>, written back by <see cref="UpdateStats"/>.</summary>
    protected float _fatigueStat = 0f;



    /// <summary>
    /// FSM transition chosen by <see cref="AnimalUtility.DecideNextAction"/>. Its Animator trigger is set
    /// immediately, but the FSM only changes state once <see cref="AnimatableState.IsPlayingNewAnimation"/>
    /// reports that the Animator actually switched clips.
    /// </summary>
    protected string _transitionName = string.Empty;


    /// <summary>True once the next action has been decided, so it is not decided again every frame while the Animator transitions.</summary>
    protected bool _readyToTransition = false;

    /// <summary>Caches <see cref="AnimalUtility"/>, copies its initial stats and registers this state's action.</summary>
    protected override void Awake()
    {
        base.Awake();

        _animalUtility = GetComponent<AnimalUtility>();

        AnimalUtility.Stats animalStats = _animalUtility.stats;

        _hungerStat = animalStats.Hunger;
        _boredomStat = animalStats.Boredom;
        _fatigueStat = animalStats.Fatigue;

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


    private void IncreaseStats()
    {
        _hungerStat = Mathf.Min(_hungerStat + _hungerIncreaseRate, 1f);
        _boredomStat = Mathf.Min(_boredomStat + _boredomIncreaseRate, 1f);
        _fatigueStat = Mathf.Min(_fatigueStat + _fatigueIncreaseRate, 1f);
    }

    /// <summary>Writes the local stat copies back to <see cref="AnimalUtility.stats"/> so the next decision sees them.</summary>
    protected void UpdateStats()
    {
        _animalUtility.stats.Hunger = _hungerStat;
        _animalUtility.stats.Boredom = _boredomStat;
        _animalUtility.stats.Fatigue = _fatigueStat;

        // Debug.Log("Animal Current State: " + GetType().Name + " | Hunger: " + _animalUtility.stats.Hunger + " | Boredom: " + _animalUtility.stats.Boredom + " | Fatigue: " + _animalUtility.stats.Fatigue);
    }
}