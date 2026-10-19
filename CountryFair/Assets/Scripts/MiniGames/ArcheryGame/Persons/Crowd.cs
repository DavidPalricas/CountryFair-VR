using UnityEngine;
/// <summary>
/// Manages the archery spectator crowd, forwarding score celebrations to all <see cref="IdlePerson"/> children.
/// </summary>
public class Crowd: MonoBehaviour
{
    /// <summary>All crowd members under this GameObject, collected once in <see cref="Awake"/>.</summary>
    private IdlePerson[] _people;

    private void Awake()
    {
        _people = GetComponentsInChildren<IdlePerson>();
    }


    /// <summary>
    /// Triggers a jump animation on every crowd member.
    /// Called by <see cref="Arrow"/> when the player scores.
    /// </summary>
    public void Cheer()
    {
        foreach (IdlePerson person in _people)
        {
            person.Jump();
        }
    }
}