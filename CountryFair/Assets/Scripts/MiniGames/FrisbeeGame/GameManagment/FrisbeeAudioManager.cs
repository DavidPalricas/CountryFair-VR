using UnityEngine;
using FMODUnity;

/// <summary>
/// Manages audio playback for the Frisbee mini-game, including dog sounds, frisbee throws, and scoring effects.
/// Inherits the Carny Wise feedback sounds from <see cref="MiniGameAudioManager"/>.
/// </summary>
public class FrisbeeAudioManager : MiniGameAudioManager
{   
    /// <summary>
    /// FMOD event reference for the dog bark sound effect.
    /// </summary>
    [Header("Frisbee Game Sound Effects")]
    [SerializeField]
    private EventReference _dogBarkSound;
    
    /// <summary>
    /// FMOD event reference for the dog happy whine sound effect.
    /// </summary>
    [SerializeField]
    private EventReference _dogHappyWhineSound;

    /// <summary>
    /// FMOD event reference for the frisbee throw sound effect.
    /// </summary>
    [SerializeField]
    private EventReference _frisbeeThrowSound;

    /// <summary>
    /// FMOD event reference for the point scored sound effect.
    /// </summary>
    [SerializeField]
    private EventReference _pointScoredSound;

    /// <summary>
    /// FMOD event reference for the dog footsteps sound effect.
    /// </summary>
    [SerializeField]
    private EventReference _dogFootstepsSound;

    /// <summary>Registers the inherited sound effects and the Frisbee FMOD events.</summary>
    protected override void Awake()
    {
        base.Awake();

        RegisterSoundEffect(GameSoundEffects.DogBark, _dogBarkSound);
        RegisterSoundEffect(GameSoundEffects.DogHappyWhine, _dogHappyWhineSound);
        RegisterSoundEffect(GameSoundEffects.FrisbeeThrow, _frisbeeThrowSound);
        RegisterSoundEffect(GameSoundEffects.PointScored, _pointScoredSound);
        RegisterSoundEffect(GameSoundEffects.DogFootSteps, _dogFootstepsSound);
    }

    /// <summary>
    /// Initializes the Frisbee audio manager.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }
}