using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrisbeeGameManager : MiniGameManager
{ 
    [Header("1. Configuração Física (Segurança)")]
    [Tooltip("Distância onde o cão começa (m). Calibra-se no Start.")]
    public float baseDogDistance = 5f;       
    
    [Tooltip("Quanto o cão recua por nível (m).")]
    public float distanceIncrement = 2.5f;   
    
    [Tooltip("Limite MÁXIMO físico. O cão nunca passa daqui para não lesionar o paciente.")]
    public float maxDogDistance = 20f;       

    [Header("2. Configuração Cognitiva (Infinita)")]
    public int baseTargetCount = 3;
    
    [Tooltip("Quantos alvos novos ganha por nível (Ex: 0.2 = 1 alvo a cada 5 níveis).")]
    public float targetsPerLevel = 1.5f;    

    [Header("3. Curvas de Complexidade (0 a 1)")]
    [Tooltip("Define a % de alvos que se movem baseada na 'Saturação' do nível.")]
    public AnimationCurve movingRatioCurve;     
    [Tooltip("Define a % de alvos que piscam/somem.")]
    public AnimationCurve visibilityRatioCurve; 

    [Header("Referências")]
    [SerializeField] private GameObject scoreAreaPrefab;
    [SerializeField] 
    private Collider dogAreaCollider;

    [SerializeField]
    private Collider dogScoreAreaCollider;

    // Estado Interno
    private Transform _playerTransform;
    private readonly List<GameObject> _scoreAreas = new();
    
    // Variáveis para spawn
    private float _currentMovingRatio;
    private float _currentVisibilityRatio;

    protected override void Awake()
    {
        base.Awake();

        if (movingRatioCurve.length == 0 || visibilityRatioCurve.length == 0)
        {
            Debug.LogError("One or more animation curves are not assigned in the inspector.");
        }
    }

    public override void TutorialCompleted()
    {   
        base.TutorialCompleted();
        
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_playerTransform == null)
        {
            Debug.LogError("Player not found. The rehabilitation system cannot start.");
            return;
        }

    
        float initialDist = GetPlayerDistanceToDog();
        if (initialDist > 0)
        {
            baseDogDistance = initialDist;
        } 

        ApplyDifficultySettings();
    }



    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {
        difficultyLevel = isToIncreaseDiff ? difficultyLevel + 1 : Mathf.Max(0, difficultyLevel - 1);
        Debug.Log($"<color=blue>DDA:</color> Adjusting difficulty to Level {difficultyLevel} ({(isToIncreaseDiff ? "Increased" : "Decreased")})");
        ApplyDifficultySettings();
    }

    // --- MATEMÁTICA DE ADAPTAÇÃO ---

    protected override void ApplyDifficultySettings()
    {
        float rawDistance = baseDogDistance + (difficultyLevel * distanceIncrement);
        float finalDogDistance = Mathf.Min(rawDistance, maxDogDistance);
        
        PlayerPrefs.SetFloat("DogDistance", finalDogDistance);

        int finalTargetCount = Mathf.RoundToInt(baseTargetCount + (difficultyLevel * targetsPerLevel));

        float saturationFactor = difficultyLevel / (difficultyLevel + 4f);

        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentVisibilityRatio = visibilityRatioCurve.Evaluate(saturationFactor);

        Debug.Log($"[DDA] Level {difficultyLevel} | Dist: {finalDogDistance:F1}m | Targets: {finalTargetCount} | Complexity: {saturationFactor:P0}");

        SyncScoreAreas(finalTargetCount);
    }

    private void SyncScoreAreas(int desiredCount)
    {
        while (_scoreAreas.Count < desiredCount)
        {
            AddScoreArea();
        }
        
        while (_scoreAreas.Count > desiredCount)
        {
            RemoveScoreArea();
        }

        UpdateTargetsProperties();
    }

    private void AddScoreArea()
    {
        Vector3 pos = GetScoreAreaPosition();
        GameObject newScoreArea = Instantiate(scoreAreaPrefab, pos, Quaternion.identity);
        _scoreAreas.Add(newScoreArea);
    }

    private void RemoveScoreArea()
    {
        if (_scoreAreas.Count > 0)
        {
            GameObject target = _scoreAreas[0];
            _scoreAreas.RemoveAt(0);
            Destroy(target);
        }
    }

    private void UpdateTargetsProperties()
    {
        int total = _scoreAreas.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetVisibleCount = Mathf.RoundToInt(total * _currentVisibilityRatio);

        List<GameObject> shuffled = _scoreAreas.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {   
            if (shuffled[i].TryGetComponent<ScoreAreaProperties>(out var scoreArea))
            {
                scoreArea.AdjustMovement( i < targetMovingCount);

                scoreArea.AdjustVisibility(i < targetVisibleCount); 
            }
           
       
        }
    }

    private Vector3 GetScoreAreaPosition()
    {
        float currentDogDist = PlayerPrefs.GetFloat("DogDistance", baseDogDistance);
        
        const float MIN_OFFSET = 0.2f;
        const float MAX_OFFSET = 1f;
        const float MIN_DISTANCE_BETWEEN_AREAS = 2f; 

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        float scoreAreaDistance = currentDogDist * Random.Range(MIN_OFFSET, MAX_OFFSET);

        Vector3 scoreAreaPosition = _playerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * scoreAreaDistance;
        
        scoreAreaPosition.y = dogAreaCollider.bounds.center.y;

        bool isTooClose = _scoreAreas.Any(scoreArea => Vector3.Distance(scoreArea.transform.position, scoreAreaPosition) < MIN_DISTANCE_BETWEEN_AREAS && dogScoreAreaCollider.bounds.Contains(scoreAreaPosition));

        if (!dogAreaCollider.bounds.Contains(scoreAreaPosition) || isTooClose)
        {
            return GetScoreAreaPosition(); 
        }

        return scoreAreaPosition;
    }

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
}