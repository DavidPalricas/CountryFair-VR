
/// <summary>
/// Represents a transition between two states in the FSM.
/// Serializable to allow configuration in the Unity Inspector.
/// </summary>
[System.Serializable]
public class Transition
{
    /// <summary>
    /// Name passed to <see cref="FSM.ChangeState"/> to fire this transition; matched ignoring case and spaces.
    /// Several transitions may share a name if they have different <see cref="From"/> states.
    /// </summary>
    public string Name;

    /// <summary>
    /// Source state from which the transition originates. Null indicates a wildcard (any state).
    /// </summary>
    public State From;

    /// <summary>
    /// Target state to which the transition leads. Must also be in the FSM's states list, otherwise the transition is
    /// rejected with an error.
    /// </summary>
    public State To;
}
