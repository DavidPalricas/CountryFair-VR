using UnityEngine;
using UnityEngine.Events;

public class ConfigButton : GameButton
{   
    [Header("Config Button Parent GameObject")]
    [SerializeField]
    private GameObject _configButton;
    
    [Header("Config Button Events")]
    /// <summary>Wired in Inspector to the scene ConfigSessionManager's OnConfigButtonPressed method.</summary>
    [SerializeField]
    private UnityEvent<ConfigButton> _configButtonPressed;
    
    [SerializeField]
    private UnityEvent<AudioManager.GameSoundEffects, GameObject> _playAppearedSound;

     private readonly AudioManager.GameSoundEffects _appeared = AudioManager.GameSoundEffects.ConfigButtonApeared;

   private void Awake()
    {
        if (!Utils.RequireAssigned(_configButton, nameof(_configButton), this))
        {
            return;
        }
    }

    /// <summary>
    /// Invokes the button-pressed sound effect event, using this GameObject as the spatial audio source.
    /// </summary>
    /// <remarks>Invoked via Inspector by the button's XR Interactable (OnSelectEntered).</remarks>
    
    public override void Pressed()
    {
        base.Pressed();

        _configButtonPressed.Invoke(this);
    }

    public void ToggleConfigButton(bool isActive)
    {
        _configButton.SetActive(isActive);

        if (isActive)
        {
            _playAppearedSound.Invoke(_appeared, _configButton);
        }
    }
}