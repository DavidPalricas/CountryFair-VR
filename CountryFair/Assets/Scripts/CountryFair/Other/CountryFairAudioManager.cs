using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class CountryFairAudioManager : AudioManager
{
   
   [SerializeField]
   private EventReference crowdNoise;

    private EventInstance  _crowdNoiseInstance;

   private void Awake()
   {
      if (crowdNoise.IsNull)
        {
            Debug.LogError("Crowd noise EventReference is not assigned in CountryFairAudioManager.");
        }
   }

   protected override void Start()
   {
      base.Start();

      PlayCrowdSound();
   }


   private void PlayCrowdSound()
    {   
        _crowdNoiseInstance = RuntimeManager.CreateInstance(crowdNoise);
        RuntimeManager.AttachInstanceToGameObject(_crowdNoiseInstance, transform);
        _crowdNoiseInstance.start();
    }

    private void OnDestroy()
    { 
        _crowdNoiseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _crowdNoiseInstance.release();
    }
}