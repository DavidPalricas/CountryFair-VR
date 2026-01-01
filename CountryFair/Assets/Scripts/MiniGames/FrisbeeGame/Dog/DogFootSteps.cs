using UnityEngine;

public class DogFootSteps : MonoBehaviour
{
    private FrisbeeAudioManager _audioManager;

    private AudioManager.GameSoundEffects _footStepSoundEfect = AudioManager.GameSoundEffects.DOG_FOOTSTEPS;
    private void Start()
    {
        _audioManager = GameObject.Find("GameManager").GetComponent<FrisbeeAudioManager>();

        if (_audioManager == null)
        {
            Debug.LogError("FrisbeeAudioManager not found in the scene.");
        }
    }

    public void PlayFootSteps()
    {
        _audioManager.PlaySpatialSoundEffect(_footStepSoundEfect, gameObject);
    }
}
