using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

/// <summary>
/// Manages the Archery mini-game: spawns colored balloons inside the spawn area, applies DDA difficulty
/// curves for movement and transparency ratios, and randomly selects the scoring balloon color each round.
/// Inherits target lifecycle from <see cref="MiniGameManager"/>.
/// </summary>
public class ArcheryGameManager : MiniGameManager
{
    /// <summary>PlayerPrefs key of the saved Archery difficulty level; also read by <see cref="CarnyWise"/>.</summary>
    public const string DifficultyLevelKey = "ArcheryDifficultyLevel";
    /// <summary>PlayerPrefs key of the balloon color that scores; read by <see cref="BalloonArcheryGame"/> and <see cref="ArcheryCheatCodes"/>.</summary>
    public const string BalloonColorToScoreKey = "BalloonColorToScore";

    /// <summary>TMP text displaying the balloon color that scores; its text and color are set to match the target balloon.</summary>
    [Header("Game Specific References")]
    [SerializeField] private TextMeshProUGUI _balloonColorToScoreText;
    /// <summary>Collider whose world bounds define the volume where balloons are spawned (separate from the <c>BalloonSpawn</c> box balloons wander in).</summary>
    [SerializeField] private Collider _balloonSpawnArea;
    /// <summary>
    /// Layer(s) checked for overlapping balloons when picking a spawn position; set it to the dedicated <c>Balloon</c> layer
    /// (also assigned to the balloon prefabs) so the query only returns balloons and the small result buffer cannot be
    /// filled by unrelated colliders. Hits are still filtered by the <c>Balloon</c> tag as a safety net. Defaults to every
    /// layer, which works but is slower and can miss a balloon when more than <see cref="_overlapBuffer"/>-length colliders overlap.
    /// </summary>
    [SerializeField] private LayerMask _balloonOverlapLayers = ~0;

    /// <summary>
    /// Prefab of the blue balloon. Its root is what is taken from and returned to the pool; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>BLUE</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [Header("Balloons Prefabs")]
    [SerializeField]
     private GameObject _blueBalloonPrefab;
    /// <summary>
    /// Prefab of the red balloon. Its root is what is taken from and returned to the pool; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>RED</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [SerializeField]
    private GameObject _redBalloonPrefab;
    /// <summary>
    /// Prefab of the yellow balloon. Its root is what is taken from and returned to the pool; a child must hold a <see cref="BalloonArcheryGame"/>
    /// whose color is <c>YELLOW</c>, otherwise the scoring color shown on screen and the balloon that actually scores disagree.
    /// </summary>
    [SerializeField]
    private GameObject _yellowBalloonPrefab;

    /// <summary>
    /// Fraction of balloons (Y, 0–1) that move, as a function of the difficulty saturation factor (X, 0–1; see
    /// <see cref="ApplyDifficultySettings"/>). Must have at least one key.
    /// </summary>
    [Header("Complexity Curves")]
    [SerializeField]
    private AnimationCurve _movingRatioCurve;
    /// <summary>
    /// Fraction of balloons (Y, 0–1) with the fade cycle on, as a function of the difficulty saturation factor (X, 0–1).
    /// Must have at least one key. Moving and transparent balloons are chosen independently, so one balloon can be both.
    /// </summary>
    [SerializeField]
    private AnimationCurve _transparencyRatioCurve;

    /// <summary>
    /// Seconds a balloon takes between two wander points at level 0 (larger is slower). The duration is interpolated
    /// towards <see cref="_fastMoveDuration"/> by the saturation factor, so it is never fully reached.
    /// </summary>
    [Header("Speed Progression")]
    [Tooltip("Movement duration in lowest difficulty (Bigger is slower). Ex: 4s")]
    [SerializeField]
    private float _slowMoveDuration = 6.0f;

    /// <summary>Seconds between two wander points that the duration approaches at very high levels (smaller is faster).</summary>
    [Tooltip("Movement duration in highest difficulty (Smaller is faster). Ex: 1s" )]
    [SerializeField]
    private float _fastMoveDuration = 1.5f;

    /// <summary>Punch strength of the scoring-color text animation, added to its scale on every axis (1.2 → grows to 2.2× at the peak).</summary>
    [Header("Color to Score Animation Settings")]
    [SerializeField]
    private float _punchScale = 1.2f;
    /// <summary>Duration in seconds of the DOPunchScale animation on the balloon-color text.</summary>
    [SerializeField]
    private float _punchDuration = 0.5f;

    /// <summary>Live balloons per prefab, used by <see cref="GetBalloonType"/> to keep the colors balanced.</summary>
    private Dictionary<GameObject, int> _balloonTypesCount;
    /// <summary>
    /// Pool the balloons are taken from and returned to. Colors are mixed in its queue (what matters is the total
    /// number of balloons): a reused balloon keeps its color, which <see cref="AddTarget"/> accounts for.
    /// </summary>
    private ObjectPool _balloonsPool;
    /// <summary>Prefab of the color that currently scores; always kept present in the scene by <see cref="RemoveTarget(GameObject, GameObject)"/>.</summary>
    private GameObject _balloonPrefabToScore;
    
