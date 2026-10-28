using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

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

    /// <summary>
    /// Pool the balloons are taken from and returned to when they pop. Colors are mixed in its queue: a reused balloon
    /// keeps its color, which is fine for these decorative balloons. Grown from <see cref="_maxBalloons"/> by
    /// <see cref="UpdateBalloonsPoolSize"/>.
    /// </summary>
    private ObjectPool _balloonsPool;

    /// <summary>Upper bound of balloons alive at once; <c>ScoreAreaAnimations</c> overwrites it with its placeholder count.</summary>
    [Header("Balloons Spawner Settings")]
    [SerializeField]
    protected int _maxBalloons = 5;

    /// <summary>Lower bound of balloons released per burst (used by <c>ScoreAreaAnimations</c>).</summary>
    [SerializeField]
    protected int _minBalloons = 0;

    /// <summary>
    /// Balloons currently flying. Decremented by <see cref="ReleaseBalloon"/> when a balloon pops; only subclasses
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
        if (!Utils.RequireAssigned(_blueBalloonPrefab, nameof(_blueBalloonPrefab), this) ||
            !Utils.RequireAssigned(_redBalloonPrefab, nameof(_redBalloonPrefab), this) ||
            !Utils.RequireAssigned(_yellowBalloonPrefab, nameof(_yellowBalloonPrefab), this))
        {
            return;
        }

        _balloonTypesCount = new()
        {
            { _blueBalloonPrefab, 0 },
            { _redBalloonPrefab, 0 },
            { _yellowBalloonPrefab, 0 }
        };

        _balloonsPool = new ObjectPool(_blueBalloonPrefab);
    }

    /// <summary>
    /// Selects a balloon type (color) using a balanced distribution algorithm.
    /// Prioritizes balloon types that have been spawned the least to maintain color balance.
    /// </summary>
    /// <returns>A GameObject reference to the selected balloon prefab.</returns>
    protected GameObject GetBalloonType()
    {
        return Utils.PickBalancedPrefab(_balloonTypesCount);
    }


    /// <summary>
    /// Pre-creates <see cref="_maxBalloons"/> balloons, split evenly between the colors, so the first bursts do not
    /// instantiate. Called by the subclasses at the end of <c>Awake</c>, once <see cref="_maxBalloons"/> has its final value.
    /// </summary>
    protected void UpdateBalloonsPoolSize()
    {
        if (_balloonsPool == null)
        {
            return;
        }

        int balloonsPerColor = Mathf.CeilToInt(_maxBalloons / (float)_balloonTypesCount.Count);

        foreach (GameObject balloonPrefab in _balloonTypesCount.Keys)
        {
            _balloonsPool.SetPrefab(balloonPrefab);
            _balloonsPool.UpdatePoolSize(_balloonsPool.PoolSize + balloonsPerColor);
        }
    }

    /// <summary>
    /// Takes a balloon from the pool at <paramref name="position"/> and flies it upward by a random height over a random
    /// duration; then pops it, returns it to the pool and decrements <see cref="_currentBalloons"/>.
    /// </summary>
    /// <param name="balloonPrefab">
    /// Color created if the pool is empty, usually from <see cref="GetBalloonType"/>; a reused balloon keeps its own color.
    /// Must have a <see cref="PopBalloon"/> component.
    /// </param>
    /// <param name="position">Spawn position; the random rise is added on top of its Y.</param>
    /// <remarks>
    /// If the spawner was destroyed mid-flight (a tutorial score area destroys itself on score), its pool is gone with
    /// it, so the balloon is destroyed instead.
    /// </remarks>
    protected void ReleaseBalloon(GameObject balloonPrefab, Vector3 position)
    {
        _balloonsPool.SetPrefab(balloonPrefab);

        GameObject balloon = _balloonsPool.GetObject(position, Quaternion.identity);

        float moveDuration = Utils.RandomValueInRange(_minFlyDuration, _maxFlyDuration);

        float addedHeight = Utils.RandomValueInRange(_minHeightToBalloonExplode, _maxHeightToBalloonExplode);
        float targetY = position.y + addedHeight;

        // Linked to the balloon so the tween dies with it when the scene unloads mid-flight.
        balloon.transform.DOMoveY(targetY, moveDuration)
            .SetEase(Ease.InSine)
            .SetLink(balloon)
            .OnComplete(() =>
            {
                    balloon.GetComponent<PopBalloon>().Pop();

                    // Unity's destroyed-object check on the spawner itself.
                    if (this != null)
                    {
                        _balloonsPool.ReturnObject(balloon);
                    }
                    else
                    {
                        Destroy(balloon);
                    }

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