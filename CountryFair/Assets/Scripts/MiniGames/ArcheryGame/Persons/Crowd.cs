using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
/// <summary>
/// Manages the archery spectator crowd, forwarding score celebrations to all <see cref="IdlePerson"/> children.
/// </summary>
public class Crowd: MonoBehaviour
{
    /// <summary>Intended to play the crowd cheer sound at the crowd's position; currently never invoked by <see cref="Cheer"/>.</summary>
    [SerializeField]
    [FormerlySerializedAs("cheer")]
    private UnityEvent<AudioManager.GameSoundEffects, GameObject> _cheer;

    /// <summary>All crowd members under this GameObject, collected once in <see cref="Awake"/>.</summary>
    private IdlePerson[] _people;

    private void Awake()
    {
        _people = GetComponentsInChildren<IdlePerson>();
    }


    /// <summary>
    /// Triggers a jump animation on every crowd member (the <see cref="_cheer"/> sound event is not invoked here).
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