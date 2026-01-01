using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class AudioManager: MonoBehaviour
{  
    [Header("Ambience Music")]
    [SerializeField]
    protected EventReference ambienceMusic;
    private static bool ambienceMusicStarted = false;

    private static EventInstance ambienceMusicInstance;

    public enum GameSoundEffects {
        // Frisbee Game Sound Effects
        DOG_BARK,
        FRISBEE_THROW,
        POINT_SCORED,
        DOG_FOOTSTEPS,
    }

    protected virtual void Start()
    {
        if (!ambienceMusicStarted)
        {
            ambienceMusicStarted = true;
            PlayAmbienceMusic();

            return;
        }

        IsToPauseMusic(false);
    }


    public virtual void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        Debug.LogError("The PlaySpatialAudio method must be overridden in a derived class.");
    }

    public virtual void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        Debug.LogError("The PlayGlobalSoundEffect method must be overridden in a derived class.");
    }

    private void PlayAmbienceMusic()
    {
        ambienceMusicInstance = RuntimeManager.CreateInstance(ambienceMusic);
        ambienceMusicInstance.start();
    }

    private void OnDestroy()
    {
        IsToPauseMusic(true);
    }


    private void IsToPauseMusic(bool pauseMusic)
    {
        ambienceMusicInstance.setPaused(pauseMusic);
    }
}

