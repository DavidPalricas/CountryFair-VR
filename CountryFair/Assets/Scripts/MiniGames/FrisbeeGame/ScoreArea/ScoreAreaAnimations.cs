using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the animation of the score area including rotating and pulsing effects, and the balloon burst
/// (inherited from <see cref="BalloonsSpawner"/>) played when the frisbee lands in it.
/// </summary>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ScoreAreaProperties))]
public class ScoreAreaAnimations : BalloonsSpawner
{
    /// <summary>
    /// Duration of one full rotation cycle.
    /// </summary>
    [Header("Rotate Animation Settings")]
    [SerializeField]
     private float _rotationDuration = 3f;

    /// <summary>
    /// Duration of one cycle of the pulse scaling animation.
    /// </summary>
    [SerializeField]
     private float _pulseDuration = 1.5f;

    /// <summary>
    /// Maximum scale multiplier for the pulse animation.
    /// </summary>
    [SerializeField]
     private float _pulseScale = 1.2f;

    /// <summary>
    /// Cached starting scale used as baseline for pulse animation.
    /// </summary>
    private Vector3 _startScale;


    /// <summary>Sibling component notified after the burst so the area is consumed (<see cref="ScoreAreaProperties.OnPlayerScore"/>).</summary>
    private ScoreAreaProperties _scoreAreaProperties;


    /// <summary>
    /// Transform containing child GameObjects that serve as spawn positions for balloons.
    /// </summary>
    [Header("Score Animation Settings")]
    [SerializeField]
    private Transform _balloonsPlaceHoldersGroup = null;

    /// <summary>
    /// Cached array of placeholder GameObjects used as spawn positions for balloons.
    /// Populated during Awake from the children of <see cref="_balloonsPlaceHoldersGroup"/>; its length also becomes <see cref="BalloonsSpawner._maxBalloons"/>.
    /// </summary>
    private GameObject[] _balloonsPlaceHolders = null;

    /// <summary>
    /// Caches the initial scale for animation reference, collects the balloon placeholders and caps
    /// the burst size to their count.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        
        _startScale = transform.localScale;

        if (_balloonsPlaceHoldersGroup == null)
        {
            Debug.LogError("Balloons PlaceHolders Group is not assigned in ScoreAreaAnim script.");

            return; 
        }

        _balloonsPlaceHolders = Utils.GetChildren(_balloonsPlaceHoldersGroup);

        _maxBalloons = _balloonsPlaceHolders.Length;

        _scoreAreaProperties = GetComponent<ScoreAreaProperties>();

        UpdateBalloonsPoolSize();
    }

    /// <summary>
    /// Starts the idle rotate/pulse loops every time the area is enabled, since areas are reused from the
    /// <see cref="FrisbeeGameManager"/>'s <see cref="ObjectPool"/> (and the dog's area is toggled on/off).
    /// </summary>
    private void OnEnable()
    {
        StartRotationAnimation();
    }

    /// <summary>Kills the loops while the area is in the pool and restores its scale, so the next pulse starts from <see cref="_startScale"/>.</summary>
    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = _startScale;
    }

    /// <summary>
    /// Initializes and starts the rotating and pulsing animations using DOTween.
    /// </summary>
    private void StartRotationAnimation()
    {
        transform.DORotate(new Vector3(0, 360, 0), _rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        transform.DOScale(_startScale * _pulseScale, _pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Kills this area's looping tweens so DOTween does not keep animating a destroyed transform.
    /// </summary>
    private void OnDestroy()
    {
        transform.DOKill();
    }

    /// <summary>
    /// Triggers the score animation by spawning and animating balloons.
    /// Randomly spawns between <see cref="BalloonsSpawner._minBalloons"/> and <see cref="BalloonsSpawner._maxBalloons"/>
    /// (exclusive), one per free placeholder, ensuring balanced distribution of colors.
    /// Each balloon flies upward and pops at a random height. Then consumes the area via <see cref="ScoreAreaProperties.OnPlayerScore"/>.
    /// </summary>
    /// <remarks>Called by <see cref="Landed"/> when the frisbee comes to rest inside this area.</remarks>
    public void ScoreAnimation()
    {    
        int balloonsToSpawn = Utils.RandomValueInRange(_minBalloons, _maxBalloons);

        List<GameObject> avaiblePlaceholders = _balloonsPlaceHolders.ToList();

        ResetBalloonCount();

        for (int i = 0; i < balloonsToSpawn; i++)
        {
            GameObject balloonType = GetBalloonType();

            Vector3 spawnPosition = GetBalloonSpawnPosition(avaiblePlaceholders);
            
            ReleaseBalloon(balloonType, spawnPosition);

        }

        _scoreAreaProperties.OnPlayerScore();
    }
  
    /// <summary>
    /// Selects a random spawn position from the available placeholders and removes it from the list.
    /// Ensures each balloon spawns at a unique position during a single score animation.
    /// </summary>
    /// <param name="avaiblePlaceholders">List of available placeholder GameObjects for balloon spawning.</param>
    /// <returns>The world position of the selected placeholder.</returns>
    private Vector3 GetBalloonSpawnPosition(List<GameObject> avaiblePlaceholders)
    {
        GameObject placeholder = avaiblePlaceholders[Utils.RandomValueInRange(0, avaiblePlaceholders.Count)];
        avaiblePlaceholders.Remove(placeholder);

        return placeholder.transform.position;
    }
}