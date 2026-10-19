using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Base audio manager class that handles ambient music and provides virtual methods for game-specific sound effects.
/// This class uses FMOD for audio playback and should be inherited by specific game audio managers.
/// </summary>
public abstract class AudioManager: MonoBehaviour
{
    /// <summary>
    /// FMOD event reference for the ambient background music.
    /// </summary>
    [Header("Ambience Music")]
    [SerializeField]
    protected EventReference _ambienceMusic;

    /// <summary>
    /// FMOD event reference for the button pressed sound effect.
    /// </summary>
    [Header("Button Pressed Sound Effect")]
    [SerializeField]
    protected EventReference _buttonPressedSound;

    /// <summary>
    /// Tracks whether the ambient music has been started to prevent multiple instances.
    /// Static so it survives scene loads: every scene's audio manager shares the single instance below.
    /// </summary>
    private static bool _ambienceMusicStarted = false;

    /// <summary>
    /// The FMOD event instance for the ambient music that persists across scenes.
    /// </summary>
    private static EventInstance _ambienceMusicInstance;

    /// <summary>
    /// Enumeration of all available game sound effects across different mini-games.
    /// </summary>
    public enum GameSoundEffects {
        /* General Sound Effects */
        ButtonPressed,

        /* Country Fair Sound Effects */

        /// <summary>Played when giant mode grows the player (<c>PlayerScale</c>).</summary>
        ScaleToGiant,

        /// <summary>Played when giant mode shrinks the player back (<c>PlayerScale</c>).</summary>
        SCALEToNormal,

        /* Carny Wise Sound Effects */
        /// <summary>Carny Wise feedback when the DDA raises the difficulty.</summary>
        CarnyWiseIncreaseDiff,

        /// <summary>Carny Wise feedback when the DDA lowers the difficulty.</summary>
        CarnyWiseDecreaseDiff,

        /* Frisbee Game Sound Effects*/
        /// <summary>Dog barking sound effect.</summary>
        DogBark,
        /// <summary>Frisbee throwing sound effect.</summary>
        FrisbeeThrow,
        /// <summary>Point scored sound effect.</summary>
        PointScored,
        /// <summary>Dog footsteps sound effect.</summary>
        DogFootSteps,

        /// <summary>Dog whine played when it is happy (e.g. after returning the frisbee).</summary>
        DogHappyWhine,

        /* Archery Game Sound Effects */
        ///<summary>Balloon popping sound effect.</summary>
        BalloonPop,

        ///<summary>Arrow shooting sound effect.</summary>
        ArrowShot,
    }


    /// <summary>Logs an error for any unassigned base FMOD event reference.</summary>
    protected virtual void Awake()
    {
        if (_ambienceMusic.IsNull)
        {
            Debug.LogError("Ambience music EventReference is not assigned in AudioManager.");

            return;
        }

        if (_buttonPressedSound.IsNull)
        {
            Debug.LogError("Button pressed sound EventReference is not assigned in AudioManager.");

            return;
        }
    }

    /// <summary>
    /// Initializes the audio manager. Starts ambient music if not already playing, otherwise resumes it.
    /// </summary>
    protected virtual void Start()
    {
        if (!_ambienceMusicStarted)
        {
            _ambienceMusicStarted = true;
            PlayAmbienceMusic();

            return;
        }

        IsToPauseMusic(false);
    }

    /// <summary>
    /// Plays a spatial sound effect attached to a specific game object in 3D space.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    /// <param name="target">The game object to which the sound will be attached.</param>
    public abstract void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target);

    /// <summary>
    /// Plays a non-spatial sound effect at the default listener position.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    public abstract void PlaySoundEffect(GameSoundEffects soundEffect);

    /// <summary>
    /// Creates and starts the ambient music instance.
    /// </summary>
    private void PlayAmbienceMusic()
    {
        _ambienceMusicInstance = RuntimeManager.CreateInstance(_ambienceMusic);
        _ambienceMusicInstance.start();
    }

    /// <summary>
    /// Pauses the ambient music when the audio manager is destroyed (e.g. on scene change);
    /// the next scene's audio manager resumes it in <see cref="Start"/>.
    /// </summary>
    private void OnDestroy()
    {
        IsToPauseMusic(true);
    }

    /// <summary>
    /// Pauses or resumes the ambient music.
    /// </summary>
    /// <param name="pauseMusic">True to pause the music, false to resume it.</param>
    private void IsToPauseMusic(bool pauseMusic)
    {
        _ambienceMusicInstance.setPaused(pauseMusic);
    }
}
