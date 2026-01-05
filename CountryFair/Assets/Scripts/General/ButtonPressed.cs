using UnityEngine;
using UnityEngine.Events;

public class ButtonPressed : MonoBehaviour
{   
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> buttonPressed;
    private AudioManager.GameSoundEffects _buttonPressSoundEffect = AudioManager.GameSoundEffects.BUTTON_PRESSED;

    public void Pressed()
    {
        buttonPressed.Invoke(_buttonPressSoundEffect, gameObject);
    }
}