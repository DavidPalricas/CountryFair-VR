using UnityEngine;
using FMODUnity;
using FMOD.Studio;


/// <summary>
/// Audio manager for the CountryFair plaza scene.
/// Starts a spatialized crowd-noise loop on top of the shared ambience music from <see cref="AudioManager"/>.
/// </summary>
public class CountryFairAudioManager : AudioManager
{  
   /// <summary>Starts the crowd loop once the base manager (ambience music) has started.</summary>
   protected override void Start()
   {
      base.Start();

      PlayCrowdSound();
   }
}