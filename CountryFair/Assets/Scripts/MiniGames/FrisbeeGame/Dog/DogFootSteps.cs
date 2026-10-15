using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the dog's footstep sound effects during the dog's walk animation in the Frisbee game.
/// </summary>
public class DogFootSteps  : MonoBehaviour
{
    /// <summary>
    /// Unity event that triggers the dog footstep sound effect playback.
    /// Wired in the Inspector to <see cref="FrisbeeAudioManager"/>, which plays the sound attached to the dog.
    /// </summary>
    [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects, GameObject> _playDogFootSteps;

    /// <summary>
    /// The specific sound effect type for dog footsteps.
    /// </summary>
    private readonly AudioManager.GameSoundEffects _footStepSoundEfect = AudioManager.GameSoundEffects.DogFootSteps;

    /// <summary>
    /// Plays the dog footstep sound effect by invoking the <see cref="_playDogFootSteps"/> event.
    /// </summary>
    /// <remarks>Called by Animation Events on the dog's walk/run clips; renaming it breaks those bindings.</remarks>
    public void PlayFootSteps()
    {
        _playDogFootSteps.Invoke(_footStepSoundEfect, gameObject);
    }
}
