using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bridges XR interactable button presses to the audio system via a UnityEvent.
/// Attach to any pressable prop and wire <see cref="buttonPressed"/> to an AudioManager in the Inspector.
/// </summary>
public class ButtonPressed : MonoBehaviour
{
    /// <summary>Wired in Inspector to the scene AudioManager's PlaySpatialSoundEffect method.</summary>
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> buttonPressed;

    /// <summary>Sound effect sent with every press; fixed to <see cref="AudioManager.GameSoundEffects.ButtonPressed"/>.</summary>
    private AudioManager.GameSoundEffects _buttonPressSoundEffect = AudioManager.GameSoundEffects.ButtonPressed;

    /// <summary>
    /// Invokes the button-pressed sound effect event, using this GameObject as the spatial audio source.
    /// </summary>
    /// <remarks>Invoked via Inspector by the button's XR Interactable (OnSelectEntered).</remarks>
    public void Pressed()
    {
        buttonPressed.Invoke(_buttonPressSoundEffect, gameObject);
    }
}