using UnityEngine;
using FMODUnity;

/// <summary>
/// Manages audio playback for the Frisbee mini-game, including dog sounds, frisbee throws, and scoring effects.
/// Inherits from AudioManager to provide game-specific audio functionality.
/// </summary>
public class FrisbeeAudioManager : AudioManager
{   
    [Header("Frisbee Game Sound Effects")]
    /// <summary>
    /// FMOD event reference for the dog bark sound effect.
    /// </summary>
    [SerializeField]
    private EventReference dogBarkSound;

    /// <summary>
    /// FMOD event reference for the frisbee throw sound effect.
    /// </summary>
    [SerializeField]
    private EventReference frisbeeThrowSound;

    /// <summary>
    /// FMOD event reference for the point scored sound effect.
    /// </summary>
    [SerializeField]
    private EventReference pointScoredSound;

    /// <summary>
    /// FMOD event reference for the dog footsteps sound effect.
    /// </summary>
    [SerializeField]
    private EventReference dogFootstepsSound;


    /// <summary>
    /// Initializes the Frisbee audio manager.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Plays a spatial sound effect attached to a specific game object in 3D space.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    /// <param name="target">The game object to which the sound will be attached.</param>
    public override void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {   
        EventReference eventToPlay;

        switch (soundEffect)
        {
            case GameSoundEffects.DOG_BARK:
                eventToPlay = dogBarkSound;
                break;
            case GameSoundEffects.FRISBEE_THROW:
                eventToPlay = frisbeeThrowSound;
                break;

            case GameSoundEffects.DOG_FOOTSTEPS:
                eventToPlay = dogFootstepsSound;
                break;
   
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }

    /// <summary>
    /// Plays a non-spatial sound effect at the default listener position.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    public override void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        EventReference eventToPlay;
       
        switch (soundEffect)
        {
            case GameSoundEffects.POINT_SCORED:
                eventToPlay = pointScoredSound;
                break;
            default:
                Debug.LogError("Unhandled sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShot(eventToPlay);
    }
}