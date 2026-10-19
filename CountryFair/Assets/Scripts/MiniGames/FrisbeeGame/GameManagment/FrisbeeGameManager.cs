using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the Frisbee mini-game: spawns score areas around the player, applies DDA difficulty settings
/// for dog distance, target count, movement ratio, and visibility ratio.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class FrisbeeGameManager : MiniGameManager
{
    /// <summary>PlayerPrefs key of the saved Frisbee difficulty level; also read by <see cref="CarnyWise"/>.</summary>
    public const string DifficultyLevelKey = "FrisbeeDifficultyLevel";
    /// <summary>PlayerPrefs key of the current player-to-dog distance; read by <see cref="GoToTarget"/>.</summary>
    public const string DogDistanceKey = "DogDistance";
    /// <summary>Tag of the dog, used to measure its starting distance to the player.</summary>
    private const string DogTag = "Dog";
    /// <summary>Distance in meters added to the baseline dog distance per difficulty level.</summary>
    [Header("Dog Settings")]
    [SerializeField]
    private float _distanceIncrement = 2.5f;

    /// <summary>Maximum dog distance cap in meters, regardless of difficulty level.</summary>
    [SerializeField]
    private float _maxDogDistance = 20f;

    /// <summary>AnimationCurve controlling the fraction of score areas that move at a given difficulty saturation.</summary>
    [Header("Complexity Curves")]
    [SerializeField]
    private AnimationCurve _movingRatioCurve;
    /// <summary>AnimationCurve controlling the fraction of score areas that blink at a given difficulty saturation.</summary>
    [SerializeField]
    private AnimationCurve _visibilityRatioCurve;

    /// <summary>Prefab of the score area targets; <see cref="_scoreAreaPool"/> instantiates it when it needs more areas.</summary>
    [Header("Game Specific References")]
    [SerializeField]
    private GameObject _scoreAreaPrefab;
    /// <summary>Collider defining the valid spawn region for score areas around the player.</summary>
    [SerializeField]
    private Collider _dogAreaCollider;

    /// <summary>Collider marking the elevated dog-score area; score area spawns are excluded from this zone.</summary>
    [SerializeField]
    private Collider _dogScoreAreaCollider;

    /// <summary>Player transform used to calculate dog distance and score-area placement relative to the player.</summary>
    [SerializeField]
    private Transform _playerTransform;

    /// <summary>Fired when difficulty changes (after the first level), signaling the dog AI to reposition.</summary>
    [SerializeField]
    private UnityEvent _difficultyHasChanged;
    
    /// <summary>Fraction of score areas that orbit at the current level, from <see cref="_movingRatioCurve"/>.</summary>
    private float _currentMovingRatio;
    /// <summary>Fraction of score areas that blink at the current level, from <see cref="_visibilityRatioCurve"/>.</summary>
    private float _currentVisibilityRatio;

    /// <summary>Score areas are taken from and returned to this pool; grown to the target count on every difficulty change.</summary>
    private ObjectPool _scoreAreaPool;

    /// <summary>Bounding radius of the score area prefab, used to keep spawned areas from overlapping.</summary>
    private float _scoreAreaRadius = 0;

    /// <summary>Player-to-dog distance measured when the tutorial ends; level 0 keeps the dog where the scene placed it.</summary>
    private float _defaultDogDistance;

    /// <summary>
    /// Player-to-dog distance of the current level, cached from the <c>DogDistance</c> PlayerPrefs key so
    /// <see cref="GetRandomTargetPosition"/> does not read PlayerPrefs on every spawn.
    /// </summary>
    private float _currentDogDistance;

    /// <summary>Layer mask of <see cref="ScoreAreaProperties.ScoreAreaLayer"/>, resolved once in <see cref="Awake"/>.</summary>
    private int _scoreAreaLayerMask;


    /// <summary>
    /// Validates references, measures the score area radius, and clears the saved difficulty if the tutorial was never
    /// completed (the first session starts at level 0).
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
  
        if (_playerTransform == null)
        {
            Debug.LogError("Player not found. The rehabilitation system cannot start.");
            return;
        }

        if (_movingRatioCurve.length == 0 || _visibilityRatioCurve.length == 0)
        {
            Debug.LogError("One or more animation curves are not assigned in the inspector.");
        }

        _scoreAreaRadius = _scoreAreaPrefab.GetComponent<Renderer>().bounds.extents.magnitude;

        _scoreAreaPool = new ObjectPool(_scoreAreaPrefab);

        _scoreAreaLayerMask = LayerMask.GetMask(ScoreAreaProperties.ScoreAreaLayer);

        // Same value GetRandomTargetPosition read before the tutorial ends (_defaultDogDistance is still 0 here).
        _currentDogDistance = PlayerPrefs.GetFloat(DogDistanceKey, _defaultDogDistance);

        if (!GameManager.GetInstance().FrisbeeTutorialCompleted)
        {
            PlayerPrefs.DeleteKey(DifficultyLevelKey);
        }
    }

    /// <summary>Measures the baseline dog distance, restores the saved Frisbee difficulty and applies it.</summary>
    public override void TutorialCompleted()
    {   
        _defaultDogDistance =  GetPlayerDistanceToDog();
        
        DifficultyLevel = PlayerPrefs.GetInt(DifficultyLevelKey, 0);

        ApplyDifficultySettings();
    }

    /// <summary>
    /// Moves one level up or down (never below 0) and re-applies the settings. Above level 0 it first raises
    /// <see cref="_difficultyHasChanged"/> so the dog repositions.
    /// </summary>
    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {   
        if (DifficultyLevel > 0)
        {   
            _difficultyHasChanged.Invoke();
        }

        DifficultyLevel = isToIncreaseDiff ? DifficultyLevel + 1 : Mathf.Max(0, DifficultyLevel - 1);
        // Debug.Log($"<color=blue>DDA:</color> Adjusting difficulty to Level {DifficultyLevel} ({(isToIncreaseDiff ? "Increased" : "Decreased")})");
        ApplyDifficultySettings();

    }


    /// <summary>
    /// Derives the dog distance (written to the <c>DogDistance</c> PlayerPrefs key read by <c>GoToTarget</c>), the score
    /// area count and the moving/blinking ratios from the level, and saves the level. Score areas are not respawned
    /// here; that happens on <see cref="SpawnScoreAreas"/>.
    /// </summary>
    /// <remarks><c>level / (level + 4)</c> is a saturation factor in [0, 1) so the curves flatten out at high levels.</remarks>
    protected override void ApplyDifficultySettings()
    {
        float rawDistance = _defaultDogDistance + (DifficultyLevel * _distanceIncrement);
        float finalDogDistance = Mathf.Min(rawDistance, _maxDogDistance);

        _currentDogDistance = finalDogDistance;
        PlayerPrefs.SetFloat(DogDistanceKey, finalDogDistance);

        PlayerPrefs.SetInt(DifficultyLevelKey, DifficultyLevel);

        _currentDesiredCount = Mathf.RoundToInt(_targetsCount + (DifficultyLevel * _targetsPerLevel));

        float saturationFactor = DifficultyLevel / (DifficultyLevel + 4f);

        _currentMovingRatio = _movingRatioCurve.Evaluate(saturationFactor);
        _currentVisibilityRatio = _visibilityRatioCurve.Evaluate(saturationFactor);

        _scoreAreaPool.UpdatePoolSize(_currentDesiredCount);

        // Debug.Log($"[DDA] Level {DifficultyLevel} | Dist: {finalDogDistance:F1}m | Targets: {_currentDesiredCount} | Complexity: {saturationFactor:P0}");
    }

    /// <summary>Takes one score area from the pool at a free random position; <paramref name="prefabToSpawn"/> is ignored.</summary>
    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();
        GameObject newScoreArea = _scoreAreaPool.GetObject(pos, Quaternion.identity);
        _spawnedTargets.Add(newScoreArea);
    }

    /// <summary>Returns the oldest score area to the pool.</summary>
    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
            _spawnedTargets.RemoveAt(0);
            _scoreAreaPool.ReturnObject(target);
        }
    }

    /// <summary>Shuffles the score areas and makes the first N moving and the first M blinking, according to the current ratios.</summary>
    protected override void UpdateTargetsProperties()
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetVisibleCount = Mathf.RoundToInt(total * _currentVisibilityRatio);

        List<GameObject> shuffled = _spawnedTargets.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {   
            if (shuffled[i].TryGetComponent<ScoreAreaProperties>(out var scoreArea))
            {
                scoreArea.AdjustMovement( i < targetMovingCount);

                scoreArea.AdjustVisibility(i < targetVisibleCount); 
            }
        }
    }

    /// <summary>
    /// Random point around the player at 20–100 % of the current dog distance, at the dog area's height, inside the dog
    /// area and not overlapping another score area (layer <c>ScoreArea</c>).
    /// </summary>
    /// <remarks>
    /// Gives up after <c>MaxAttempts</c> failures instead of retrying without limit, which overflowed the stack when the
    /// area was too small for the requested count. The fallback is the last candidate clamped into the dog area, so the
    /// dog can still reach it even if it overlaps another score area.
    /// </remarks>
    protected override Vector3 GetRandomTargetPosition()
    {
        const float MinOffset = 0.2f;
        const float MaxOffset = 1f;
        const int MaxAttempts = 30;

        Bounds dogAreaBounds = _dogAreaCollider.bounds;

        Vector3 scoreAreaPosition = Vector3.zero;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            float scoreAreaDistance = _currentDogDistance * Random.Range(MinOffset, MaxOffset);

            scoreAreaPosition = _playerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * scoreAreaDistance;

            scoreAreaPosition.y = dogAreaBounds.center.y;

            bool collidesWithOtherAreas = Physics.CheckSphere(scoreAreaPosition, _scoreAreaRadius, _scoreAreaLayerMask);

            if (dogAreaBounds.Contains(scoreAreaPosition) && !collidesWithOtherAreas)
            {
                return scoreAreaPosition;
            }
        }

        return dogAreaBounds.ClosestPoint(scoreAreaPosition);
    }
    

    /// <summary>Returns a score area to the pool without replacing it and redistributes the properties of the remaining ones.</summary>
    public override void RemoveTarget(GameObject target, GameObject targetPrefab = null)
    {
        _spawnedTargets.Remove(target);
        _scoreAreaPool.ReturnObject(target);

        UpdateTargetsProperties();
    }


    /// <summary>
    /// Spawns or removes score areas to match the current desired count for the active difficulty level.
    /// </summary>
    /// <remarks>Invoked via Inspector by the dog's <c>_spawnScoreAreas</c> event (<see cref="DogIdle"/>) when a new round starts.</remarks>
    public void SpawnScoreAreas()
    {
        SyncTargets(_currentDesiredCount);
    }

    /// <summary>
    /// Returns all active score areas to the pool and clears the spawned-targets list.
    /// </summary>
    /// <remarks>Invoked via Inspector by the dog's <c>_removeScoreAreas</c> event and by <see cref="_difficultyHasChanged"/>, to clear the areas before a respawn.</remarks>
    public void RemoveScoreAreas()
    {
       if (_spawnedTargets.Count > 0)
        {
            foreach (GameObject target in _spawnedTargets)
            {
                _scoreAreaPool.ReturnObject(target);
            }

            _spawnedTargets.Clear();
        }
    }

    /// <summary>Distance from the player to the <c>Dog</c>-tagged object, or 0 if it is missing.</summary>
    private float GetPlayerDistanceToDog()
    {
        GameObject dog = GameObject.FindGameObjectWithTag(DogTag);

        if (dog == null)
        {
            Debug.LogError("Dog not found in the scene.");
            return 0f;
        }

        return Vector3.Distance(_playerTransform.position, dog.transform.position);
    }


    /// <summary>Raises <see cref="_difficultyHasChanged"/> so the dog repositions, then drops to level 0.</summary>
    public override void ResetDifficulty()
    {      
        _difficultyHasChanged.Invoke();
        base.ResetDifficulty();
    }
}