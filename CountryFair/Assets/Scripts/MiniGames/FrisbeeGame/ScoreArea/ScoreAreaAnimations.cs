using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles the animation of the score area including floating, rotating, and pulsing effects.
/// </summary>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ScoreAreaProperties))]
public class ScoreAreaAnimations : BalloonsSpawner
{
    [Header("Rotate Animation Settings")]
    /// <summary>
    /// Duration of one cycle of the floating animation.
    /// </summary>
    [SerializeField]
     private float floatDuration = 2f;

    /// <summary>
    /// Duration of one full rotation cycle.
    /// </summary>
    [SerializeField]
     private float rotationDuration = 3f;

    /// <summary>
    /// Duration of one cycle of the pulse scaling animation.
    /// </summary>
    [SerializeField]
     private float pulseDuration = 1.5f;

    /// <summary>
    /// Maximum scale multiplier for the pulse animation.
    /// </summary>
    [SerializeField]
     private float pulseScale = 1.2f;

    /// <summary>
    /// Cached starting position used as baseline for floating animation.
    /// </summary>
    private Vector3 _startPosition;

    /// <summary>
    /// Cached starting scale used as baseline for pulse animation.
    /// </summary>
    private Vector3 _startScale;

    /// <summary>
    /// DOTween sequence controlling all active animations.
    /// </summary>
    private Sequence _animSequence;


    private ScoreAreaProperties _scoreAreaProperties;


    [Header("Score Animation Settings")]
    /// <summary>
    /// Transform containing child GameObjects that serve as spawn positions for balloons.
    /// </summary>
    [SerializeField]
    private Transform balloonsPlaceHoldersGroup = null;

    /// <summary>
    /// Cached array of placeholder GameObjects used as spawn positions for balloons.
    /// Populated during Awake from the children of balloonsPlaceHoldersGroup.
    /// </summary>
    private GameObject[] _balloonsPlaceHolders = null;

    /// <summary>
    /// Caches the initial position and scale for animation reference.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        
        _startPosition = transform.position;
        _startScale = transform.localScale;

        if (balloonsPlaceHoldersGroup == null)
        {
            Debug.LogError("Balloons PlaceHolders Group is not assigned in ScoreAreaAnim script.");

            return; 
        }

        _balloonsPlaceHolders = Utils.GetChildren(balloonsPlaceHoldersGroup);

        maxBalloons = _balloonsPlaceHolders.Length;

        _scoreAreaProperties = GetComponent<ScoreAreaProperties>();
    }

    /// <summary>
    /// Begins the animation sequence.
    /// Unity callback called before the first frame update.
    /// </summary>
    private void Start()
    {
        StartRotationAnimation();
    }

    /// <summary>
    /// Initializes and starts the floating, rotating, and pulsing animations using DOTween.
    /// </summary>
    private void StartRotationAnimation()
    {
        _animSequence = DOTween.Sequence();

        const float FLOAT_HEIGHT = 0f;

        transform.DOMoveY(_startPosition.y + FLOAT_HEIGHT, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        transform.DOScale(_startScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Cleans up DOTween animations when the object is destroyed.
    /// Unity callback called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
        transform.DOKill();
        _animSequence?.Kill();
    }

    /// <summary>
    /// Triggers the score animation by spawning and animating balloons.
    /// Randomly spawns between minBaloons and maxBaloons, ensuring balanced distribution of colors.
    /// Each balloon flies upward and pops at a random height.
    /// </summary>
    public void ScoreAnimation()
    {    
        int balloonsToSpawn = Utils.RandomValueInRange(minBalloons, maxBalloons);

        List<GameObject> avaiblePlaceholders = _balloonsPlaceHolders.ToList();

        ResetBalloonCount();

        for (int i = 0; i < balloonsToSpawn; i++)
        {
            GameObject balloonType = GetBalloonType();

            Vector3 spawnPosition = GetBalloonSpawnPosition(avaiblePlaceholders);
            
            GameObject balloon = Instantiate(balloonType, spawnPosition, Quaternion.identity);

            MoveBalloon(balloon, spawnPosition.y);
            
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