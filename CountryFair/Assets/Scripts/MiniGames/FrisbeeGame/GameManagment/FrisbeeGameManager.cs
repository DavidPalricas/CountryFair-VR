using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Manages the Frisbee mini-game: spawns score areas around the player, applies DDA difficulty settings
/// for dog distance, target count, movement ratio, and visibility ratio.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class FrisbeeGameManager : MiniGameManager
{
    /// <summary>Distance in meters added to the baseline dog distance per difficulty level.</summary>
    [Header("Dog Settings")]
    [SerializeField]
    [FormerlySerializedAs("distanceIncrement")]
    private float _distanceIncrement = 2.5f;

    /// <summary>Maximum dog distance cap in meters, regardless of difficulty level.</summary>
    [SerializeField]
    [FormerlySerializedAs("maxDogDistance")]
    private float _maxDogDistance = 20f;

    /// <summary>AnimationCurve controlling the fraction of score areas that move at a given difficulty saturation.</summary>
    [Header("Complexity Curves")]
    [SerializeField]
    [FormerlySerializedAs("movingRatioCurve")]
    private AnimationCurve _movingRatioCurve;
    /// <summary>AnimationCurve controlling the fraction of score areas that blink at a given difficulty saturation.</summary>
    [SerializeField]
    [FormerlySerializedAs("visibilityRatioCurve")]
    private AnimationCurve _visibilityRatioCurve;

    /// <summary>Prefab instantiated for each score area target.</summary>
    [Header("Game Specific References")]
    [SerializeField]
    [FormerlySerializedAs("scoreAreaPrefab")]
    private GameObject _scoreAreaPrefab;
    /// <summary>Collider defining the valid spawn region for score areas around the player.</summary>
    [SerializeField]
    [FormerlySerializedAs("dogAreaCollider")]
    private Collider _dogAreaCollider;

    /// <summary>Collider marking the elevated dog-score area; score area spawns are excluded from this zone.</summary>
    [SerializeField]
    [FormerlySerializedAs("dogScoreAreaCollider")]
    private Collider _dogScoreAreaCollider;

    /// <summary>Player transform used to calculate dog distance and score-area placement relative to the player.</summary>
    [SerializeField]
    [FormerlySerializedAs("playerTransform")]
    private Transform _playerTransform;

    /// <summary>Fired when difficulty changes (after the first level), signaling the dog AI to reposition.</summary>
    [SerializeField]
    [FormerlySerializedAs("difficultyHasChanged")]
    private UnityEvent _difficultyHasChanged;
    
    /// <summary>Ratios derived from the difficulty in <see cref="ApplyDifficultySettings"/> and applied to the score areas.</summary>
    private float _currentMovingRatio;
    private float _currentVisibilityRatio;

    /// <summary>Bounding radius of the score area prefab, used to keep spawned areas from overlapping.</summary>
    private float _scoreAreaRadius = 0;

    /// <summary>Player-to-dog distance measured when the tutorial ends; level 0 keeps the dog where the scene placed it.</summary>
    private float _defaultDogDistance;


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

        if (!GameManager.GetInstance().FrisbeeTutorialCompleted)
        {
            PlayerPrefs.DeleteKey("FrisbeeDifficultyLevel");
        }
    }

    /// <summary>Measures the baseline dog distance, restores the saved Frisbee difficulty and applies it.</summary>
    public override void TutorialCompleted()
    {   
        _defaultDogDistance =  GetPlayerDistanceToDog();
        
        DifficultyLevel = PlayerPrefs.GetInt("FrisbeeDifficultyLevel", 0);

        Debug.Log("Current Difficulty Level: " + DifficultyLevel);

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
        
        PlayerPrefs.SetFloat("DogDistance", finalDogDistance);

        PlayerPrefs.SetInt("FrisbeeDifficultyLevel", DifficultyLevel);

        _currentDesiredCount = Mathf.RoundToInt(_targetsCount + (DifficultyLevel * _targetsPerLevel));

        float saturationFactor = DifficultyLevel / (DifficultyLevel + 4f);

        _currentMovingRatio = _movingRatioCurve.Evaluate(saturationFactor);
        _currentVisibilityRatio = _visibilityRatioCurve.Evaluate(saturationFactor);

        // Debug.Log($"[DDA] Level {DifficultyLevel} | Dist: {finalDogDistance:F1}m | Targets: {_currentDesiredCount} | Complexity: {saturationFactor:P0}");
    }

    /// <summary>Adds or removes score areas until there are <paramref name="desiredCount"/>, then redistributes movement/visibility.</summary>
    protected override void SyncTargets(int desiredCount)
    {
        while (_spawnedTargets.Count < desiredCount)
        {
            AddTarget();
        }
        
        while (_spawnedTargets.Count > desiredCount)
        {
            RemoveTarget();
        }

        UpdateTargetsProperties();
    }

    /// <summary>Spawns one score area at a free random position; <paramref name="prefabToSpawn"/> is ignored.</summary>
    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();
        GameObject newScoreArea = Instantiate(_scoreAreaPrefab, pos, Quaternion.identity);
        _spawnedTargets.Add(newScoreArea);
    }

    /// <summary>Destroys the oldest score area.</summary>
    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
            _spawnedTargets.RemoveAt(0);
            Destroy(target);
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
    /// <remarks>Retries recursively with no attempt limit, so an area too small for the requested count recurses until it overflows the stack.</remarks>
    protected override Vector3 GetRandomTargetPosition()
    {
        float currentDogDist = PlayerPrefs.GetFloat("DogDistance", _defaultDogDistance);
        
        const float MinOffset = 0.2f;
        const float MaxOffset = 1f;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        float scoreAreaDistance = currentDogDist * Random.Range(MinOffset, MaxOffset);

        Vector3 scoreAreaPosition = _playerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * scoreAreaDistance;

        scoreAreaPosition.y = _dogAreaCollider.bounds.center.y;

        int layerMask = LayerMask.GetMask("ScoreArea");
        
        bool collidesWithOtherAreas = Physics.CheckSphere(scoreAreaPosition, _scoreAreaRadius, layerMask);

        if (!_dogAreaCollider.bounds.Contains(scoreAreaPosition) || collidesWithOtherAreas)
        {
            return GetRandomTargetPosition();
        }

        return scoreAreaPosition;
    }
    

    /// <summary>Destroys a score area without replacing it and redistributes the properties of the remaining ones.</summary>
    public override void DestroyTarget(GameObject target, GameObject targetPrefab = null)
    {
        _spawnedTargets.Remove(target);
        Destroy(target);

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
    /// Destroys all active score areas and clears the spawned-targets list.
    /// </summary>
    /// <remarks>Invoked via Inspector by the dog's <c>_removeScoreAreas</c> event and by <see cref="_difficultyHasChanged"/>, to clear the areas before a respawn.</remarks>
    public void RemoveScoreAreas()
    {
       if (_spawnedTargets.Count > 0)
        {
            foreach (GameObject target in _spawnedTargets)
            {
                Destroy(target);
            }

            _spawnedTargets.Clear();
        }
    }

    /// <summary>Distance from the player to the <c>Dog</c>-tagged object, or 0 if it is missing.</summary>
    private float GetPlayerDistanceToDog()
    {
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

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