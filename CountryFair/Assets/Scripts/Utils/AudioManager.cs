using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager: MonoBehaviour
{
    [Header("---------- Events ----------")]
    [SerializeField]
    private EventReference dogBarkEvent;



    public void PlayDogBark(Vector3 position)
    {
        RuntimeManager.PlayOneShot(dogBarkEvent, position);
    }
}
