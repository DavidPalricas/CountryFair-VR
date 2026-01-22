using UnityEngine;

/// <summary>
/// Spawns and manages a pool of balloons within a defined area.
/// Tracks active balloon count and respawns new balloons up to the maximum limit.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CountryFairBalloonSpawner : BalloonsSpawner
{   
    [Header ("Extra Spawner Settings")]

    /// <summary>Minimum time interval between balloon spawns in seconds.</summary>
    [SerializeField]
    private float minSpawnInterval = 0.5f;

    /// <summary>Maximum time interval between balloon spawns in seconds.</summary>
    [SerializeField]
    private float maxSpawnInterval = 1f;

    /// <summary>Initial spawn height for new balloons.</summary>
    [SerializeField]
    private float balloonsInitialHeight = 2f;

    /// <summary>Collider component defining the spawn area bounds.</summary>
    private Collider areaCollider;

    /// <summary>
    /// Initializes the spawner by retrieving the collider component.
    /// Logs an error if the collider is not found.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();

        areaCollider = GetComponent<Collider>();

        if (areaCollider == null)
        {
            Debug.LogError("Spawn area GameObject must have a Collider component!");
        }
    }

    /// <summary>
    /// Called every frame. Spawns balloons at random intervals while below maximum count.
    /// </summary>
    private void Update()
    {
        if (currentBalloons < maxBalloons)
        {
            Invoke(nameof(SpawnBalloon), Utils.RandomValueInRange(minSpawnInterval, maxSpawnInterval));
        }
    }

    /// <summary>
    /// Spawns a single balloon at a random position within the collider bounds.
    /// Assigns this spawner as a reference to the balloon.
    /// </summary>
    private void SpawnBalloon()
    {  
        GameObject balloonType = GetBalloonType();

        GameObject balloon = Instantiate(balloonType, GetRandomPositionInBounds(), Quaternion.identity);

        MoveBalloon(balloon, balloonsInitialHeight);

        currentBalloons++;
    }

    /// <summary>
    /// Generates a random position within the collider bounds at the initial height.
    /// </summary>
    /// <returns>A random Vector3 position within the spawn area.</returns>
    private Vector3 GetRandomPositionInBounds()
    {
        Bounds bounds = areaCollider.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, balloonsInitialHeight, randomZ);
    }
}