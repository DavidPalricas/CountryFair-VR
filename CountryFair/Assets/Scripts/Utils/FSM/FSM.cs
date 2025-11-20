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
    [SerializeField]
    private List<State> states;

    /// <summary>
    /// List of all possible state transitions in this FSM.
    /// Each transition defines a valid state change with a unique name, a source state (from), and a destination state (to).
    /// Transitions with a null 'from' state act as wildcards and can be triggered from any state.
    /// </summary>
    [SerializeField] 
    private List<Transition> transitions = new();

    /// <summary>
    /// Gets the currently active state of the FSM.
    /// This state's Execute method is called each frame in the Update loop.
    /// </summary>
    /// <value>The current active <see cref="State"/> instance, or null before Start is called.</value>
    public State CurrentState { get; private set; }

    /// <summary>
    /// Initializes the FSM on game start.
    /// Sets the current state to the first state in the states list and calls its Enter method.
    /// Also calls LateStart on all states for any necessary initialization.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked before the first frame update.
    /// Logs an error and returns early if no states are defined.
    /// </remarks>
    private void Start()
    {   
        if (states.Count == 0)
        {
            Debug.LogError("FSM has no states defined.");

            return;
        }

        foreach (State state in states)
        {
            state.LateStart();
        }

        CurrentState = states[0];

        CurrentState.Enter();
    }

    /// <summary>
    /// Updates the FSM each frame by executing the current state's logic.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked once per frame.
    /// Delegates to the <see cref="State.Execute"/> method of the <see cref="CurrentState"/>.
    /// </remarks>
    private void Update()
    {
        CurrentState.Execute();
    }

    /// <summary>
    /// Changes the current state of the FSM by executing the specified transition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method searches through the transitions list to find a matching transition.
    /// A transition matches if:
    /// <list type="bullet">
    /// <item><description>Its name equals the <paramref name="transitionName"/> (comparison is case-insensitive and ignores whitespace)</description></item>
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
    /// </para>
    /// <para>
    /// If no matching transition is found, an error is logged to the console indicating the transition name,
    /// the GameObject name, and the current state name.
    /// </para>
    /// </remarks>
    /// <param name="transitionName">The name of the transition to execute. Case and whitespace insensitive.</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// fsm.ChangeState("ToIdle");
    /// fsm.ChangeState("to idle"); // Equivalent due to whitespace/case insensitivity
    /// </code>
    /// </example>
    public void ChangeState (string transitionName)
    {      
        foreach (Transition transition in transitions)
        {   
            // Removes whitespaces and converts to lowercase to avoid case sensitivity and whitespaces issues
            if (transition.name.Replace(" ", "").ToLower() == transitionName.Replace(" ", "").ToLower() && (transition.from == null || CurrentState == transition.from))
            {   
                CurrentState.Exit();
                CurrentState = transition.to;
                CurrentState.Enter();

                return;
            }
        }

        Debug.LogError($"Transition {transitionName} not found for game object {gameObject.name} in {CurrentState.StateName}.");
    }
}
