using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Makes this dog play a random MJ sound effect (through <see cref="MJAudio"/>) after a random wait,
/// over and over. Each dog has its own timer, so several dogs play at different times.
/// </summary>
public class MJDog : MonoBehaviour
{
    /// <summary>
    /// The audio manager that owns the MJ sound effects.
    /// </summary>
    [Header("AudioEvent")]
    [SerializeField]
    private UnityEvent<GameObject> _playMjSoundEffect;

    /// <summary>
    /// Minimum time, in seconds, between two sound effects.
    /// </summary>
    [Header("Times for sound effects")]
    [SerializeField]
    private float _minTimeToPlaySoundEffect = 35f;

    /// <summary>
    /// Maximum time, in seconds, between two sound effects.
    /// </summary>
    [SerializeField]
    private float _maxTimeToPlaySoundEffect = 60f;

    /// <summary>
    /// System timer that signals when the next sound effect should play. It fires on a thread-pool thread.
    /// </summary>
    private System.Timers.Timer _soundEffectTimer;

    /// <summary>
    /// Set by <see cref="_soundEffectTimer"/> and consumed on the main thread in <see cref="Update"/>,
    /// because FMOD and Unity APIs cannot be called from the timer's thread.
    /// </summary>
    private volatile bool _isTimeToPlaySoundEffect = false;

  

    /// <summary>Creates the system timer and starts the first countdown.</summary>
    private void Start()
    {
        _soundEffectTimer = new System.Timers.Timer { AutoReset = false };
        _soundEffectTimer.Elapsed += (_, _) => _isTimeToPlaySoundEffect = true;

        ResetSoundEffectTimer();
    }

    /// <summary>Plays a random sound effect on the main thread once the system timer has elapsed.</summary>
    private void Update()
    {
        if (!_isTimeToPlaySoundEffect)
        {
            return;
        }

        _isTimeToPlaySoundEffect = false;

        _playMjSoundEffect.Invoke(gameObject);

        ResetSoundEffectTimer();
    }

    /// <summary>
    /// Restarts the system timer with a new random wait between <see cref="_minTimeToPlaySoundEffect"/>
    /// and <see cref="_maxTimeToPlaySoundEffect"/>.
    /// </summary>
    private void ResetSoundEffectTimer()
    {
        float secondsToWait = Utils.RandomValueInRange(_minTimeToPlaySoundEffect, _maxTimeToPlaySoundEffect);

        _soundEffectTimer.Interval = secondsToWait * 1000f;
        _soundEffectTimer.Start();
    }

    /// <summary>Stops the timer when the dog is destroyed so it does not keep running.</summary>
    private void OnDestroy()
    {
        _soundEffectTimer?.Dispose();
    }
}
