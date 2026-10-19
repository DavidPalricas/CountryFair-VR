using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

/// <summary>
/// Base audio manager class that handles ambient music and plays the sound effects registered by each scene's manager.
/// This class uses FMOD for audio playback and should be inherited by specific game audio managers.
/// </summary>
/// <remarks>
/// Subclasses map each <see cref="GameSoundEffects"/> they support to its FMOD event with
/// <see cref="RegisterSoundEffect"/> in <c>Awake</c>; <see cref="PlaySoundEffect"/> and
/// <see cref="PlaySpatialSoundEffect"/> then look the event up instead of each subclass repeating a switch.
/// </remarks>
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

    /// <summary>FMOD event of every sound effect this scene supports, filled by <see cref="RegisterSoundEffect"/>.</summary>
    private readonly Dictionary<GameSoundEffects, EventReference> _soundEffects = new();

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
        ScaleToNormal,

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


    /// <summary>Validates the ambience music and registers the button pressed sound shared by every scene.</summary>
    protected virtual void Awake()
    {
        if (_ambienceMusic.IsNull)
        {
            Debug.LogError("Ambience music EventReference is not assigned in AudioManager.");
        }

        RegisterSoundEffect(GameSoundEffects.ButtonPressed, _buttonPressedSound);
    }

    /// <summary>
    /// Makes <paramref name="soundEffect"/> playable through <see cref="PlaySoundEffect"/> and
    /// <see cref="PlaySpatialSoundEffect"/>. Logs an error and skips it when <paramref name="eventReference"/> is not
    /// assigned in the Inspector.
    /// </summary>
    /// <param name="soundEffect">The sound effect type.</param>
    /// <param name="eventReference">The FMOD event played for it.</param>
    protected void RegisterSoundEffect(GameSoundEffects soundEffect, EventReference eventReference)
    {
        if (eventReference.IsNull)
        {
            Debug.LogError($"{soundEffect} sound EventReference is not assigned in {GetType().Name}.");

            return;
        }

        _soundEffects[soundEffect] = eventReference;
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
    /// <remarks>Invoked via Inspector by UnityEvents across the scenes; logs an error for effects this scene did not register.</remarks>
    public void PlaySpatialSoundEffect(GameSoundEffects soundEffect, GameObject target)
    {
        if (_soundEffects.TryGetValue(soundEffect, out EventReference eventToPlay))
        {
            RuntimeManager.PlayOneShotAttached(eventToPlay, target);

            return;
        }

        Debug.LogError($"Sound effect {soundEffect} is not handled by {GetType().Name}.");
    }

    /// <summary>
    /// Plays a non-spatial sound effect at the default listener position.
    /// </summary>
    /// <param name="soundEffect">The type of sound effect to play.</param>
    /// <remarks>Invoked via Inspector by UnityEvents across the scenes; logs an error for effects this scene did not register.</remarks>
    public void PlaySoundEffect(GameSoundEffects soundEffect)
    {
        if (_soundEffects.TryGetValue(soundEffect, out EventReference eventToPlay))
        {
            RuntimeManager.PlayOneShot(eventToPlay);

            return;
        }

        Debug.LogError($"Sound effect {soundEffect} is not handled by {GetType().Name}.");
    }

  
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
