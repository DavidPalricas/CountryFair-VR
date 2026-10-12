using UnityEngine;
using FMODUnity;
using FMOD.Studio;


/// <summary>
/// Audio manager for the CountryFair hub scene.
/// Starts a spatialized crowd-noise loop on top of the shared ambience music from <see cref="AudioManager"/>.
/// </summary>
public class CountryFairAudioManager : AudioManager
{  
   [Header("Country Fair Sound Effects")]
    /// <summary>FMOD event reference for the crowd ambience loop spatialized to the fair area.</summary>
   [SerializeField]
   private EventReference _crowdNoise;

   /// <summary>FMOD one-shot played when the player grows to giant scale (<see cref="AudioManager.GameSoundEffects.SCALE_TO_GIANT"/>).</summary>
   [SerializeField]
   private EventReference _scaleToGiantSound;
  
   /// <summary>FMOD one-shot played when the player shrinks back to normal scale (<see cref="AudioManager.GameSoundEffects.SCALE_TO_NORMAL"/>).</summary>
   [SerializeField]
   private EventReference _scaleToNormalSound;

    /// <summary>Running instance of the crowd loop, kept so it can be stopped and released in <see cref="OnDestroy"/>.</summary>
    private EventInstance  _crowdNoiseInstance;

   /// <summary>Validates that the crowd and scale FMOD events are assigned (logs an error otherwise).</summary>
   protected override void Awake()
   {  
      base.Awake();

      if (_crowdNoise.IsNull)
        {
            Debug.LogError("Crowd noise EventReference is not assigned in CountryFairAudioManager.");

            return;
        }

      if (_scaleToGiantSound.IsNull || _scaleToNormalSound.IsNull)
        {
            Debug.LogError("One or more scale sound EventReferences are not assigned in CountryFairAudioManager.");
        }
   }

   /// <summary>Starts the crowd loop once the base manager (ambience music) has started.</summary>
   protected override void Start()
   {
      base.Start();

      PlayCrowdSound();
   }


   /// <summary>Creates the crowd loop instance, attaches it to this GameObject so it is spatialized, and starts it.</summary>
   private void PlayCrowdSound()
    {   
        _crowdNoiseInstance = RuntimeManager.CreateInstance(_crowdNoise);
        RuntimeManager.AttachInstanceToGameObject(_crowdNoiseInstance, transform);
        _crowdNoiseInstance.start();
    }


    /// <summary>Plays a non-spatial one-shot. Only the giant-mode scale sounds are handled in the hub; any other value logs an error.</summary>
    /// <remarks>Invoked via Inspector by the <c>_onScaleToGiant</c>/<c>_onScaleToNormal</c> events of <see cref="PlayerScale"/> in the CountryFair scene.</remarks>
    public override void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
           case GameSoundEffects.SCALE_TO_GIANT:
                eventToPlay = _scaleToGiantSound;
                break;


            case GameSoundEffects.SCALE_TO_NORMAL:
                eventToPlay = _scaleToNormalSound;
                break;
                
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShot(eventToPlay);
    }


    /// <summary>Plays a one-shot attached to <paramref name="target"/>. Only <c>BUTTON_PRESSED</c> is handled in the hub.</summary>
    /// <param name="soundEffect">Effect to play.</param>
    /// <param name="target">GameObject the sound follows (e.g. the pressed button).</param>
    public override void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
           case GameSoundEffects.BUTTON_PRESSED:
                eventToPlay = buttonPressedSound;
                break;
                
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }

    /// <summary>Stops the crowd loop immediately and releases its FMOD instance so it does not leak across scene loads.</summary>
    private void OnDestroy()
    { 
        _crowdNoiseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _crowdNoiseInstance.release();
    }
}