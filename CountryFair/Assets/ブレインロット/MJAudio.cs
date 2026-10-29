using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Standalone audio manager, independent from <see cref="AudioManager"/>, that plays a single music track in loop.
/// Any music already playing (the shared ambient music or a previous <see cref="MJAudio"/> track) is stopped first.
/// </summary>
/// <remarks>
/// The loop itself is defined in the FMOD event (loop region / looping instrument).
/// It also plays the MJ sound effects on demand through <see cref="PlaySpatialSoundEffect"/>, called by each <see cref="MJDog"/>.
/// </remarks>
public class MJAudio : MonoBehaviour
{
    /// <summary>
    /// FMOD event reference for the music played in loop.
    /// </summary>
    [Header("Music")]
    [SerializeField]
    private EventReference _music;


    [Header("MJSoundEffects")]
    [SerializeField]
    private EventReference _soundEffectHeeHee;

    [SerializeField]
    private EventReference _soundEffectOw;

    /// <summary>
    /// The sound effects that can be played by this <see cref="MJAudio"/>. The enum is used to avoid hardcoding the event references in multiple places.
    /// </summary>
    private enum MJSoundEffects{
        HeeHee,
        Ow
    }

    private readonly MJSoundEffects[] _mJSoundEffects = new MJSoundEffects[] { MJSoundEffects.HeeHee, MJSoundEffects.Ow };

    /// <summary>
    /// The music instance currently playing. Static so a new <see cref="MJAudio"/> can stop the previous one.
    /// </summary>
    private static EventInstance _musicInstance;

    /// <summary>Validates the music and sound effect references.</summary>
    private void Awake()
    {
        Utils.RequireAssigned(_music, nameof(_music), this);
        Utils.RequireAssigned(_soundEffectHeeHee, nameof(_soundEffectHeeHee), this);
        Utils.RequireAssigned(_soundEffectOw, nameof(_soundEffectOw), this);
    }

    /// <summary>Stops any music already playing and starts this one.</summary>
    private void Start()
    {
        AudioManager.StopAmbienceMusic();
        StopMusic();

        _musicInstance = RuntimeManager.CreateInstance(_music);
        _musicInstance.start();
    }

    /// <summary>Stops the music when leaving the scene so it does not keep playing elsewhere.</summary>
    private void OnDestroy()
    {
        StopMusic();
    }

    /// <summary>
    /// Stops and releases the current music instance, if there is one.
    /// </summary>
    private static void StopMusic()
    {
        if (!_musicInstance.isValid())
        {
            return;
        }

        _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicInstance.release();
        _musicInstance.clearHandle();
    }


    /// <summary>
    /// Plays a random sound effect from the available options, attached to <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The game object the sound follows (e.g. the dog playing it).</param>
    public void PlaySpatialSoundEffect(GameObject target)
    {
        MJSoundEffects randomSoundEffect = _mJSoundEffects[Utils.RandomValueInRange(0, _mJSoundEffects.Length)];

        EventReference eventToPlay = randomSoundEffect switch
        {
            MJSoundEffects.HeeHee => _soundEffectHeeHee,
            MJSoundEffects.Ow => _soundEffectOw,
            _ => throw new System.ArgumentOutOfRangeException()
        };

        RuntimeManager.PlayOneShotAttached(eventToPlay, target);
    }
}