    /// <summary>Fraction of balloons that move at the current level, from <see cref="_movingRatioCurve"/>.</summary>
    private float _currentMovingRatio;
    /// <summary>Fraction of balloons with the fade cycle on at the current level, from <see cref="_transparencyRatioCurve"/>.</summary>
    private float _currentTransparencyRatio;
    /// <summary>Wander duration given to every balloon at the current level (between <see cref="_slowMoveDuration"/> and <see cref="_fastMoveDuration"/>).</summary>
    private float _currentMoveDuration;

    /// <summary>Reused by <see cref="GetRandomTargetPosition"/> so the overlap checks do not allocate an array per attempt.</summary>
    private readonly Collider[] _overlapBuffer = new Collider[16];


    /// <summary>
    /// Validates references, initializes the color counters, clears the saved difficulty if the tutorial was
    /// never completed (first session starts at level 0), and picks the first scoring color.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake(); 

        if (!Utils.RequireAssigned(_balloonColorToScoreText, nameof(_balloonColorToScoreText), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_balloonSpawnArea, nameof(_balloonSpawnArea), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_blueBalloonPrefab, nameof(_blueBalloonPrefab), this) ||
            !Utils.RequireAssigned(_yellowBalloonPrefab, nameof(_yellowBalloonPrefab), this) ||
            !Utils.RequireAssigned(_redBalloonPrefab, nameof(_redBalloonPrefab), this))
        {
            return;
        }

        if (!Utils.RequireNotEmpty(_movingRatioCurve, nameof(_movingRatioCurve), this) ||
            !Utils.RequireNotEmpty(_transparencyRatioCurve, nameof(_transparencyRatioCurve), this))
        {
            return;
        }

        _balloonTypesCount = new Dictionary<GameObject, int>
        {
            { _blueBalloonPrefab, 0 },
            { _redBalloonPrefab, 0 },
            { _yellowBalloonPrefab, 0 }
        };

        _balloonsPool = new ObjectPool(_blueBalloonPrefab);

          if (!GameManager.GetInstance().ArcheryTutorialCompleted)
        {
            PlayerPrefs.DeleteKey(DifficultyLevelKey);
        }

