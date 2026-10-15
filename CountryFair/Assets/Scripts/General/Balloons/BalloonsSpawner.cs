using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base class for anything that releases decorative balloons: picks a color with a balanced
/// distribution and flies each balloon upward with DOTween until it pops.
/// Subclasses (<c>CountryFairBalloonSpawner</c>, <c>ScoreAreaAnimations</c>) decide when and where to spawn.
/// </summary>
public class BalloonsSpawner : MonoBehaviour
{
    /// <summary>
    /// Prefab for the blue balloon used in the score animation.
    /// </summary>
    [Header("Balloons Prefabs")]
    [SerializeField]
    private GameObject _blueBalloonPrefab = null;

    /// <summary>
    /// Prefab for the red balloon used in the score animation.
    /// </summary>
   [SerializeField]
    private GameObject _redBalloonPrefab = null;

    /// <summary>
    /// Prefab for the yellow balloon used in the score animation.
    /// </summary>
    [SerializeField]
    private GameObject _yellowBalloonPrefab = null;

    /// <summary>Upper bound of balloons alive at once; <c>ScoreAreaAnimations</c> overwrites it with its placeholder count.</summary>
    [Header("Balloons Spawner Settings")]
    [SerializeField]
    protected int _maxBalloons = 5;

    /// <summary>Lower bound of balloons released per burst (used by <c>ScoreAreaAnimations</c>).</summary>
    [SerializeField]
    protected int _minBalloons = 0;

    /// <summary>
    /// Balloons currently flying. Decremented by <see cref="MoveBalloon"/> when a balloon pops; only subclasses
    /// that cap the live count (<c>CountryFairBalloonSpawner</c>) increment it.
    /// </summary>
    protected int _currentBalloons = 0;

    /// <summary>
    /// Minimum height (in units) that balloons will rise before exploding.
    /// </summary>
    [Header("Balloons Movement Settings")]
   [SerializeField]
    private float _minHeightToBalloonExplode = 3f;

    /// <summary>
    /// Maximum height (in units) that balloons will rise before exploding.
    /// </summary>
    [SerializeField]
    private float _maxHeightToBalloonExplode = 5f;

    /// <summary>
    /// Minimum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float _minFlyDuration = 1f;

    /// <summary>
    /// Maximum duration (in seconds) for balloons to fly upward before exploding.
    /// </summary>
    [SerializeField]
    private float _maxFlyDuration = 2f;

    /// <summary>How many balloons of each prefab have been spawned since the last <see cref="ResetBalloonCount"/>; drives <see cref="GetBalloonType"/>.</summary>
    private Dictionary<GameObject, int> _balloonTypesCount;

    /// <summary>Validates the three prefabs and initializes the per-color spawn counters.</summary>
    protected virtual void Awake()
    {
        if (_blueBalloonPrefab == null || _redBalloonPrefab == null || _yellowBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefabs are not assigned in SpawnBalloons script.");

            return;
        }

        _balloonTypesCount = new()
        {
            { _blueBalloonPrefab, 0 },
            { _redBalloonPrefab, 0 },
            { _yellowBalloonPrefab, 0 }
        };
    }

    /// <summary>
    /// Selects a balloon type (color) using a balanced distribution algorithm.
    /// Prioritizes balloon types that have been spawned the least to maintain color balance.
    /// </summary>
    /// <returns>A GameObject reference to the selected balloon prefab.</returns>
    protected GameObject GetBalloonType()
    {
        int minCount = _balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = _balloonTypesCount
            .Where(typeCount => typeCount.Value == minCount)
            .Select(typeCount => typeCount.Key)
            .ToArray();

        GameObject balloonType = candidates[Utils.RandomValueInRange(0, candidates.Length)];

        _balloonTypesCount[balloonType]++;

        return balloonType;
    }


    /// <summary>
    /// Flies <paramref name="balloon"/> upward by a random height over a random duration, then pops it
    /// and decrements <see cref="_currentBalloons"/>.
    /// </summary>
    /// <param name="balloon">Spawned balloon instance; must have a <see cref="PopBalloon"/> component.</param>
    /// <param name="balloonSpawnHeight">World Y the balloon was spawned at; the random rise is added on top of it.</param>
    protected void MoveBalloon(GameObject balloon, float balloonSpawnHeight)
    {
        float moveDuration = Utils.RandomValueInRange(_minFlyDuration, _maxFlyDuration);

        float addedHeight = Utils.RandomValueInRange(_minHeightToBalloonExplode, _maxHeightToBalloonExplode);
        float targetY = balloonSpawnHeight + addedHeight;

        balloon.transform.DOMoveY(targetY, moveDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                    balloon.GetComponent<PopBalloon>().Pop();

                    // Current Balloons variable maybe not used in some intherited classes
                    _currentBalloons--;
            });
    }

    /// <summary>Resets the per-color spawn counters so the next burst starts balanced again.</summary>
    protected void ResetBalloonCount()
    {
        _balloonTypesCount = new()
        {
            { _blueBalloonPrefab, 0 },
            { _redBalloonPrefab, 0 },
            { _yellowBalloonPrefab, 0 }
        };
    }
}