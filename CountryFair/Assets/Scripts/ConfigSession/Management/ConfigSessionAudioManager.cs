using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class ConfigSessionAudioManager : AudioManager
{   
    [Header("Config Session Sound Effects")]
    /// <summary>FMOD one-shot played when the player grows to giant scale (<see cref="AudioManager.GameSoundEffects.ScaleToGiant"/>).</summary>
    [SerializeField]
    private EventReference _scaleToGiantSound;

    /// <summary>FMOD one-shot played when the player shrinks back to normal scale (<see cref="AudioManager.GameSoundEffects.ScaleToNormal"/>).</summary>
    [SerializeField]
    private EventReference _scaleToNormalSound;

    [SerializeField]
    private EventReference _configButtonAppearedSound;

    [SerializeField]
    private EventReference _uIClickedSound;

    /// <summary>Validates the giant-mode scale sounds.</summary>
    /// <remarks>The scale sounds are played via Inspector by the <c>_onScaleToGiant</c>/<c>_onScaleToNormal</c> events of <see cref="PlayerScale"/> in the ConfigSession scene.</remarks>
    protected override void Awake()
    {
        base.Awake();

        if (!Utils.RequireAssigned(_scaleToGiantSound, nameof(_scaleToGiantSound), this) ||
            !Utils.RequireAssigned(_scaleToNormalSound, nameof(_scaleToNormalSound), this) ||
            !Utils.RequireAssigned(_configButtonAppearedSound, nameof(_configButtonAppearedSound), this) ||
            !Utils.RequireAssigned(_uIClickedSound, nameof(_uIClickedSound), this))
        {
            return;
        }

        RegisterSoundEffect(GameSoundEffects.ScaleToGiant, _scaleToGiantSound);
        RegisterSoundEffect(GameSoundEffects.ScaleToNormal, _scaleToNormalSound);

        RegisterSoundEffect(GameSoundEffects.ConfigButtonApeared, _configButtonAppearedSound);
        RegisterSoundEffect(GameSoundEffects.UIClick, _uIClickedSound);
    }

       /// <summary>Starts the crowd loop once the base manager (ambience music) has started.</summary>
   protected override void Start()
   {
      base.Start();

      PlayCrowdSound();
   }
}