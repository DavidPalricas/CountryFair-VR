using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Debug = UnityEngine.Debug;

/// <summary>
/// Plays per-line dialogue narration through an FMOD event with a <b>Programmer Instrument</b>, whose audio is picked
/// at runtime from an FMOD <b>Audio Table</b> by key (the audio file name without extension).
/// </summary>
/// <remarks>
/// <para>
/// Usage (driven by <see cref="UIDialog"/>): call <see cref="Prepare"/> to load the clip and learn its duration, then
/// <see cref="Play"/> in the same frame the typewriter starts, so text and voice stay in sync. When the key is not in
/// any loaded audio table, <see cref="Prepare"/> logs a warning and returns -1: no audio plays and the text is typed at
/// its normal speed.
/// </para>
/// <para>
/// The clip is loaded <b>synchronously</b>: an asynchronous (<see cref="MODE.NONBLOCKING"/>) load of an audio-table
/// entry was seen to stay in <see cref="OPENSTATE.LOADING"/> indefinitely, and a voice line read from an already loaded
/// bank is small enough to load within a frame.
/// </para>
/// <para>
/// The FMOD callback is static and marked <see cref="AOT.MonoPInvokeCallbackAttribute"/> because the Quest build uses
/// IL2CPP, which cannot marshal instance-method callbacks. The prepared sound reaches the callback through a
/// <see cref="GCHandle"/> stored as the event instance's user data.
/// </para>
/// </remarks>
public class DialogueNarration
{
    /// <summary>Sound mode used for audio-table clips (as recommended by FMOD for programmer sounds, minus non-blocking).</summary>
    private const MODE SOUND_MODE = MODE.LOOP_NORMAL | MODE.CREATECOMPRESSEDSAMPLE;

    /// <summary>Kept in a static field so the delegate handed to native code is never garbage collected.</summary>
    private static readonly EVENT_CALLBACK _eventCallback = OnEventCallback;

    /// <summary>FMOD event containing the Programmer Instrument.</summary>
    private readonly EventReference _narrationEvent;

    /// <summary>Sound loaded by <see cref="Prepare"/> and not yet handed to an event; released by <see cref="Stop"/> if never played.</summary>
    private Sound _pendingSound;

    /// <summary>Subsound index of <see cref="_pendingSound"/> inside the audio table's FSB.</summary>
    private int _pendingSubsoundIndex;

    /// <summary>True while <see cref="_pendingSound"/> holds a loaded sound waiting for <see cref="Play"/>.</summary>
    private bool _hasPendingSound;

    /// <summary>Instance currently speaking; stopped by <see cref="Stop"/> or when the next line starts.</summary>
    private EventInstance _currentInstance;

    /// <param name="narrationEvent">FMOD event containing a Programmer Instrument.</param>
    public DialogueNarration(EventReference narrationEvent)
    {
        _narrationEvent = narrationEvent;
    }

