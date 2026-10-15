using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.Serialization;

/// <summary>
/// Manages the Archery mini-game: spawns colored balloons inside the spawn area, applies DDA difficulty
/// curves for movement and transparency ratios, and randomly selects the scoring balloon color each round.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class ArcheryGameManager : MiniGameManager
{
    /// <summary>TMP text displaying the balloon color that scores; its text and color are set to match the target balloon.</summary>
    [Header("Game Specific References")]
    [FormerlySerializedAs("balloonColorToScoreText")]
    [SerializeField] private TextMeshProUGUI _balloonColorToScoreText;
    /// <summary>Collider whose world bounds define the volume where balloons are spawned (separate from the <c>BalloonSpawn</c> box balloons wander in).</summary>
    [FormerlySerializedAs("balloonSpawnArea")]
    [SerializeField] private Collider _balloonSpawnArea;

    /// <summary>
    /// Prefab of the blue balloon. Its root is what gets spawned and destroyed; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>BLUE</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [Header("Balloons Prefabs")]
    [SerializeField]
     [FormerlySerializedAs("blueBalloonPrefab")]
     private GameObject _blueBalloonPrefab;
    /// <summary>
    /// Prefab of the red balloon. Its root is what gets spawned and destroyed; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>RED</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("redBalloonPrefab")]
    private GameObject _redBalloonPrefab;
    /// <summary>
    /// Prefab of the yellow balloon. Its root is what gets spawned and destroyed; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>YELLOW</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("yellowBalloonPrefab")]
    private GameObject _yellowBalloonPrefab;

    /// <summary>
    /// Fraction of balloons (Y, 0–1) that move, as a function of the difficulty saturation factor (X, 0–1; see
    /// <see cref="ApplyDifficultySettings"/>). Must have at least one key.
    /// </summary>
    [Header("Complexity Curves")]
    [SerializeField]
    [FormerlySerializedAs("movingRatioCurve")]
    private AnimationCurve _movingRatioCurve;
    /// <summary>
    /// Fraction of balloons (Y, 0–1) with the fade cycle on, as a function of the difficulty saturation factor (X, 0–1).
    /// Must have at least one key. Moving and transparent balloons are chosen independently, so one balloon can be both.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("transparencyRatioCurve")]
    private AnimationCurve _transparencyRatioCurve;

    /// <summary>
    /// Seconds a balloon takes between two wander points at level 0 (larger is slower). The duration is interpolated
    /// towards <see cref="_fastMoveDuration"/> by the saturation factor, so it is never fully reached.
    /// </summary>
    [Header("Speed Progression")]
    [Tooltip("Movement duration in lowest difficulty (Bigger is slower). Ex: 4s")]
    [SerializeField]
    [FormerlySerializedAs("slowMoveDuration")]
    private float _slowMoveDuration = 6.0f;

    /// <summary>Seconds between two wander points that the duration approaches at very high levels (smaller is faster).</summary>
    [Tooltip("Movement duration in highest difficulty (Smaller is faster). Ex: 1s" )]
    [SerializeField]
    [FormerlySerializedAs("fastMoveDuration")]
    private float _fastMoveDuration = 1.5f;

    /// <summary>Punch strength of the scoring-color text animation, added to its scale on every axis (1.2 → grows to 2.2× at the peak).</summary>
    [Header("Color to Score Animation Settings")]
    [SerializeField]
    [FormerlySerializedAs("punchScale")]
    private float _punchScale = 1.2f;
    /// <summary>Duration in seconds of the DOPunchScale animation on the balloon-color text.</summary>
    [SerializeField]
    [FormerlySerializedAs("punchDuration")]
    private float _punchDuration = 0.5f;

    /// <summary>Live balloons per prefab, used by <see cref="GetBalloonType"/> to keep the colors balanced.</summary>
    private Dictionary<GameObject, int> _balloonTypesCount;
    /// <summary>Prefab of the color that currently scores; always kept present in the scene by <see cref="DestroyTarget"/>.</summary>
    private GameObject _balloonPrefabToScore;
    
    /// <summary>Fraction of balloons that move at the current level, from <see cref="_movingRatioCurve"/>.</summary>
    private float _currentMovingRatio;
    /// <summary>Fraction of balloons with the fade cycle on at the current level, from <see cref="_transparencyRatioCurve"/>.</summary>
    private float _currentTransparencyRatio;
    /// <summary>Wander duration given to every balloon at the current level (between <see cref="_slowMoveDuration"/> and <see cref="_fastMoveDuration"/>).</summary>
    private float _currentMoveDuration; 


    /// <summary>
    /// Validates references, initializes the color counters, clears the saved difficulty if the tutorial was
    /// never completed (first session starts at level 0), and picks the first scoring color.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake(); 

        if (_balloonColorToScoreText == null)
        {
            Debug.LogError("Balloon Color To Score Text reference is missing.");
            return;
        }

        if (_balloonSpawnArea == null)
        {
            Debug.LogError("Balloon Spawn Area reference is missing.");
            return;
        }

        if (_blueBalloonPrefab == null || _yellowBalloonPrefab == null || _redBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefab references are missing.");
            return;
        }

        if (_movingRatioCurve.length == 0 || _transparencyRatioCurve.length == 0)
        {
            Debug.LogError("One or more difficulty curves are not set.");
            return;
        }

        _balloonTypesCount = new Dictionary<GameObject, int>
        {
            { _blueBalloonPrefab, 0 },
            { _redBalloonPrefab, 0 },
            { _yellowBalloonPrefab, 0 }
        };

          if (!GameManager.GetInstance().ArcheryTutorialCompleted)
        {
            PlayerPrefs.DeleteKey("ArcheryDifficultyLevel");
        }

        SetBalloonColorToScore();
    }

    /// <summary>Restores the saved archery difficulty and spawns the balloons for it.</summary>
    public override void TutorialCompleted()
    {   
        DifficultyLevel = PlayerPrefs.GetInt("ArcheryDifficultyLevel", 0);
        ApplyDifficultySettings();
    }

    /// <summary>Moves one difficulty level up or down (never below 0) and re-applies the settings.</summary>
    /// <remarks>
    /// A decrease at level 0 returns early without calling <see cref="ApplyDifficultySettings"/>: nothing would change,
    /// and re-applying would re-roll the scoring color for no reason.
    /// </remarks>
    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {
        if (!isToIncreaseDiff && DifficultyLevel == 0)
        {
            return;
        }

        DifficultyLevel = isToIncreaseDiff ? DifficultyLevel + 1 : DifficultyLevel - 1;
        // Debug.Log($"<color=orange>ARCHERY DDA:</color> Level {DifficultyLevel}");
        ApplyDifficultySettings();
    }

    /// <summary>
    /// Derives the balloon count, moving/transparent ratios and move duration from the level, saves the level,
    /// re-rolls the scoring color and syncs the balloons.
    /// </summary>
    /// <remarks>
    /// <c>level / (level + 4)</c> is a saturation factor in [0, 1): it rises quickly at low levels and flattens out,
    /// so the curves never run past their end however high the level goes.
    /// </remarks>
    protected override void ApplyDifficultySettings()
    {    
        _currentDesiredCount = Mathf.RoundToInt(_targetsCount + (DifficultyLevel * _targetsPerLevel));

        float saturationFactor = DifficultyLevel / (DifficultyLevel + 4f); 
        _currentMovingRatio = _movingRatioCurve.Evaluate(saturationFactor);
        _currentTransparencyRatio = _transparencyRatioCurve.Evaluate(saturationFactor);

        _currentMoveDuration = Mathf.Lerp(_slowMoveDuration, _fastMoveDuration, saturationFactor);

        PlayerPrefs.SetInt("ArcheryDifficultyLevel", DifficultyLevel);
        
        // Debug.Log($"[Archery Stats] Total Balloons: {_currentDesiredCount} | Movimento Ratio: {_currentMovingRatio:P0} | Move Duration: {_currentMoveDuration:F1}s");
        
        SetBalloonColorToScore();
        SyncTargets(_currentDesiredCount);
    }

    /// <summary>Adds or removes balloons until there are <paramref name="desiredCount"/>, then redistributes movement/transparency.</summary>
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

    /// <summary>Spawns a balloon at a free position; when no prefab is given, the color is chosen by <see cref="GetBalloonType"/>.</summary>
    /// <remarks>An explicit <paramref name="prefabToSpawn"/> does not increment <see cref="_balloonTypesCount"/>.</remarks>
    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();
        
        if (prefabToSpawn == null)
        {
            prefabToSpawn = GetBalloonType();
        }

        GameObject newBalloon = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        BalloonArcheryGame balloonComponent = newBalloon.GetComponentInChildren<BalloonArcheryGame>();
        
        balloonComponent.OriginalPrefab = prefabToSpawn;
            
        _spawnedTargets.Add(newBalloon);
    }

    /// <summary>Destroys the oldest balloon.</summary>
    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
 
            BalloonArcheryGame balloonComponent = target.GetComponentInChildren<BalloonArcheryGame>();

            DestroyTarget(target, balloonComponent.OriginalPrefab);
        }  
    }

    /// <summary>
    /// Destroys a balloon (popped or removed) and, if the scene is below the desired count, spawns a replacement,
    /// forcing the scoring color when none of it is left so the player always has a valid target.
    /// </summary>
    /// <param name="target">Root GameObject of the balloon instance.</param>
    /// <param name="targetPrefab">Prefab it was spawned from, used to update the color counters.</param>
    public override void DestroyTarget(GameObject target, GameObject targetPrefab)
    {   
        _balloonTypesCount[targetPrefab] = Mathf.Max(0, _balloonTypesCount[targetPrefab] - 1);
        
        _spawnedTargets.Remove(target);

        Destroy(target);

        if (_spawnedTargets.Count < _currentDesiredCount)
        {
            if (_balloonTypesCount[_balloonPrefabToScore] == 0)
            {
                AddTarget(_balloonPrefabToScore);
            }
            else
            {
                AddTarget();
            }
            
            UpdateTargetsProperties();
        }
    }

    /// <summary>Shuffles the balloons and makes the first N moving and the first M transparent, according to the current ratios.</summary>
    protected override void UpdateTargetsProperties()
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetTransparentCount = Mathf.RoundToInt(total * _currentTransparencyRatio);

        List<GameObject> shuffled = _spawnedTargets.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {    
            BalloonArcheryGame balloonComponent = shuffled[i].GetComponentInChildren<BalloonArcheryGame>();

            balloonComponent.SetMoveDuration(_currentMoveDuration);

            balloonComponent.AdjustMovement(i < targetMovingCount);
            balloonComponent.AdjustTransparency(i < targetTransparentCount);
        }
    }

    /// <summary>
    /// Random point in the spawn area at least <c>SafetyRadius</c> away from other balloons; after
    /// <c>MaxAttempts</c> failures the last candidate is used even if it overlaps.
    /// </summary>
    /// <remarks>
    /// The attempt limit replaced an unbounded retry that overflowed the stack when the area was crowded (e.g. a high
    /// difficulty carried over from a previous session). Only colliders tagged <c>Balloon</c> count as overlaps.
    /// </remarks>
    protected override Vector3 GetRandomTargetPosition()
    {
        Bounds bounds = _balloonSpawnArea.bounds;

        const float SafetyRadius = 0.3f;
        const int MaxAttempts = 30;

        Vector3 candidatePos = Vector3.zero;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);

            candidatePos = new (x, y, z);

            Collider[] hitColliders = Physics.OverlapSphere(candidatePos, SafetyRadius);
            bool hitBalloon = false;

            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag("Balloon"))
                {
                    hitBalloon = true;
                    break;
                }
            }

            if (!hitBalloon)
            {
                return candidatePos;
            }
        }

        return candidatePos;
    }

    /// <summary>
    /// Picks a random scoring color, shows its Portuguese name in that color with a punch animation, and stores it in
    /// the <c>BalloonColorToScore</c> PlayerPrefs key read by <see cref="BalloonArcheryGame.GetScoreValue"/>.
    /// </summary>
    /// <remarks>
    /// The color can repeat the previous one. Running tweens on the text are killed first so repeated calls do not
    /// stack punches; the punch uses 5 vibrations and 0.5 elasticity.
    /// </remarks>
    private void SetBalloonColorToScore()
    {   
        int randomColorIndex = Utils.RandomValueInRange(0, Enum.GetValues(typeof(BalloonArcheryGame.Colors)).Length);
        string colorToScore = ((BalloonArcheryGame.Colors)randomColorIndex).ToString().ToLower();

        switch (colorToScore)
        {
            case "red": 
                _balloonColorToScoreText.text = "Vermelho"; 
                _balloonColorToScoreText.color = Color.red;
                _balloonPrefabToScore = _redBalloonPrefab; 
                break;

            case "blue": 
                _balloonColorToScoreText.text = "Azul"; 
                _balloonColorToScoreText.color = Color.blue;
                _balloonPrefabToScore = _blueBalloonPrefab; 
                break;

            case "yellow": 
                _balloonColorToScoreText.text = "Amarelo"; 
                _balloonColorToScoreText.color = Color.yellow;
                _balloonPrefabToScore = _yellowBalloonPrefab; 
                break;

             default:
                Debug.LogError("Invalid balloon color selected for scoring.");
                return;
        }

        _balloonColorToScoreText.transform.DOKill();

        
        _balloonColorToScoreText.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 5, 0.5f);


        PlayerPrefs.SetString("BalloonColorToScore", colorToScore); 
    }

    /// <summary>Returns the least-spawned color, preferring the scoring color on ties, and increments its counter.</summary>
    private GameObject GetBalloonType()
    {   
        int minCount = _balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = _balloonTypesCount
            .Where(element => element.Value == minCount)
            .Select(element => element.Key)
            .ToArray();

        GameObject selectedBalloon;

        if (candidates.Contains(_balloonPrefabToScore))
        {
            selectedBalloon = _balloonPrefabToScore;
        }
        else
        {
            selectedBalloon = candidates[Utils.RandomValueInRange(0, candidates.Length)];
        }

        _balloonTypesCount[selectedBalloon]++;
        return selectedBalloon;
    }

    /// <summary>Drops back to level 0 (the <c>reset</c> cheat); no-op if already there.</summary>
    public override void ResetDifficulty()
    {   
        if (DifficultyLevel > 0)
        {
            DifficultyLevel = 0;
            ApplyDifficultySettings();
        }
    }
}