using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArcheryGameManager : GameManager
{   
    [Header("Game Specific References")]
    [SerializeField] private TextMeshProUGUI balloonColorToScoreText;
    [SerializeField] private Collider balloonSpawnArea;

    [Header("Balloons Prefabs")]
    [SerializeField] private GameObject blueBalloonPrefab;
    [SerializeField] private GameObject redBalloonPrefab;
    [SerializeField] private GameObject yellowBalloonPrefab;

    [Header("1. Configuração de Spawn (Infinito)")]
    [SerializeField] private int baseBalloonCount = 5;
    
    [Tooltip("Quantos balões novos por nível (Ex: 0.5 = 1 balão a cada 2 níveis)")]
    [SerializeField] private float balloonsPerLevel = 0.5f;

    [Header("2. Curvas de Complexidade (0 a 1)")]
    [Tooltip("Eixo X: Saturação de Dificuldade. Eixo Y: % de balões que se movem.")]
    [SerializeField] private AnimationCurve movingRatioCurve;
    
    [Tooltip("Eixo X: Saturação de Dificuldade. Eixo Y: % de balões transparentes.")]
    [SerializeField] private AnimationCurve transparencyRatioCurve;

    // Estado Interno
    private readonly List<GameObject> _spawnedBalloons = new();
    private float _currentMovingRatio;
    private float _currentTransparencyRatio;
    private Dictionary<GameObject, int> balloonTypesCount;
    private GameObject _balloonPrefabToScore;

    protected override void Awake()
    {   
        base.Awake(); 

        // Validações de segurança
        if (balloonColorToScoreText == null || balloonSpawnArea == null)
        {
            Debug.LogError("Referências de UI ou Spawn Area em falta no ArcheryGameManager.");
            return;
        }

        if (redBalloonPrefab == null || blueBalloonPrefab == null || yellowBalloonPrefab == null)
        {
            Debug.LogError("Referências de Prefabs de balões em falta.");
            return;
        }

        // Setup das curvas padrão (Fallback)
        if (movingRatioCurve.length == 0) movingRatioCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.8f);
        if (transparencyRatioCurve.length == 0) transparencyRatioCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.5f);

        // Inicializa Dicionário
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
        ApplyDifficultySettings();
    }

    public override void ChangeDifficulty(bool isToIncreaseDiff)
    {
        difficultyLevel = isToIncreaseDiff ? difficultyLevel + 1 : Mathf.Max(0, difficultyLevel - 1);
        Debug.Log($"<color=orange>ARCHERY DDA:</color> Nível {difficultyLevel} ({(isToIncreaseDiff ? "Subiu" : "Desceu")})");
        ApplyDifficultySettings();
    }

    // --- LÓGICA DE ADAPTAÇÃO ---

    protected override void ApplyDifficultySettings()
    {
        // 1. Quantidade de Balões
        int finalBalloonCount = Mathf.RoundToInt(baseBalloonCount + (difficultyLevel * balloonsPerLevel));

        // 2. Complexidade
        float saturationFactor = difficultyLevel / (difficultyLevel + 4f); 
        _currentMovingRatio = movingRatioCurve.Evaluate(saturationFactor);
        _currentTransparencyRatio = transparencyRatioCurve.Evaluate(saturationFactor);

        Debug.Log($"[Archery Stats] Nível: {difficultyLevel} | Total: {finalBalloonCount} | Movimento: {_currentMovingRatio:P0} | Transparência: {_currentTransparencyRatio:P0}");
        
        // 3. Define Regra e Sincroniza
        SetBalloonColorToScore();
        SyncBalloons(finalBalloonCount);
    }

    private void SyncBalloons(int desiredCount)
    {
        // PASSO CRÍTICO: Limpa referências nulas antes de decidir spawnar novos
        _spawnedBalloons.RemoveAll(item => item == null);

        // Adiciona se faltar
        while (_spawnedBalloons.Count < desiredCount)
        {
            SpawnBalloon();
        }

        // Remove se sobrar (Remove o mais antigo)
        while (_spawnedBalloons.Count > desiredCount)
        {
            RemoveBalloon();
        }

        // Atualiza propriedades (Não precisa de limpar nulos novamente, acabámos de o fazer)
        UpdateBalloonsProperties();
    }

    private void SpawnBalloon()
    {
        Vector3 pos = GetRandomBalloonPosition();
        GameObject prefabToSpawn = GetBalloonType();

        GameObject newBalloon = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        if (newBalloon.TryGetComponent(out BalloonArcheryGame balloonComponent))
        {   
            balloonComponent.OriginalPrefab = prefabToSpawn;
        }
        else
        {
            Debug.LogError("Spawned balloon does not have BalloonArcheryGame component.");
        }

        _spawnedBalloons.Add(newBalloon);
    }

    private void RemoveBalloon()
    {
        if (_spawnedBalloons.Count == 0) return;

        GameObject target = _spawnedBalloons[0];

        // Se tiver o componente, usa o método seguro que atualiza o dicionário e remove da lista
        if (target != null && target.TryGetComponent(out BalloonArcheryGame balloonComponent))
        {
            BalloonDestroyed(target, balloonComponent.OriginalPrefab);
        }
        else
        {
            // Fallback caso o objeto esteja corrompido ou sem script
            _spawnedBalloons.RemoveAt(0);
        }

        if (target != null) Destroy(target);    
    }

    // Método robusto chamado pelo Sistema (RemoveBalloon) ou pelo Balão (Pop)
    public void BalloonDestroyed(GameObject balloonDestroyed, GameObject balloonPrefab)
    {   
        if (balloonPrefab == null || !balloonTypesCount.ContainsKey(balloonPrefab))
        {
            // Warning em vez de Error para evitar spam em casos de teste
            Debug.LogWarning("BalloonDestroyed: Prefab inválido ou não registado.");
            return;
        }

        // Atualiza Matemática
        balloonTypesCount[balloonPrefab] = Mathf.Max(0, balloonTypesCount[balloonPrefab] - 1);

        // Atualiza Lista Física (Só remove se ainda lá estiver)
        if (_spawnedBalloons.Contains(balloonDestroyed))
        {
            _spawnedBalloons.Remove(balloonDestroyed);
        }
    }

    private void UpdateBalloonsProperties()
    {
        int total = _spawnedBalloons.Count;
        int targetMovingCount = Mathf.RoundToInt(total * _currentMovingRatio);
        int targetTransparentCount = Mathf.RoundToInt(total * _currentTransparencyRatio);

        // Embaralha uma cópia da lista para distribuir propriedades aleatoriamente
        var shuffled = _spawnedBalloons.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < total; i++)
        {
            if (shuffled[i].TryGetComponent<BalloonArcheryGame>(out var balloonScript))
            {
                // Os primeiros N da lista embaralhada ganham a habilidade
                balloonScript.AdjustMovement(i < targetMovingCount);
                balloonScript.AdjustTransparency(i < targetTransparentCount);
            }
        }
    }

    private Vector3 GetRandomBalloonPosition()
    {
        Bounds bounds = balloonSpawnArea.bounds;
        
        // Tenta 10 vezes encontrar uma posição livre
        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 candidatePos = new Vector3(x, y, z);

            // 0.3f é o raio estimado do balão
            if (!Physics.CheckSphere(candidatePos, 0.3f)) 
            {
                return candidatePos;
            }
        }

        // CORREÇÃO: Fallback seguro em vez de recursão infinita (Crash fix)
        Debug.LogWarning("Não foi possível encontrar posição livre. Usando centro.");
        return bounds.center;
    }

    private void SetBalloonColorToScore()
    {   
        int randomColorIndex = Utils.RandomValueInRange(0, Enum.GetValues(typeof(BalloonArcheryGame.Colors)).Length);
        string colorToScore = ((BalloonArcheryGame.Colors)randomColorIndex).ToString().ToLower();

        switch (colorToScore)
        {
            case "red": 
                balloonColorToScoreText.text = "Cor para marcar: Vermelho"; 
                _balloonPrefabToScore = redBalloonPrefab;
                break;
            case "blue": 
                balloonColorToScoreText.text = "Cor para marcar: Azul"; 
                _balloonPrefabToScore = blueBalloonPrefab;
                break;
            case "yellow": 
                balloonColorToScoreText.text = "Cor para marcar: Amarelo"; 
                _balloonPrefabToScore = yellowBalloonPrefab;
                break;
            default: 
                Debug.LogError("Cor inválida gerada: " + colorToScore);    
                return;
        }

        PlayerPrefs.SetString("BalloonColorToScore", colorToScore); 
    }

    protected GameObject GetBalloonType()
    {   
        int minCount = balloonTypesCount.Min(typeCount => typeCount.Value);

        // Encontra candidatos com a menor contagem
        var candidates = balloonTypesCount
            .Where(kvp => kvp.Value == minCount)
            .Select(kvp => kvp.Key)
            .ToArray();

        GameObject selectedBalloon;

        // Prioridade ao balão que dá pontos se ele estiver entre os menos abundantes
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