        SetBalloonColorToScore();
    }

    /// <summary>Restores the saved archery difficulty and spawns the balloons for it.</summary>
    public override void TutorialCompleted()
    {   
        DifficultyLevel = PlayerPrefs.GetInt(DifficultyLevelKey, 0);
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

        PlayerPrefs.SetInt(DifficultyLevelKey, DifficultyLevel);
        
        // Debug.Log($"[Archery Stats] Total Balloons: {_currentDesiredCount} | Movimento Ratio: {_currentMovingRatio:P0} | Move Duration: {_currentMoveDuration:F1}s");
        
        SetBalloonColorToScore();
        UpdateTargetsPoolSize();
        SyncTargets(_currentDesiredCount);
    }

    /// <summary>
    /// Grows the pool to <see cref="MiniGameManager._currentDesiredCount"/> balloons, creating the missing ones split
    /// evenly between the colors, so the new level's balloons are created now and not mid-aim.
    /// </summary>
    private void UpdateTargetsPoolSize()
    {
        int missingPerColor = Mathf.CeilToInt((_currentDesiredCount - _balloonsPool.PoolSize) / (float)_balloonTypesCount.Count);

        if (missingPerColor <= 0)
        {
            return;
        }

        foreach (GameObject balloonPrefab in _balloonTypesCount.Keys)
        {
            _balloonsPool.SetPrefab(balloonPrefab);
            _balloonsPool.UpdatePoolSize(_balloonsPool.PoolSize + missingPerColor);
        }
    }

    /// <summary>
    /// Takes a balloon from the pool at a free position; when no prefab is given, the color is chosen by
    /// <see cref="GetBalloonType"/>. Since the pool's queue mixes colors, a reused balloon can come out with another
    /// color than the one asked for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A balanced pick (no prefab given) accepts whatever color comes out and moves its <see cref="_balloonTypesCount"/>
    /// increment to that color. An explicit <paramref name="prefabToSpawn"/> (the scoring color forced by
    /// <see cref="RemoveTarget(GameObject, GameObject)"/>) must be honored, or the player could be left with no balloon that scores: a reused
    /// balloon of another color goes back to the pool and a new one of the right color is created instead.
    /// </para>
    /// <para>An explicit <paramref name="prefabToSpawn"/> does not increment <see cref="_balloonTypesCount"/>.</para>
    /// </remarks>
    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();

        bool isForcedColor = prefabToSpawn != null;

        if (!isForcedColor)
        {
            prefabToSpawn = GetBalloonType();
        }

        _balloonsPool.SetPrefab(prefabToSpawn);

        GameObject newBalloon = _balloonsPool.GetObject(pos, Quaternion.identity);
        BalloonArcheryGame balloonComponent = newBalloon.GetComponentInChildren<BalloonArcheryGame>();

        // A reused balloon keeps the color it was created with, so its prefab comes from its real color, not from the request.
        balloonComponent.OriginalPrefab = GetBalloonPrefab(balloonComponent.GetBalloonColorName());

        if (balloonComponent.OriginalPrefab != prefabToSpawn)
        {
            if (isForcedColor)
            {
                _balloonsPool.ReturnObject(newBalloon);

                newBalloon = _balloonsPool.CreateObject(pos, Quaternion.identity);
                balloonComponent = newBalloon.GetComponentInChildren<BalloonArcheryGame>();
                balloonComponent.OriginalPrefab = prefabToSpawn;
            }
            else
            {
                _balloonTypesCount[prefabToSpawn]--;
                _balloonTypesCount[balloonComponent.OriginalPrefab]++;
            }
        }

        _spawnedTargets.Add(newBalloon);
    }

    /// <summary>Balloon prefab of the given lowercase color name (see <see cref="BalloonArcheryGame.GetBalloonColorName"/>).</summary>
    private GameObject GetBalloonPrefab(string colorName)
    {
        return colorName switch
        {
            "red" => _redBalloonPrefab,
            "blue" => _blueBalloonPrefab,
            _ => _yellowBalloonPrefab
        };
    }

    /// <summary>Returns the oldest balloon to the pool.</summary>
    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
 
            BalloonArcheryGame balloonComponent = target.GetComponentInChildren<BalloonArcheryGame>();

            RemoveTarget(target, balloonComponent.OriginalPrefab);
        }  
    }

    /// <summary>
    /// Returns a balloon (popped or removed) to the pool and, if the scene is below the desired count, spawns a replacement,
    /// forcing the scoring color when none of it is left so the player always has a valid target.
    /// </summary>
    /// <param name="target">Root GameObject of the balloon instance.</param>
    /// <param name="targetPrefab">Prefab it was spawned from, used to update the color counters.</param>
    /// <remarks>
    /// Only the replacement gets its properties (<see cref="UpdateNewTargetProperties"/>); the other balloons are left
    /// as they are, so a pop never changes the balloon the player is aiming at mid-attempt.
    /// </remarks>
    public override void RemoveTarget(GameObject target, GameObject targetPrefab)
    {
        _balloonTypesCount[targetPrefab] = Mathf.Max(0, _balloonTypesCount[targetPrefab] - 1);

        _spawnedTargets.Remove(target);

        _balloonsPool.ReturnObject(target);

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

            UpdateNewTargetProperties(_spawnedTargets[^1]);
        }
    }

    /// <summary>
    /// Gives a freshly spawned balloon the current move duration, and makes it moving/transparent only if the other
    /// balloons are below the counts the current ratios ask for.
    /// </summary>
    /// <param name="newTarget">Root GameObject of the balloon just added by <see cref="AddTarget"/>.</param>
    private void UpdateNewTargetProperties(GameObject newTarget)
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetTransparentCount = Mathf.RoundToInt(total * _currentTransparencyRatio);

        int movingCount = 0;
        int transparentCount = 0;

        foreach (GameObject target in _spawnedTargets)
        {
            if (target == newTarget)
            {
                continue;
            }

            BalloonArcheryGame balloon = target.GetComponentInChildren<BalloonArcheryGame>();

            if (balloon.IsMoving)
            {
                movingCount++;
            }

            if (balloon.IsTransparent)
            {
                transparentCount++;
            }
        }

        BalloonArcheryGame newBalloon = newTarget.GetComponentInChildren<BalloonArcheryGame>();

        newBalloon.SetMoveDuration(_currentMoveDuration);
        newBalloon.AdjustMovement(movingCount < targetMovingCount);
        newBalloon.AdjustTransparency(transparentCount < targetTransparentCount);
    }

    /// <summary>Shuffles the balloons and makes the first N moving and the first M transparent, according to the current ratios.</summary>
    /// <remarks>Only runs on a difficulty change (<see cref="SyncTargets"/>); a pop updates just the replacement.</remarks>
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
    /// difficulty carried over from a previous session). Only colliders on <see cref="_balloonOverlapLayers"/> tagged
    /// <c>Balloon</c> count as overlaps, and only the first <see cref="_overlapBuffer"/>-length hits are inspected.
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

            int hitCount = Physics.OverlapSphereNonAlloc(candidatePos, SafetyRadius, _overlapBuffer, _balloonOverlapLayers);
            bool hitBalloon = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_overlapBuffer[i].CompareTag(BalloonArcheryGame.BalloonTag))
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


        PlayerPrefs.SetString(BalloonColorToScoreKey, colorToScore); 
    }

    /// <summary>Returns the least-spawned color, preferring the scoring color on ties, and increments its counter.</summary>
    private GameObject GetBalloonType()
    {   
        return Utils.PickBalancedPrefab(_balloonTypesCount, _balloonPrefabToScore);
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