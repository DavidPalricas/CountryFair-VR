using UnityEngine;

/// <summary>
/// Spawns and manages balloons (taken from an <see cref="ObjectPool"/>) within a defined area.
/// Tracks active balloon count and respawns new balloons up to the maximum limit.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CountryFairBalloonSpawner : BalloonsSpawner
{   
    [Header ("Extra Spawner Settings")]

    /// <summary>Minimum time interval between balloon spawns in seconds.</summary>
    [SerializeField]
    private float _minSpawnInterval = 0.5f;

    /// <summary>Maximum time interval between balloon spawns in seconds.</summary>
    [SerializeField]
    private float _maxSpawnInterval = 1f;

    /// <summary>Initial spawn height for new balloons.</summary>
    [SerializeField]
    private float _balloonsInitialHeight = 2f;

    /// <summary>Collider component defining the spawn area bounds.</summary>
    private Collider _areaCollider;

    /// <summary>
    /// <see cref="Time.time"/> at which the next balloon may spawn. A single timer means at most one spawn is pending at
    /// a time, so the live count never overshoots <see cref="BalloonsSpawner._maxBalloons"/>.
    /// </summary>
    private float _nextSpawnTime = 0f;

    /// <summary>
    /// Initializes the spawner by retrieving the collider component.
    /// Logs an error if the collider is not found.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();

        _areaCollider = GetComponent<Collider>();

        if (_areaCollider == null)
        {
            Debug.LogError("Spawn area GameObject must have a Collider component!");
        }

        UpdateBalloonsPoolSize();
    }

    /// <summary>
    /// While below <c>_maxBalloons</c>, spawns one balloon each time <see cref="_nextSpawnTime"/> is reached, then waits
    /// a random delay in [<see cref="_minSpawnInterval"/>, <see cref="_maxSpawnInterval"/>] before the next one.
    /// </summary>
    /// <remarks>
    /// Replaces an <c>Invoke</c> scheduled every frame: since the counter only grew when a spawn fired, dozens of spawns
    /// were queued per refill and the live count overshot the cap.
    /// </remarks>
    private void Update()
    {
        if (_currentBalloons < _maxBalloons && Time.time >= _nextSpawnTime)
        {
            SpawnBalloon();

            _nextSpawnTime = Time.time + Utils.RandomValueInRange(_minSpawnInterval, _maxSpawnInterval);
        }
    }

    /// <summary>
    /// Takes a single balloon from the pool at a random position within the collider bounds.
    /// Delegates the balloon's movement/pop bookkeeping to <c>ReleaseBalloon</c>, which decrements <c>_currentBalloons</c> when it pops.
    /// </summary>
    private void SpawnBalloon()
    {
        GameObject balloonType = GetBalloonType();

        ReleaseBalloon(balloonType, GetRandomPositionInBounds());

        _currentBalloons++;
    }

    /// <summary>
    /// Generates a random position within the collider bounds at the initial height.
    /// </summary>
    /// <returns>A random Vector3 position within the spawn area.</returns>
    private Vector3 GetRandomPositionInBounds()
    {
        Bounds bounds = _areaCollider.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, _balloonsInitialHeight, randomZ);
    }
}