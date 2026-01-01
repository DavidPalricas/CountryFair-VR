using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the dog's footstep sound effects during the dog's walk animation in the Frisbee game.
/// </summary>
public class DogFootSteps  : MonoBehaviour
{
    /// <summary>
    /// Unity event that triggers the dog footstep sound effect playback.
    /// This event its to alert the <see cref="FrisbeeAudioManager"/> to play the sound. 
    /// </summary>
    public UnityEvent <AudioManager.GameSoundEffects, GameObject> playDogFootSteps;

    /// <summary>
    /// The specific sound effect type for dog footsteps.
    /// </summary>
    private readonly AudioManager.GameSoundEffects _footStepSoundEfect = AudioManager.GameSoundEffects.DOG_FOOTSTEPS;

    /// <summary>
    /// Plays the dog footstep sound effect by invoking the playDogFootSteps event.
    /// This method is typically called from animation events.
    /// </summary>
    public void PlayFootSteps()
    {
        playDogFootSteps.Invoke(_footStepSoundEfect, gameObject);
    }
}
