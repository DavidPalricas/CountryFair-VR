using UnityEngine;
using FMODUnity;

/// <summary>
/// Archery-scene audio manager. Registers the spatial balloon-pop and arrow-shot sounds on top of the
/// ambience music and button-pressed sound handled by <see cref="AudioManager"/>.
/// </summary>
/// <remarks>
/// Carny Wise's difficulty-change feedback no longer has sound effects here: it is voiced by the narration event of
/// <see cref="CarnyWiseDiffFeedback"/>.
/// </remarks>
public class ArcheryAudioManager : AudioManager
{
    /// <summary>FMOD event played when a balloon is popped by an arrow.</summary>
    [SerializeField]
    private EventReference _balloonPopSound;

    /// <summary>FMOD event played when the player releases the arrow.</summary>
    [SerializeField]
    private EventReference _arrowShotSound;

    /// <summary>Registers the shared button-pressed sound (base) and the two archery FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();

        RegisterSoundEffect(GameSoundEffects.BalloonPop, _balloonPopSound);
        RegisterSoundEffect(GameSoundEffects.ArrowShot, _arrowShotSound);
    }

    /// <summary>Starts or resumes the shared ambience music (base); the archery scene adds no start-up audio of its own.</summary>
    protected override void Start()
    {
        base.Start();
    }
}