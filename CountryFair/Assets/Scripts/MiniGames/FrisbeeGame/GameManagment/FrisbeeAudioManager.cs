using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;

/// <summary>
/// Manages audio playback for the Frisbee mini-game, including dog sounds, frisbee throws, and scoring effects.
/// Inherits the Carny Wise feedback sounds from <see cref="MiniGameAudioManager"/>.
/// </summary>
public class FrisbeeAudioManager : MiniGameAudioManager
{   
    /// <summary>
    /// FMOD event reference for the dog bark sound effect.
    /// </summary>
    [Header("Frisbee Game Sound Effects")]
    [SerializeField]
    [FormerlySerializedAs("dogBarkSound")]
    private EventReference _dogBarkSound;
    
    /// <summary>
    /// FMOD event reference for the dog happy whine sound effect.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("dogHappyWhineSound")]
    private EventReference _dogHappyWhineSound;

    /// <summary>
    /// FMOD event reference for the frisbee throw sound effect.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("frisbeeThrowSound")]
    private EventReference _frisbeeThrowSound;

    /// <summary>
    /// FMOD event reference for the point scored sound effect.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("pointScoredSound")]
    private EventReference _pointScoredSound;

    /// <summary>
    /// FMOD event reference for the dog footsteps sound effect.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("dogFootstepsSound")]
    private EventReference _dogFootstepsSound;

    /// <summary>Validates the inherited references and the Frisbee FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();

        if (_dogBarkSound.IsNull)
        {
            Debug.LogError("Dog bark sound EventReference is not assigned in FrisbeeAudioManager.");

            return;
        }

        if (_frisbeeThrowSound.IsNull)
        {
            Debug.LogError("Frisbee throw sound EventReference is not assigned in FrisbeeAudioManager.");

            return;
        }

        if (_pointScoredSound.IsNull)
        {
            Debug.LogError("Point scored sound EventReference is not assigned in FrisbeeAudioManager.");

            return;
        }

        if (_dogFootstepsSound.IsNull)
        {
            Debug.LogError("Dog footsteps sound EventReference is not assigned in FrisbeeAudioManager.");
        }
    }

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
            case GameSoundEffects.ButtonPressed:
                eventToPlay = _buttonPressedSound;
                break;

            case GameSoundEffects.DogBark:
                eventToPlay = _dogBarkSound;
                break;

            case GameSoundEffects.DogHappyWhine:
                eventToPlay = _dogHappyWhineSound;
                break;
                
            case GameSoundEffects.FrisbeeThrow:
                eventToPlay = _frisbeeThrowSound;
                break;

            case GameSoundEffects.DogFootSteps:
                eventToPlay = _dogFootstepsSound;
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
            case GameSoundEffects.CarnyWiseIncreaseDiff:
                eventToPlay = _carnyWiseIncreaseDiffSound;
                break;

            case GameSoundEffects.CarnyWiseDecreaseDiff:
                eventToPlay = _carnyWiseDecreaseDiffSound;
                break;

            case GameSoundEffects.PointScored:
                eventToPlay = _pointScoredSound;
                break;
                
            default:
                Debug.LogError("Unhandled sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShot(eventToPlay);
    }
}