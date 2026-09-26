using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


/// <summary>
/// Individual archery-game balloon target. Manages looping movement (DOTween yoyo), periodic transparency
/// (fade-in/fade-out cycle with collider toggling), score-value multipliers, pop effects, and integration
/// with <see cref="ArcheryGameManager"/> for target lifecycle management.
/// Score value is multiplied by 2 when moving and by 3 when transparent.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class BalloonArcheryGame : MonoBehaviour
{
    public enum Colors { RED, BLUE, YELLOW }

    /// <summary>The prefab this balloon was instantiated from; stored for lifecycle management in <see cref="ArcheryGameManager"/>.</summary>
    public GameObject OriginalPrefab { get; set; }

    [SerializeField] private Colors color = Colors.RED;
    [SerializeField] private GameObject popEffect;



    [Header("Settings")]
    [SerializeField] private bool isFromTutorial = false;
    [SerializeField] private UnityEvent taskCompleted;

    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 2f;

    [Header("Visual Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float stayTranslucentDuration = 1f;
    [SerializeField] private float minAlpha = 0.3f;

    private const int _BASE_SCORE = 1;
    private const int _MOVING_MULTIPLIER = 2;
    private const int _TRANSPARENT_MULTIPLIER = 3;

    private bool _isMoving = false;
    private bool _isTransparent = false;

    private Renderer _renderer;
    private Collider _collider;
    private Color _originalColor;
    private Vector3 _initialPosition;
    private string _colorName;

    private string _moveId;
    private string _fadeId;
    private const int _INFINITE_LOOPS = -1;

    private ArcheryAudioManager _archeryAudioManager;

    private ArcheryGameManager _archeryGameManager;

    private readonly AudioManager.GameSoundEffects popSoundEffect = AudioManager.GameSoundEffects.BALLOON_POP;
    private BoxCollider _spawnArea;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _initialPosition = transform.position;

        _moveId = "move_" + GetInstanceID();
        _fadeId = "fade_" + GetInstanceID();

        _colorName = color.ToString().ToLower();

        if (_renderer.material.HasProperty("_Color"))
        {
            _originalColor = _renderer.material.color;
        } 
        else if (_renderer.material.HasProperty("_BaseColor"))
        {
            _originalColor = _renderer.material.GetColor("_BaseColor");
        } 

        GameObject spawner = GameObject.FindGameObjectWithTag("BalloonSpawn");

        if (spawner == null)
        {
            Debug.LogError("BalloonSpawner GameObject not found in the scene.");
            return;
        }

        _spawnArea = spawner.GetComponent<BoxCollider>();

        if (_spawnArea == null)
        {
            Debug.LogError("BoxCollider component not found on BalloonSpawner GameObject.");
            return;
        }

    
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");


        if (gameManager == null)
        {
            Debug.LogError("ArcheryGameManager GameObject not found in the scene.");
            return;
        }

        _archeryAudioManager = gameManager.GetComponent<ArcheryAudioManager>();
        _archeryGameManager = gameManager.GetComponent<ArcheryGameManager>();

        if (_archeryGameManager == null || _archeryAudioManager == null)
        {
            Debug.LogError("ArcheryGameManager or ArcheryAudioManager component not found on GameManager GameObject.");
        }
    }
 
    /// <summary>
    /// Starts or stops the looping movement tween. Moving balloons award double score points.
    /// </summary>
    /// <param name="shouldMove">True to begin movement; false to return to the initial spawn position.</param>
    public void AdjustMovement(bool shouldMove)
    {
        _isMoving = shouldMove;

        if (shouldMove)
        {   
            StartMovement();

            return;
        } 

        StopMovement();
    }

    /// <summary>
    /// Starts or stops the periodic fade-in/fade-out cycle. Transparent balloons award triple score points.
    /// The collider is disabled during the invisible phase so the arrow cannot hit them.
    /// </summary>
    /// <param name="shouldFade">True to start blinking; false to restore full opacity.</param>
    public void AdjustTransparency(bool shouldFade)
    {
        _isTransparent = shouldFade;

        if (shouldFade)
        {   
            StartTranslucency();
            return;
        }

        StopTranslucency();
    }


    private void StartMovement()
    {
        if (DOTween.IsTweening(_moveId))
        {
            return;
        }

        transform.DOKill(false);

        MoveToNextWanderPoint();
    }

    /// <summary>
    /// Moves the balloon to a new random point inside the spawn area and, on arrival, picks another one,
    /// so each balloon wanders around the whole area instead of bouncing between two fixed points.
    /// </summary>
    private void MoveToNextWanderPoint()
    {
        transform.DOMove(GetRandomPointInSpawnArea(), moveDuration)
            .SetId(_moveId)
            .SetEase(Ease.InOutSine)
            .OnComplete(MoveToNextWanderPoint);
    }

    private Vector3 GetRandomPointInSpawnArea()
    {
        Bounds area = _spawnArea.bounds;
        Vector3 extents = _collider.bounds.extents;

        // Clamp the margins so a balloon bigger than the area on some axis doesn't produce an inverted range
        Vector3 min = Vector3.Min(area.min + extents, area.center);
        Vector3 max = Vector3.Max(area.max - extents, area.center);

        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
    }

    private void StopMovement()
    {
        if (!DOTween.IsTweening(_moveId))
        {
            return;
        } 
        
        DOTween.Kill(_moveId);
        transform.DOMove(_initialPosition, 1f).SetEase(Ease.OutQuad);
    }

    private void StartTranslucency()
    {
        if (DOTween.IsTweening(_fadeId))
        {
            return;
        } 

        Sequence seq = DOTween.Sequence();
        seq.SetId(_fadeId);

        seq.Append(_renderer.material.DOFade(minAlpha, fadeDuration).SetEase(Ease.InOutSine));
        seq.AppendCallback(() => _collider.enabled = false);
        seq.AppendInterval(stayTranslucentDuration);
        seq.Append(_renderer.material.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine));
        seq.AppendCallback(() => _collider.enabled = true); 

        seq.SetLoops(_INFINITE_LOOPS);
    }

    private void StopTranslucency()
    {
        if (!DOTween.IsTweening(_fadeId))
        {
            return;
        }

        DOTween.Kill(_fadeId);

        _renderer.material.DOFade(1f, 0.5f);
        _collider.enabled = true;
    }

    /// <summary>
    /// Kills all running tweens, spawns the pop particle effect tinted in the balloon color, plays the pop sound,
    /// and delegates destruction to <see cref="ArcheryGameManager.DestroyTarget"/>.
    /// Tutorial balloons fire <c>taskCompleted</c> instead.
    /// </summary>
    public void Pop()
    {
        transform.DOKill();
        _renderer.material.DOKill();

        if (popEffect != null)
        {
            GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);
            if (fx.TryGetComponent<ParticleSystem>(out var ps))
            {
                ParticleSystem.MainModule main = ps.main;
                main.startColor = _originalColor;
            }
        }

        _archeryAudioManager.PlaySpatialSoundEffect(popSoundEffect, gameObject);
        
        if (isFromTutorial)
        {   
            taskCompleted.Invoke();

            Destroy(transform.parent.gameObject);

            return;
        }

        _archeryGameManager.DestroyTarget(transform.parent.gameObject, OriginalPrefab);
    }

    /// <summary>
    /// Returns the accumulated score value if this balloon's color matches the current scoring color
    /// (<c>PlayerPrefs "BalloonColorToScore"</c>), or 0 otherwise.
    /// </summary>
    public int GetScoreValue()
    {
        string colorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();

        if (_colorName != colorToScore)
        {
            return 0;
        }

        // Normal = 1, moving = 2, transparent = 3, moving + transparent = 6
        int scoreValue = _BASE_SCORE;

        if (_isMoving)
        {
            scoreValue *= _MOVING_MULTIPLIER;
        }

        if (_isTransparent)
        {
            scoreValue *= _TRANSPARENT_MULTIPLIER;
        }

        return scoreValue;
    }

    /// <summary>Returns the lowercase color name of this balloon (e.g. <c>"red"</c>, <c>"blue"</c>, <c>"yellow"</c>).</summary>
    public string GetBalloonColorName()
    {
        return _colorName;
    }


    /// <summary>
    /// Updates the movement tween duration. If the balloon is currently moving, restarts the tween with the new duration.
    /// </summary>
    /// <param name="duration">New movement cycle duration in seconds.</param>
    public void SetMoveDuration(float duration)
    {
        moveDuration = duration;
        
        if (DOTween.IsTweening(_moveId))
        {
            StopMovement();
            StartMovement();
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (_renderer && _renderer.material) _renderer.material.DOKill();
    }
}