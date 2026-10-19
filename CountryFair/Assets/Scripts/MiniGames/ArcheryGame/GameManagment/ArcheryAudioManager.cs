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

    /// <summary>Registers the inherited sound effects and the two archery FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();

        RegisterSoundEffect(GameSoundEffects.BalloonPop, _balloonPopSound);
        RegisterSoundEffect(GameSoundEffects.ArrowShot, _arrowShotSound);
    }

    protected override void Start()
    {
        base.Start();
    }
}