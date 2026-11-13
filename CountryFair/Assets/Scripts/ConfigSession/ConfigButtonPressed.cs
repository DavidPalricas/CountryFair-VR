using UnityEngine;
using UnityEngine.Events;

public class ConfigButtonPressed : ButtonPressed
{
    /// <summary>Wired in Inspector to the scene ConfigSessionManager's OnConfigButtonPressed method.</summary>
    public UnityEvent<ConfigButtonPressed> configButtonPressed;

    /// <summary>
    /// Invokes the button-pressed sound effect event, using this GameObject as the spatial audio source.
    /// </summary>
    /// <remarks>Invoked via Inspector by the button's XR Interactable (OnSelectEntered).</remarks>
    public override void Pressed()
    {
        base.Pressed();

        configButtonPressed.Invoke(this);
    }
}