using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrisbeeGameManager : GameManager
{
    [Header("--- Estado da Sessão (Read Only) ---")]
    public int difficultyLevel = 0; // Nível infinito
    
    [Header("1. Configuração Física (Segurança)")]
    [Tooltip("Distância onde o cão começa (m). Calibra-se no Start.")]
    public float baseDogDistance = 5f;       
    
    [Tooltip("Quanto o cão recua por nível (m).")]
    public float distanceIncrement = 0.5f;   
    
    [Tooltip("Limite MÁXIMO físico. O cão nunca passa daqui para não lesionar o paciente.")]
    public float maxDogDistance = 20f;       

    [Header("2. Configuração Cognitiva (Infinita)")]
    public int baseTargetCount = 3;
    
    [Tooltip("Quantos alvos novos ganha por nível (Ex: 0.2 = 1 alvo a cada 5 níveis).")]
    public float targetsPerLevel = 0.2f;    

    [Header("3. Curvas de Complexidade (0 a 1)")]
    [Tooltip("Define a % de alvos que se movem baseada na 'Saturação' do nível.")]
    public AnimationCurve movingRatioCurve;     
    [Tooltip("Define a % de alvos que piscam/somem.")]
    public AnimationCurve visibilityRatioCurve; 

    [Header("Referências")]
    [SerializeField] private GameObject scoreAreaPrefab;
    [SerializeField] private Collider dogAreaCollider;

    // Estado Interno
    private Transform _playerTransform;
    private readonly List<GameObject> _scoreAreas = new();
    
    // Variáveis para spawn
    private float _currentMovingRatio;
    private float _currentVisibilityRatio;

    protected override void Awake()
    {
        base.Awake();
        // Curvas padrão de segurança (Logísticas)
        if (movingRatioCurve.length == 0) movingRatioCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.8f);
        if (visibilityRatioCurve.length == 0) visibilityRatioCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.5f);
    }

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_playerTransform == null)
        {
            Debug.LogError("CRÍTICO: Player não encontrado. O sistema de reabilitação não pode iniciar.");
            return;
        }

        // CALIBRAÇÃO INICIAL:
        // Assume que a distância inicial definida na cena é a "base" confortável para este paciente.
        float initialDist = GetPlayerDistanceToDog();
        if (initialDist > 0) baseDogDistance = initialDist;

        // Aplica o nível 0
        ApplyDifficultySettings();
    }

    // --- INTERFACE COM CARNYWISE ---

    public override void IncreaseDifficulty()
    {
        difficultyLevel++;
        Debug.Log($"<color=green>PROGRESSÃO:</color> Subindo para Nível {difficultyLevel}");
        ApplyDifficultySettings();
    }

    public override void DecreaseDifficulty()
    {
        if (difficultyLevel > 0)
        {
            difficultyLevel--;
            Debug.Log($"<color=yellow>AJUSTE:</color> Descendo para Nível {difficultyLevel}");
            ApplyDifficultySettings();
        }
    }

    // --- MATEMÁTICA DE ADAPTAÇÃO ---

    private void ApplyDifficultySettings()
    {
        // 1. Distância (Com Teto de Segurança)
        float rawDistance = baseDogDistance + (difficultyLevel * distanceIncrement);
        float finalDogDistance = Mathf.Min(rawDistance, maxDogDistance);
        
        // Salva para a IA do Cão ler
        PlayerPrefs.SetFloat("DogDistance", finalDogDistance);

        // 2. Quantidade de Alvos (Sem Teto)
        int finalTargetCount = Mathf.RoundToInt(baseTargetCount + (difficultyLevel * targetsPerLevel));

        // 3. Fator de Complexidade (Saturação)
        // Converte o nível infinito (0 a 1000) num valor de 0.0 a 1.0 que sobe rápido e desacelera.
        // Fórmula: x / (x + k). Ajuste k (20f) para mudar a inclinação da curva.
        float saturationFactor = difficultyLevel / (difficultyLevel + 20f);

        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentVisibilityRatio = visibilityRatioCurve.Evaluate(saturationFactor);

        Debug.Log($"[DDA] Nível {difficultyLevel} | Dist: {finalDogDistance:F1}m | Alvos: {finalTargetCount} | Complexidade: {saturationFactor:P0}");

        SyncScoreAreas(finalTargetCount);
    }

    // --- GESTÃO DOS OBJETOS ---

    private void SyncScoreAreas(int desiredCount)
    {
        // Adiciona se faltar
        while (_scoreAreas.Count < desiredCount)
        {
            AddScoreArea();
        }
        
        // Remove se sobrar (Remove o mais antigo/primeiro da lista)
        while (_scoreAreas.Count > desiredCount)
        {
            RemoveScoreArea();
        }

        // Atualiza propriedades de TODOS (mesmo os antigos) para a nova dificuldade
        UpdateTargetsProperties();
        
        // Opcional: Reposicionar todos para o novo padrão de ângulo?
        // Para reabilitação, talvez seja melhor reposicionar apenas quando o paciente acerta, 
        // mas aqui vamos manter a posição até serem reciclados para não "teletransportar" na cara do paciente.
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

        // Embaralha para aleatoriedade
        var shuffled = _scoreAreas.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {
            if (!shuffled[i].TryGetComponent<ScoreAreaExtraSettings>(out var scoreArea)) continue;
           
            scoreArea.AdjustMovement( i < targetMovingCount);

            scoreArea.AdjustVisibility(i < targetVisibleCount); 
        }
    }

    // --- LÓGICA DE POSICIONAMENTO CLÍNICO (O CONE) ---

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

        bool isTooClose = _scoreAreas.Any(scoreArea => Vector3.Distance(scoreArea.transform.position, scoreAreaPosition) < MIN_DISTANCE_BETWEEN_AREAS);

        if (!dogAreaCollider.bounds.Contains(scoreAreaPosition) || isTooClose)
        {
            return GetScoreAreaPosition(); 
        }

        return scoreAreaPosition;
    }

    private float GetPlayerDistanceToDog()
    {
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");
        return dog != null ? Vector3.Distance(_playerTransform.position, dog.transform.position) : 0f;
    }
}