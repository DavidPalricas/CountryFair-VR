using UnityEngine;
using FMODUnity;


/// <summary>
/// Extends <see cref="AudioManager"/> with the Carny Wise DDA sound effects shared across all mini-game scenes.
/// Concrete audio managers (<see cref="FrisbeeAudioManager"/>, <see cref="ArcheryAudioManager"/>) inherit from this class.
/// </summary>
/// <remarks>Abstract because it leaves <see cref="AudioManager.PlaySoundEffect"/> and
/// <see cref="AudioManager.PlaySpatialSoundEffect"/> to the concrete mini-game managers.</remarks>
public abstract class MiniGameAudioManager : AudioManager
{
    /// <summary>FMOD event played when CarnyWise increases the difficulty.</summary>
    [Header("Carny Wise Feedback Sound Effects")]
    [SerializeField]
    protected EventReference _carnyWiseIncreaseDiffSound;

    /// <summary>FMOD event played when CarnyWise decreases the difficulty.</summary>
    [SerializeField]
    protected EventReference _carnyWiseDecreaseDiffSound;


    /// <summary>Validates the base references and the two Carny Wise FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();


        if (_carnyWiseIncreaseDiffSound.IsNull)
        {
            Debug.LogError("Carny Wise increase difficulty sound EventReference is not assigned in MiniGameAudioManager.");

            return;
        }

        if (_carnyWiseDecreaseDiffSound.IsNull)
        {
            Debug.LogError("Carny Wise decrease difficulty sound EventReference is not assigned in MiniGameAudioManager.");
        }
    }
}