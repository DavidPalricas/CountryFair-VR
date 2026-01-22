using UnityEngine;
using UnityEngine.Events;
using DG.Tweening; // Necessário para o DOTween

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class BalloonArcheryGame : MonoBehaviour
{
    [SerializeField]
    private Colors color = Colors.RED;

    [SerializeField]
    private GameObject balloonPrefab;

    [Header("Tutorial Settings")]
    [SerializeField]
    private bool isFromTutorial = false;

    [SerializeField]
    private UnityEvent taskCompleted;

    [Header("Movement Settings (DOTween)")]
    [SerializeField]
    private float moveDuration = 2f; // Tempo para ir de um ponto ao outro

    [Header("Visual Settings")]
    [SerializeField]
    private float fadeDuration = 1f;

    [SerializeField]
    private float stayTranslucentDuration = 1f;

    [SerializeField]
    private float minAlpha = 0.3f; // Quão translúcido fica

    [Header("Explosion Effect")]
    public GameObject popEffect;

    private Renderer _renderer;
    private Color _originalColor;
    
    [Header("Score Value")]
    [SerializeField]
    private int scoreValue;

    private BoxCollider _spawnArea;
    private Vector3 _extents;

    // Propriedades auto-implementadas são suficientes
    public bool CanMove { get; set; } = false;
    public bool CanBeTransluced { get; set; } = false;

    private string _colorName;

    private string _colorToScore;

    private Collider _collider;

    public enum Colors
    {
        RED,
        BLUE,
        YELLOW,
    }

    private ArcheryAudioManager _archeryAudioManager;
    private readonly AudioManager.GameSoundEffects popSoundEffect = AudioManager.GameSoundEffects.BALLOON_POP;
   
    private const int _INFINITE_LOOPS = -1;

    private void Awake()
    {
        if (popEffect == null)
        {
            Debug.LogError("Pop effect is missing");
            return;
        }

       
        GameObject spawnObj = GameObject.FindGameObjectWithTag("BalloonSpawn");


        if (spawnObj == null)
        {
            Debug.LogError("Balloon Spawn Area not found!");

            return;
        }
        
   
        _spawnArea = spawnObj.GetComponent<BoxCollider>();
 
        _renderer = GetComponent<Renderer>();
        
        // Garante que obtemos a cor correta dependendo do shader
        if (_renderer.material.HasProperty("_Color"))
        {
            _originalColor = _renderer.material.color;
        }
        else if (_renderer.material.HasProperty("_BaseColor")) // URP
        {
            _originalColor = _renderer.material.GetColor("_BaseColor");
        }
        else
        {
            _originalColor = Color.white;
        }

        _collider = GetComponent<Collider>();

        _extents = _collider.bounds.extents;

        _archeryAudioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ArcheryAudioManager>();
       

        if (_archeryAudioManager == null)
        {
            Debug.LogError("ArcheryAudioManager or GameManager object not found in the scene.");

            return ;
        } 

        _colorName = color.ToString().ToLower(); 
    }

    private void Start()
    {
        _colorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();

        if (CanMove)
        {
            InitializeMovement();
        }

        if (CanBeTransluced)
        {
            InitializeTranslucency();
        }
    }

    private void InitializeMovement()
    {
        Bounds area = _spawnArea.bounds;
        
        bool moveVertical = Utils.RandomValueInRange(0f, 1f) > 0.5f;

        if (moveVertical)
        {   
            float minY = area.min.y + _extents.y;
            float maxY = area.max.y - _extents.y;

            // Move apenas no eixo Y (Sobe e Desce)
            float targetY = Utils.RandomValueInRange(minY, maxY);

            transform.DOMoveY(targetY, moveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(_INFINITE_LOOPS, LoopType.Yoyo);

            return ;
        }
        
        float minX = area.min.x + _extents.x;
        float maxX = area.max.x - _extents.x;

        float minZ = area.min.z + _extents.z;
        float maxZ = area.max.z - _extents.z;
        
        Vector3 targetPosition = new(
            Utils.RandomValueInRange(minX, maxX),
            transform.position.y, 
            Utils.RandomValueInRange(minZ, maxZ)
        );

        transform.DOMove(targetPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(_INFINITE_LOOPS, LoopType.Yoyo);
        
    }

    private void InitializeTranslucency()
    {
        // Sequence permite encadear animações e eventos lógicos
        Sequence seq = DOTween.Sequence();

        // 1. Desvanece até minAlpha
        seq.Append(_renderer.material.DOFade(minAlpha, fadeDuration).SetEase(Ease.InOutSine));
        
        // 2. Assim que termina o fade out, desliga o collider
        seq.AppendCallback(() => _collider.enabled = false);

        // 3. (Opcional) Espera um pouco no estado "fantasma"
        seq.AppendInterval(stayTranslucentDuration);

        // 4. Volta a aparecer (Fade In)
        seq.Append(_renderer.material.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine));

        // 5. Assim que fica visível, reativa o collider
        seq.AppendCallback(() => _collider.enabled = true);

        // 6. Define o loop infinito para toda a sequência
        seq.SetLoops(_INFINITE_LOOPS);
    }

    public void Pop()
    {
        transform.DOKill(); 
        _renderer.material.DOKill();

        GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);

        if (fx.TryGetComponent<ParticleSystem>(out var ps))
        {
            ParticleSystem.MainModule main = ps.main;
            main.startColor = _originalColor;
        }

        _archeryAudioManager.PlaySpatialSoundEffect(popSoundEffect, gameObject);

        if (isFromTutorial)
        {
            taskCompleted.Invoke();
        }
  
        Destroy(gameObject); 
    }

    public int GetScoreValue()
    {
      return _colorName == _colorToScore ? scoreValue : 0;   
    }


    private void OnDestroy()
    {
        // Segurança absoluta para garantir que nenhum Tween fica "pendurado" na memória
        transform.DOKill();
        if(_renderer != null && _renderer.material != null)
            _renderer.material.DOKill();
    }
}