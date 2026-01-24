using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class BalloonsSpawner : MonoBehaviour 
{  
    [Header("Balloons Prefabs")]
    /// <summary>
    /// Prefab for the blue balloon used in the score animation.
    /// </summary>
    [SerializeField]
    private GameObject blueBalloonPrefab = null;

    /// <summary>
    /// Prefab for the red balloon used in the score animation.
    /// </summary>
   [SerializeField]
    private GameObject redBalloonPrefab = null;

    /// <summary>
    /// Prefab for the yellow balloon used in the score animation.
    /// </summary>
    [SerializeField] 
    private GameObject yellowBalloonPrefab = null;

    [Header("Balloons Spawner Settings")]
    [SerializeField]
    protected int maxBalloons = 5;
    
    [SerializeField]
    protected int minBalloons = 0;

    protected int currentBalloons = 0;

    [Header("Balloons Movement Settings")]
    /// <summary>
    /// Minimum height (in units) that balloons will rise before exploding.
    /// </summary>
   [SerializeField]
    private float minHeightToBalloonExplode = 3f;
    
    /// <summary>
    /// Maximum height (in units) that balloons will rise before exploding.
    /// </summary>
    [SerializeField]
    private float maxHeightToBalloonExplode = 5f;

    /// <summary>
    /// Minimum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float minflyDuration = 1f;

    /// <summary>
    /// Maximum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float maxflyDuration = 2f;

    private Dictionary<GameObject, int> balloonTypesCount;
 
    protected virtual void Awake()
    {   
        if (blueBalloonPrefab == null || redBalloonPrefab == null || yellowBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefabs are not assigned in SpawnBalloons script.");

            return;
        }

        balloonTypesCount = new()
        {
            { blueBalloonPrefab, 0 },
            { redBalloonPrefab, 0 },
            { yellowBalloonPrefab, 0 }
        };
    }

    /// <summary>
    /// Selects a balloon type (color) using a balanced distribution algorithm.
    /// Prioritizes balloon types that have been spawned the least to maintain color balance.
    /// </summary>
    /// <param name="balloonTypesCount">Dictionary tracking the count of each balloon type spawned.</param>
    /// <returns>A GameObject reference to the selected balloon prefab.</returns>
    protected GameObject GetBalloonType()
    {   
        int minCount = balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = balloonTypesCount
            .Where(typeCount => typeCount.Value == minCount)
            .Select(typeCount => typeCount.Key)
            .ToArray();

        GameObject balloonType = candidates[Utils.RandomValueInRange(0, candidates.Length)];

        balloonTypesCount[balloonType]++;

        return balloonType;
    }


    protected void MoveBalloon(GameObject balloon, float balloonSpawnHeight)
    {
        float moveDuration = Utils.RandomValueInRange(minflyDuration, maxflyDuration); 

        float addedHeight = Utils.RandomValueInRange(minHeightToBalloonExplode, maxHeightToBalloonExplode);
        float targetY = balloonSpawnHeight + addedHeight;

        balloon.transform.DOMoveY(targetY, moveDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() => 
            {  
                    balloon.GetComponent<PopBalloon>().Pop(); 
                    
                    // Current Balloons variable maybe not used in some intherited classes
                    currentBalloons--;
            });
    }
    
    protected void ResetBalloonCount()
    {
        balloonTypesCount = new()
        {
            { blueBalloonPrefab, 0 },
            { redBalloonPrefab, 0 },
            { yellowBalloonPrefab, 0 }
        };
    }
}