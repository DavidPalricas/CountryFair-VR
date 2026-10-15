using UnityEngine;
using FMODUnity;

/// <summary>
/// Archery-scene audio manager. Handles spatial balloon-pop and arrow-shot sounds in addition to
/// the Carny Wise DDA feedback sounds inherited from <see cref="MiniGameAudioManager"/>.
/// </summary>
public class ArcheryAudioManager :MiniGameAudioManager
{
    /// <summary>FMOD event played when a balloon is popped by an arrow.</summary>
    [SerializeField]
    private EventReference _balloonPopSound;

    /// <summary>FMOD event played when the player releases the arrow.</summary>
    [SerializeField]
    private EventReference _arrowShotSound;

    /// <summary>Validates the base references and the two archery FMOD events.</summary>
    protected override void Awake()
    {   
        base.Awake();

        if (_balloonPopSound.IsNull)
        {
            Debug.LogError("Balloon pop sound EventReference is not assigned in ArcheryAudioManager.");

            return;
        }

        if (_arrowShotSound.IsNull)
        {
            Debug.LogError("Arrow shot sound EventReference is not assigned in ArcheryAudioManager.");
        }
    }

    protected override void Start()
    {
        base.Start();
    }


    /// <summary>Plays the button, balloon-pop or arrow-shot event attached to <paramref name="target"/>; other effects log an error.</summary>
    public override void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        EventReference eventToPlay;

        switch (soundEffect)
        {  
           case GameSoundEffects.ButtonPressed:
                eventToPlay = _buttonPressedSound;
                break;

           case GameSoundEffects.BalloonPop:
                eventToPlay = _balloonPopSound;
                break;

            case GameSoundEffects.ArrowShot:
                eventToPlay = _arrowShotSound;
                break;
   
            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }


    /// <summary>Plays the non-spatial Carny Wise difficulty feedback events; other effects log an error.</summary>
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

            default:
                Debug.LogError("Invalid sound effect: " + soundEffect);
                return;
        }

        RuntimeManager.PlayOneShot(eventToPlay);
    }
}