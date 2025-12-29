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

