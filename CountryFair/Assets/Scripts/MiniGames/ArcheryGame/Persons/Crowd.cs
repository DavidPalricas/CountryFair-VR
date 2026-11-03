using UnityEngine;
/// <summary>
/// Manages the archery spectator crowd, forwarding score celebrations to all <see cref="ArchPerson"/> children.
/// </summary>
public class Crowd: MonoBehaviour
{
    /// <summary>All crowd members under this GameObject, collected once in <see cref="Awake"/>.</summary>
    private ArchPerson[] _people;

    private void Awake()
    {
        _people = GetComponentsInChildren<ArchPerson>();
    }


    /// <summary>
    /// Triggers a jump animation on every crowd member.
    /// Called by <see cref="Arrow"/> when the player scores.
    /// </summary>
    public void Cheer()
    {
        foreach (ArchPerson person in _people)
        {
            person.Jump();
        }
    }
}