    /// <summary>
    /// Stops the line currently being narrated (with fade-out) and releases any clip that was prepared but never played.
    /// </summary>
    public void Stop()
    {
        ReleasePendingSound();

        if (_currentInstance.isValid())
        {
            _currentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        _currentInstance.clearHandle();
    }

    /// <summary>
    /// Loads the audio-table clip for <paramref name="key"/>, stopping the previous line, and returns its duration in
    /// seconds, or -1 (after a warning) when there is no clip for that key or it fails to load.
    /// </summary>
    /// <param name="key">Audio table key (audio file name without extension), e.g. <c>intro_ZecaPart1_0</c>.</param>
    /// <returns>The clip duration in seconds, or -1 when there is nothing to play.</returns>
    public float Prepare(string key)
    {
        Stop();

        RESULT result = RuntimeManager.StudioSystem.getSoundInfo(key, out SOUND_INFO info);

        if (result != RESULT.OK)
        {
            Debug.LogWarning($"[Narration] No audio for key '{key}' ({result}); showing the text at normal speed.");
            return -1f;
        }

        // NONBLOCKING is masked out in case the audio table's own flags carry it (see the class remarks).
        MODE mode = (SOUND_MODE | info.mode) & ~MODE.NONBLOCKING;
        result = RuntimeManager.CoreSystem.createSound(info.name_or_data, mode, ref info.exinfo, out Sound sound);

        if (result != RESULT.OK)
        {
            Debug.LogWarning($"[Narration] Could not create the sound for key '{key}' ({result}); showing the text at normal speed.");
            return -1f;
        }

        if (!TryGetDuration(sound, info.subsoundindex, out float duration, out string problem))
        {
            Debug.LogWarning($"[Narration] Audio for key '{key}' failed to load: {problem}. Showing the text at normal speed.");
            sound.release();
            return -1f;
        }

        _pendingSound = sound;
        _pendingSubsoundIndex = info.subsoundindex;
        _hasPendingSound = true;

        return duration;
    }

    /// <summary>
    /// Starts the clip loaded by the last successful <see cref="Prepare"/>. Does nothing if there is none.
    /// Ownership of the sound passes to the event, which releases it when the instance is destroyed.
    /// </summary>
    public void Play()
    {
        if (!_hasPendingSound)
        {
            return;
        }

        ProgrammerSoundData data = new() { sound = _pendingSound, subsoundIndex = _pendingSubsoundIndex };
        _hasPendingSound = false;

        EventInstance instance = RuntimeManager.CreateInstance(_narrationEvent);
        GCHandle handle = GCHandle.Alloc(data);

        instance.setUserData(GCHandle.ToIntPtr(handle));
        instance.setCallback(_eventCallback,
            EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.DESTROYED);

        instance.start();
        // Released right away: FMOD destroys the instance once it stops, and the handle stays usable for Stop() until then.
        instance.release();

        _currentInstance = instance;
    }

    /// <summary>
    /// Returns the length of the clip inside the audio-table FSB <paramref name="sound"/>: the subsound
    /// <paramref name="subsoundIndex"/>, never the FSB itself (whose length is 0).
    /// </summary>
    /// <param name="sound">FSB created from the audio table entry.</param>
    /// <param name="subsoundIndex">Index of the clip inside <paramref name="sound"/>; &lt; 0 when the sound is the clip itself.</param>
    /// <param name="duration">Clip length in seconds, when successful.</param>
    /// <param name="problem">What failed, for the warning log, when unsuccessful.</param>
    /// <returns>True when <paramref name="duration"/> holds the clip length.</returns>
    private static bool TryGetDuration(Sound sound, int subsoundIndex, out float duration, out string problem)
    {
        duration = -1f;
        Sound clip = sound;

        if (subsoundIndex >= 0)
        {
            RESULT subResult = sound.getSubSound(subsoundIndex, out clip);

            if (subResult != RESULT.OK)
            {
                problem = $"getSubSound({subsoundIndex}) failed ({subResult})";
                return false;
            }
        }

        RESULT lengthResult = clip.getLength(out uint lengthMs, TIMEUNIT.MS);

        if (lengthResult != RESULT.OK || lengthMs == 0)
        {
            problem = $"clip length unavailable ({lengthResult}, {lengthMs} ms)";
            return false;
        }

        duration = lengthMs / 1000f;
        problem = null;
        return true;
    }

    /// <summary>Releases the sound loaded by <see cref="Prepare"/> if it was never handed to an event.</summary>
    private void ReleasePendingSound()
    {
        if (!_hasPendingSound)
        {
            return;
        }

        _pendingSound.release();
        _pendingSound.clearHandle();
        _hasPendingSound = false;
    }

    /// <summary>
    /// FMOD callback: hands the prepared sound to the Programmer Instrument, releases it when the instrument is done,
    /// and frees the <see cref="GCHandle"/> once the event instance is destroyed.
    /// </summary>
    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT OnEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        EventInstance instance = new(instancePtr);

        if (instance.getUserData(out IntPtr dataPtr) != RESULT.OK || dataPtr == IntPtr.Zero)
        {
            return RESULT.OK;
        }

        GCHandle handle = GCHandle.FromIntPtr(dataPtr);

        switch (type)
        {
            case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
            {
                ProgrammerSoundData data = (ProgrammerSoundData)handle.Target;
                PROGRAMMER_SOUND_PROPERTIES parameter = Marshal.PtrToStructure<PROGRAMMER_SOUND_PROPERTIES>(parameterPtr);
                parameter.sound = data.sound.handle;
                parameter.subsoundIndex = data.subsoundIndex;
                Marshal.StructureToPtr(parameter, parameterPtr, false);
                break;
            }

            case EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
            {
                PROGRAMMER_SOUND_PROPERTIES parameter = Marshal.PtrToStructure<PROGRAMMER_SOUND_PROPERTIES>(parameterPtr);
                new Sound(parameter.sound).release();
                break;
            }

            case EVENT_CALLBACK_TYPE.DESTROYED:
                handle.Free();
                break;
        }

        return RESULT.OK;
    }
}
