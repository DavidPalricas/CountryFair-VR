using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Serialization;


/// <summary>
/// Individual archery-game balloon target. Manages wandering movement (DOTween, random points inside the spawn area), periodic transparency
/// (fade-in/fade-out cycle with collider toggling), score-value multipliers, pop effects, and integration
/// with <see cref="ArcheryGameManager"/> for target lifecycle management.
/// Score value is multiplied by 2 when moving and by 3 when transparent.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class BalloonArcheryGame : MonoBehaviour
{
    /// <summary>Balloon colors; the lower-case name is compared with the <c>BalloonColorToScore</c> PlayerPrefs value.</summary>
    public enum Colors { RED, BLUE, YELLOW }

    /// <summary>The prefab this balloon was instantiated from; stored for lifecycle management in <see cref="ArcheryGameManager"/>.</summary>
    public GameObject OriginalPrefab { get; set; }

    /// <summary>Color of this balloon prefab; decides whether hitting it scores.</summary>
    [FormerlySerializedAs("color")]
    [SerializeField] private Colors _color = Colors.RED;
    /// <summary>Particle effect spawned on pop; its start color is tinted with the balloon color.</summary>
    [FormerlySerializedAs("popEffect")]
    [SerializeField] private GameObject _popEffect;



    /// <summary>Tutorial balloons raise <see cref="_taskCompleted"/> and destroy themselves instead of going through <see cref="ArcheryGameManager"/>.</summary>
    [Header("Settings")]
    [FormerlySerializedAs("isFromTutorial")]
    [SerializeField] private bool _isFromTutorial = false;
    /// <summary>Raised when a tutorial balloon is popped, to advance the tutorial step.</summary>
    [FormerlySerializedAs("taskCompleted")]
    [SerializeField] private UnityEvent _taskCompleted;

    /// <summary>Seconds to travel between two wander points; overwritten by <see cref="SetMoveDuration"/> as difficulty changes.</summary>
    [Header("Movement Settings")]
    [FormerlySerializedAs("moveDuration")]
    [SerializeField] private float _moveDuration = 2f;

    /// <summary>Seconds for each fade out / fade in of the transparency cycle.</summary>
    [Header("Visual Settings")]
    [FormerlySerializedAs("fadeDuration")]
    [SerializeField] private float _fadeDuration = 1f;
    /// <summary>Seconds the balloon stays faded (and unhittable, collider off) before fading back in.</summary>
    [FormerlySerializedAs("stayTranslucentDuration")]
    [SerializeField] private float _stayTranslucentDuration = 1f;
    /// <summary>Alpha reached at the faded end of the cycle (0–1).</summary>
    [FormerlySerializedAs("minAlpha")]
    [SerializeField] private float _minAlpha = 0.3f;

    /// <summary>Points for a hit on a static, opaque balloon of the scoring color.</summary>
    private const int BaseScore = 1;
    /// <summary>Score multiplier while the balloon wanders (harder to hit).</summary>
    private const int MovingMultiplier = 2;
    /// <summary>Score multiplier while the fade cycle is on; stacks with <see cref="MovingMultiplier"/> (moving + transparent = 6).</summary>
    private const int TransparentMultiplier = 3;

    /// <summary>True while movement is enabled by <see cref="AdjustMovement"/>; applies <see cref="MovingMultiplier"/>.</summary>
    private bool _isMoving = false;
    /// <summary>True while the fade cycle is enabled by <see cref="AdjustTransparency"/>; applies <see cref="TransparentMultiplier"/>.</summary>
    private bool _isTransparent = false;

    /// <summary>Balloon renderer; its (per-instance) material is faded by the transparency cycle.</summary>
    private Renderer _renderer;
    /// <summary>The balloon's hit collider; toggled by the transparency cycle and force-enabled by the score cheat.</summary>
    public Collider Collider { get; private set; }
    /// <summary>
    /// Material color read in <see cref="Awake"/> (<c>_Color</c> or URP <c>_BaseColor</c>), used to tint the pop effect.
    /// Stays transparent black if the material has neither property.
    /// </summary>
    private Color _originalColor;
    /// <summary>Spawn position; where the balloon eases back to when movement stops.</summary>
    private Vector3 _initialPosition;
    /// <summary>Lower-case <see cref="_color"/> name, cached for the comparison with <c>BalloonColorToScore</c>.</summary>
    private string _colorName;

    /// <summary>Per-instance DOTween id of the wander tween, so one balloon's movement can be queried and killed without touching others.</summary>
    private string _moveId;
    /// <summary>Per-instance DOTween id of the fade sequence, so it can be killed without touching the movement tween.</summary>
    private string _fadeId;
    /// <summary>DOTween loop count meaning "repeat forever".</summary>
    private const int InfiniteLoops = -1;

    /// <summary>Plays the spatial pop sound; found on the <c>GameManager</c>-tagged object.</summary>
    private ArcheryAudioManager _archeryAudioManager;

    /// <summary>Destroys and respawns the balloon after a non-tutorial pop; found on the <c>GameManager</c>-tagged object.</summary>
    private ArcheryGameManager _archeryGameManager;

    /// <summary>Sound played on every pop (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _popSoundEffect = AudioManager.GameSoundEffects.BalloonPop;
    /// <summary>Box (tagged <c>BalloonSpawn</c>) whose bounds limit the wander points.</summary>
    private BoxCollider _spawnArea;

    /// <summary>
    /// Caches components and tween ids, reads the original material color (<c>_Color</c> or URP <c>_BaseColor</c>),
    /// and finds the spawn area and the archery managers by tag.
    /// </summary>
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        Collider = GetComponent<Collider>();
        _initialPosition = transform.position;

        _moveId = "move_" + GetInstanceID();
        _fadeId = "fade_" + GetInstanceID();

        _colorName = _color.ToString().ToLower();

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


    /// <summary>Starts wandering from the current position, unless it is already wandering (cancels a running return-to-spawn tween).</summary>
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
        transform.DOMove(GetRandomPointInSpawnArea(), _moveDuration)
            .SetId(_moveId)
            .SetEase(Ease.InOutSine)
            .OnComplete(MoveToNextWanderPoint);
    }

    /// <summary>Random point inside the spawn area, inset by the balloon's own extents so it stays fully inside.</summary>
    /// <remarks>The inset is clamped to the area's center, so a balloon bigger than the area on some axis does not produce an inverted range.</remarks>
    private Vector3 GetRandomPointInSpawnArea()
    {
        Bounds area = _spawnArea.bounds;
        Vector3 extents = Collider.bounds.extents;

        Vector3 min = Vector3.Min(area.min + extents, area.center);
        Vector3 max = Vector3.Max(area.max - extents, area.center);

        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
    }

    /// <summary>Kills the wander tween and eases the balloon back to its spawn position.</summary>
    private void StopMovement()
    {
        if (!DOTween.IsTweening(_moveId))
        {
            return;
        } 
        
        DOTween.Kill(_moveId);
        transform.DOMove(_initialPosition, 1f).SetEase(Ease.OutQuad);
    }

    /// <summary>Starts the infinite fade-out → hold (collider off) → fade-in (collider on) loop, unless it is already running.</summary>
    private void StartTranslucency()
    {
        if (DOTween.IsTweening(_fadeId))
        {
            return;
        } 

        Sequence seq = DOTween.Sequence();
        seq.SetId(_fadeId);

        seq.Append(_renderer.material.DOFade(_minAlpha, _fadeDuration).SetEase(Ease.InOutSine));
        seq.AppendCallback(() => Collider.enabled = false);
        seq.AppendInterval(_stayTranslucentDuration);
        seq.Append(_renderer.material.DOFade(1f, _fadeDuration).SetEase(Ease.InOutSine));
        seq.AppendCallback(() => Collider.enabled = true); 

        seq.SetLoops(InfiniteLoops);
    }

    /// <summary>Kills the fade cycle, fades back to fully opaque in 0.5 s and re-enables the collider immediately.</summary>
    private void StopTranslucency()
    {
        if (!DOTween.IsTweening(_fadeId))
        {
            return;
        }

        DOTween.Kill(_fadeId);

        _renderer.material.DOFade(1f, 0.5f);
        Collider.enabled = true;
    }

    /// <summary>
    /// Kills all running tweens, spawns the pop particle effect tinted in the balloon color, plays the pop sound,
    /// and delegates destruction to <see cref="ArcheryGameManager.DestroyTarget"/>.
    /// Tutorial balloons fire <c>_taskCompleted</c> instead.
    /// </summary>
    public void Pop()
    {
        transform.DOKill();
        _renderer.material.DOKill();

        if (_popEffect != null)
        {
            GameObject fx = Instantiate(_popEffect, transform.position, Quaternion.identity);
            if (fx.TryGetComponent<ParticleSystem>(out var ps))
            {
                ParticleSystem.MainModule main = ps.main;
                main.startColor = _originalColor;
            }
        }

        _archeryAudioManager.PlaySpatialSoundEffect(_popSoundEffect, gameObject);
        
        if (_isFromTutorial)
        {   
            _taskCompleted.Invoke();

            Destroy(transform.parent.gameObject);

            return;
        }

        _archeryGameManager.DestroyTarget(transform.parent.gameObject, OriginalPrefab);
    }

    /// <summary>
    /// Returns the accumulated score value if this balloon's color matches the current scoring color
    /// (<c>PlayerPrefs "BalloonColorToScore"</c>), or 0 otherwise.
    /// </summary>
    /// <remarks>Normal = 1, moving = 2, transparent = 3, moving + transparent = 6.</remarks>
    public int GetScoreValue()
    {
        string colorToScore = PlayerPrefs.GetString("BalloonColorToScore", "red").ToLower();

        if (_colorName != colorToScore)
        {
            return 0;
        }

        int scoreValue = BaseScore;

        if (_isMoving)
        {
            scoreValue *= MovingMultiplier;
        }

        if (_isTransparent)
        {
            scoreValue *= TransparentMultiplier;
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
        _moveDuration = duration;
        
        if (DOTween.IsTweening(_moveId))
        {
            StopMovement();
            StartMovement();
        }
    }

    /// <summary>Kills this balloon's tweens so DOTween does not keep animating a destroyed transform/material.</summary>
    private void OnDestroy()
    {
        transform.DOKill();
        if (_renderer && _renderer.material) _renderer.material.DOKill();
    }
}