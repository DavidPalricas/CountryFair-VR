using UnityEngine;
using FMODUnity;


/// <summary>
/// Extends <see cref="AudioManager"/> with the Carny Wise DDA sound effects shared across all mini-game scenes.
/// Concrete audio managers (<see cref="FrisbeeAudioManager"/>, <see cref="ArcheryAudioManager"/>) inherit from this class.
/// </summary>
/// <remarks>Abstract because each concrete mini-game manager registers its own sound effects on top of these.</remarks>
public abstract class MiniGameAudioManager : AudioManager
{
    /// <summary>FMOD event played when CarnyWise increases the difficulty.</summary>
    [Header("Carny Wise Feedback Sound Effects")]
    [SerializeField]
    protected EventReference _carnyWiseIncreaseDiffSound;

    /// <summary>FMOD event played when CarnyWise decreases the difficulty.</summary>
    [SerializeField]
    protected EventReference _carnyWiseDecreaseDiffSound;


    /// <summary>Registers the base sound effects and the two Carny Wise FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();

        RegisterSoundEffect(GameSoundEffects.CarnyWiseIncreaseDiff, _carnyWiseIncreaseDiffSound);
        RegisterSoundEffect(GameSoundEffects.CarnyWiseDecreaseDiff, _carnyWiseDecreaseDiffSound);
    }
}