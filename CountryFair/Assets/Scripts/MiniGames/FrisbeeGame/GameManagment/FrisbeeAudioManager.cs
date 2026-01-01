using UnityEngine;
using FMODUnity;

public class FrisbeeAudioManager : AudioManager
{   
    [Header("Frisbee Game Sound Effects")]
    [SerializeField]
    private EventReference dogBarkSound;

    [SerializeField]
    private EventReference frisbeeThrowSound;

    [SerializeField]
    private EventReference pointScoredSound;

    [SerializeField]
    private EventReference dogFootstepsSound;


    protected override void Start()
    {
        base.Start();
    }

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