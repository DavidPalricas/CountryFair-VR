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

   /// <summary>FMOD one-shot played when the player grows to giant scale (<see cref="AudioManager.GameSoundEffects.ScaleToGiant"/>).</summary>
   [SerializeField]
   private EventReference _scaleToGiantSound;
  
   /// <summary>FMOD one-shot played when the player shrinks back to normal scale (<see cref="AudioManager.GameSoundEffects.ScaleToNormal"/>).</summary>
   [SerializeField]
   private EventReference _scaleToNormalSound;

    /// <summary>Running instance of the crowd loop, kept so it can be stopped and released in <see cref="OnDestroy"/>.</summary>
    private EventInstance  _crowdNoiseInstance;

   /// <summary>Validates the crowd loop and registers the giant-mode scale sounds.</summary>
   /// <remarks>The scale sounds are played via Inspector by the <c>_onScaleToGiant</c>/<c>_onScaleToNormal</c> events of <see cref="PlayerScale"/> in the CountryFair scene.</remarks>
   protected override void Awake()
   {  
      base.Awake();

      if (_crowdNoise.IsNull)
        {
            Debug.LogError("Crowd noise EventReference is not assigned in CountryFairAudioManager.");
        }

      RegisterSoundEffect(GameSoundEffects.ScaleToGiant, _scaleToGiantSound);
      RegisterSoundEffect(GameSoundEffects.ScaleToNormal, _scaleToNormalSound);
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


    /// <summary>Stops the crowd loop immediately and releases its FMOD instance so it does not leak across scene loads.</summary>
    private void OnDestroy()
    { 
        _crowdNoiseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _crowdNoiseInstance.release();
    }
}