using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArcheryGameManager : MiniGameManager
{   
    [Header("Game Specific References")]
    [SerializeField] private TextMeshProUGUI balloonColorToScoreText;
    [SerializeField] private Collider balloonSpawnArea;

    [Header("Balloons Prefabs")]
    [SerializeField]
     private GameObject blueBalloonPrefab;
    [SerializeField] 
    private GameObject redBalloonPrefab;
    [SerializeField] 
    private GameObject yellowBalloonPrefab;

    [Header("Complexity Curves")]
    [SerializeField] 
    private AnimationCurve movingRatioCurve;
    [SerializeField] 
    private AnimationCurve transparencyRatioCurve;

    private Dictionary<GameObject, int> balloonTypesCount;
    private GameObject _balloonPrefabToScore;
    
    // Variáveis de Controlo
    private float _currentMovingRatio;
    private float _currentTransparencyRatio;
    private int _currentDesiredCount; // CRÍTICO: Para saber quantos devemos ter

    protected override void Awake()
    {   
        base.Awake(); 

        if (balloonColorToScoreText == null)
        {
            Debug.LogError("Balloon Color To Score Text reference is missing.");
            return;
        }

        if (balloonSpawnArea == null)
        {
            Debug.LogError("Balloon Spawn Area reference is missing.");
            return;
        }

        if (blueBalloonPrefab == null || yellowBalloonPrefab == null || redBalloonPrefab == null)
        {
            Debug.LogError("One or more balloon prefab references are missing.");
            return;
        }

        if (movingRatioCurve.length == 0 || transparencyRatioCurve.length == 0)
        {
            Debug.LogError("One or more difficulty curves are not set.");
            return;
        }

        balloonTypesCount = new Dictionary<GameObject, int>
        {
            { blueBalloonPrefab, 0 },
            { redBalloonPrefab, 0 },
            { yellowBalloonPrefab, 0 }
        };

        SetBalloonColorToScore();
    }

    public override void TutorialCompleted()
    {   
        base.TutorialCompleted();
        ApplyDifficultySettings();
    }

    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {
        difficultyLevel = isToIncreaseDiff ? difficultyLevel + 1 : Mathf.Max(0, difficultyLevel - 1);
        Debug.Log($"<color=orange>ARCHERY DDA:</color> Nível {difficultyLevel}");
        ApplyDifficultySettings();
    }

    protected override void ApplyDifficultySettings()
    {
        _currentDesiredCount = Mathf.RoundToInt(targetsCount + (difficultyLevel * targetsPerLevel));

        float saturationFactor = difficultyLevel / (difficultyLevel + 4f); 
        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentTransparencyRatio = transparencyRatioCurve.Evaluate(saturationFactor);

        Debug.Log($"[Archery Stats] Total Ideal: {_currentDesiredCount} | Movimento: {_currentMovingRatio:P0}");
        
        SetBalloonColorToScore();
        SyncTargets(_currentDesiredCount);
    }

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

    protected override void AddTarget(GameObject prefabToSpawn = null)
    {
        Vector3 pos = GetRandomTargetPosition();
        
        // Se não forçado, deixa o algoritmo escolher o melhor para equilíbrio
        if (prefabToSpawn == null)
        {
            prefabToSpawn = GetBalloonType();
        }

        GameObject newBalloon = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        // GetComponent simples (assumindo script na raiz ou filho)
        BalloonArcheryGame balloonComponent = newBalloon.GetComponentInChildren<BalloonArcheryGame>();
        
        balloonComponent.OriginalPrefab = prefabToSpawn;
            
        _spawnedTargets.Add(newBalloon);
    }

    protected override void RemoveTarget()
    {
        if (_spawnedTargets.Count > 0)
        {
            GameObject target = _spawnedTargets[0];
 
            BalloonArcheryGame balloonComponent = target.GetComponentInChildren<BalloonArcheryGame>();

            DestroyTarget(target, balloonComponent.OriginalPrefab);
        }  
    }

    public override void DestroyTarget(GameObject target, GameObject targetPrefab)
    {   
        balloonTypesCount[targetPrefab] = Mathf.Max(0, balloonTypesCount[targetPrefab] - 1);
        

        Debug.Log("Previous Balloon Counts: " + _spawnedTargets.Count);
        _spawnedTargets.Remove(target);

       Debug.Log("After Balloon Counts: " + _spawnedTargets.Count);

        Destroy(target);

        if (_spawnedTargets.Count < _currentDesiredCount)
        {
            // Se não houver NENHUM da cor alvo, forçamos a cor alvo.
            if (balloonTypesCount[_balloonPrefabToScore] == 0)
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

    protected override void UpdateTargetsProperties()
    {
        int total = _spawnedTargets.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetTransparentCount = Mathf.RoundToInt(total * _currentTransparencyRatio);

        List<GameObject> shuffled = _spawnedTargets.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {    
            BalloonArcheryGame balloonComponent = shuffled[i].GetComponentInChildren<BalloonArcheryGame>();

            balloonComponent.AdjustMovement(i < targetMovingCount);
            balloonComponent.AdjustTransparency(i < targetTransparentCount);
        }
    }

    protected override Vector3 GetRandomTargetPosition()
    {
        Bounds bounds = balloonSpawnArea.bounds;
        
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 candidatePos = new (x, y, z);

        const float SAFETEY_RADIUS = 0.3f;
            
        Collider[] hitColliders = Physics.OverlapSphere(candidatePos, SAFETEY_RADIUS);
        bool hitBalloon = false;

        foreach(Collider hit in hitColliders)
        {
            if(hit.CompareTag("Balloon")) 
            {
                hitBalloon = true; 
                break;
            }
        }

        return !hitBalloon ? candidatePos : GetRandomTargetPosition();
    }

    private void SetBalloonColorToScore()
    {   
        int randomColorIndex = Utils.RandomValueInRange(0, Enum.GetValues(typeof(BalloonArcheryGame.Colors)).Length);
        string colorToScore = ((BalloonArcheryGame.Colors)randomColorIndex).ToString().ToLower();

        switch (colorToScore)
        {
            case "red": 
                balloonColorToScoreText.text = "Cor para fazer pontos: Vermelho"; 
                _balloonPrefabToScore = redBalloonPrefab; 
                break;

            case "blue": 
                balloonColorToScoreText.text = "Cor para fazer pontos: Azul"; 
                _balloonPrefabToScore = blueBalloonPrefab; 
                break;

            case "yellow": 
                balloonColorToScoreText.text = "Cor para fazer pontos: Amarelo"; 
                _balloonPrefabToScore = yellowBalloonPrefab; 
                break;

             default:
                Debug.LogError("Invalid balloon color selected for scoring.");
                return;
        }
        PlayerPrefs.SetString("BalloonColorToScore", colorToScore); 
    }

    private GameObject GetBalloonType()
    {   
        int minCount = balloonTypesCount.Min(typeCount => typeCount.Value);

        GameObject[] candidates = balloonTypesCount
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

        balloonTypesCount[selectedBalloon]++;
        return selectedBalloon;
    }
}