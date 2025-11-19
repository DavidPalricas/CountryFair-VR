using System.Collections.Generic;
using UnityEngine;

public class FrisbeeGameManager : MonoBehaviour
{
    public Dictionary<string, float> AdaptiveParameters {get; private set;} = new Dictionary<string, float>();


   [HideInInspector]
    public Vector3 currentTargetPos = Vector3.zero;

    private void Start()
    {
        SetAdaptiveParameters();
    }
    
    private void SetAdaptiveParameters()
    {
        AdaptiveParameters["DogDistance"] = GetPlayerDistanceToDog();
    }

    private float GetPlayerDistanceToDog()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

        if (player == null || dog == null)
        {
            Debug.LogError("Player or Dog GameObject not found in the scene.");
            return 0f;  
        }

        return Vector3.Distance(player.transform.position, dog.transform.position);
    } 
}
