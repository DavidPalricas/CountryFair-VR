using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Finite State Machine (FSM) component used to manage state-based behavior for NPCs and other game entities.
/// The FSM manages state transitions based on defined transition rules and executes the logic of the currently active state.
/// </summary>
/// <remarks>
/// This component should be attached to GameObjects that require state-based behavior management.
/// States must inherit from the <see cref="State"/> base class and be added to the states list.
/// Transitions define valid state changes and can include wildcard transitions (from = null) that apply from any state.
/// </remarks>
public class FSM : MonoBehaviour
{
    /// <summary>
    /// List of all available states in this FSM.
    /// The first state in this list will be set as the initial state when the FSM starts.
    /// </summary>
    /// <remarks>A transition whose <see cref="Transition.To"/> is not in this list is rejected by <see cref="ChangeState"/>.</remarks>
    [SerializeField]
    private List<State> _states;

    /// <summary>
    /// List of all possible state transitions in this FSM.
    /// Each transition defines a state change with a name, a source state (from), and a destination state (to).
    /// Transitions with a null 'from' state act as wildcards and can be triggered from any state.
    /// </summary>
    /// <remarks>
    /// Names do not have to be unique: several transitions can share a name with different 'from' states. The list is
    /// searched in order and the first match wins, so a wildcard placed before a specific transition of the same name
    /// shadows it.
    /// </remarks>
    [SerializeField]
    private List<Transition> _transitions = new();

    /// <summary>
    /// Gets the currently active state of the FSM.
    /// This state's Execute method is called each frame in the Update loop.
    /// </summary>
    /// <value>The current active <see cref="State"/> instance, or null before Start is called.</value>
    public State CurrentState { get; private set; }

    /// <summary>
    /// Initializes the FSM on game start.
    /// Calls LateStart on all states, then sets the current state to the first state in the states list and calls its
    /// Enter method.
    /// </summary>
    /// <remarks>
    /// Runs after every state's <c>Awake</c>, so the states have already cached their references when <c>LateStart</c>
    /// and the first <c>Enter</c> run.
    /// Logs an error and returns early if no states are defined; <see cref="CurrentState"/> then stays null and
    /// <see cref="Update"/> throws every frame.
    /// </remarks>
    private void Start()
    {
        if (_states.Count == 0)
        {
            Debug.LogError("FSM has no states defined.");

            return;
        }

        foreach (State state in _states)
        {
            state.LateStart();
        }

        CurrentState = _states[0];

        CurrentState.Enter();
    }

    /// <summary>
    /// Runs the current state's per-frame logic (<see cref="State.Execute"/>).
    /// </summary>
    /// <remarks>
    /// Called from <c>Update</c>: states must not apply physics forces here (they would scale with the frame rate);
    /// they use <see cref="State.FixedExecute"/> instead.
    /// </remarks>
    private void Update()
    {
        CurrentState.Execute();
    }

    /// <summary>
    /// Runs the current state's physics logic (<see cref="State.FixedExecute"/>) once per physics step.
    /// </summary>
    /// <remarks>Null-checked because physics steps can run before <see cref="Start"/> has set the first state.</remarks>
    private void FixedUpdate()
    {
        CurrentState.FixedExecute();
    }

    /// <summary>
    /// Changes the current state of the FSM by executing the specified transition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method searches through the transitions list, in order, for the first matching transition.
    /// A transition matches if:
    /// <list type="bullet">
    /// <item><description>Its name equals the <paramref name="transitionName"/> (comparison is case-insensitive and ignores spaces; tabs and other whitespace are not stripped)</description></item>
    /// <item><description>Either the transition's 'from' state is null (wildcard), OR the current state matches the transition's 'from' state</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When a matching transition is found, the following sequence occurs:
    /// <list type="number">
    /// <item><description>Calls <see cref="State.Exit"/> on the current state</description></item>
    /// <item><description>Updates <see cref="CurrentState"/> to the transition's 'to' state</description></item>
    /// <item><description>Calls <see cref="State.Enter"/> on the new current state</description></item>
    /// </list>
    /// A transition whose 'to' is the current state re-runs Exit and Enter on it.
    /// </para>
    /// <para>
    /// If no matching transition is found, or its 'to' state is not in the states list, an error is logged and the
    /// current state is kept.
    /// </para>
    /// </remarks>
    /// <param name="transitionName">The name of the transition to execute. Case and space insensitive.</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// fsm.ChangeState("ToIdle");
    /// fsm.ChangeState("to idle"); // Equivalent due to space/case insensitivity
    /// </code>
    /// </example>
    public void ChangeState (string transitionName)
    {
        foreach (Transition transition in _transitions)
        {
            if (transition.Name.Replace(" ", "").ToLower() == transitionName.Replace(" ", "").ToLower() && (transition.From == null || CurrentState == transition.From))
            {
                if (!_states.Contains(transition.To))
                {
                    Debug.LogError($"Transition target state {transition.To.StateName} not found in FSM states for game object {gameObject.name}.");
                    return;
                }

                CurrentState.Exit();
                CurrentState = transition.To;
                CurrentState.Enter();

                return;
            }
        }

        Debug.LogError($"Transition {transitionName} not found for game object {gameObject.name} in {CurrentState.StateName}.");
    }
}
