using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Score area target in the Frisbee mini-game. Supports three area types: NORMAL (can move and blink,
/// score multipliers apply), DOG (fixed elevated area worth 8 points), and TUTORIAL (fires task-complete event).
/// Movement uses a circular DOTween path; visibility uses a timed blink sequence.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ScoreAreaProperties : MonoBehaviour
{
    /// <summary>Tag of every score area; used by <see cref="Landed"/> and <see cref="FrisbeeCheatCodes"/> to find them.</summary>
    public const string ScoreAreaTag = "ScoreArea";
    /// <summary>Physics layer of every score area; used by <see cref="FrisbeeGameManager"/> to keep new areas from overlapping.</summary>
    public const string ScoreAreaLayer = "ScoreArea";

    /// <summary>Determines behavior and base score value; only NORMAL areas support movement/blinking modifiers.</summary>
    [SerializeField]
    private AreaType _areaType = AreaType.Normal;

    /// <summary>Fired in tutorial mode when the player scores here — notifies <see cref="Tutorial"/> that a task completed.</summary>
    [SerializeField]
    private UnityEvent _tutorialTaskCompleted;

    /// <summary>Circular orbit speed expressed as seconds per full revolution.</summary>
    [Header("Movement Settings")]
    [SerializeField]
    private float _moveSpeed = 2f;
    /// <summary>Radius of the circular orbit path.</summary>
    [SerializeField]
    private float _radius = 0.5f;

    /// <summary>Time in seconds the area stays visible during each blink cycle.</summary>
    [Header("Visibility Settings")]
    [SerializeField]
    private float _timeVisible = 3f;
    /// <summary>Time in seconds the area stays invisible during each blink cycle.</summary>
    [SerializeField]
    private float _timeInvisible = 2f;

    /// <summary>Spawn position; center reference of the orbit and target of <see cref="StopMoving"/>.</summary>
    private Vector3 _initialPosition = Vector3.zero;
    /// <summary>Hidden during the invisible phase of the blink cycle.</summary>
    private Renderer _renderer;
    /// <summary>Disabled together with <see cref="_renderer"/> so the frisbee cannot score on an invisible area.</summary>
    private Collider _collider;

    /// <summary>DOTween loop count meaning "repeat forever".</summary>
    private const int InfiniteLoops = -1;

    /// <summary>
    /// DOTween id of the blink sequence, unique per instance. Lets <see cref="StopBlinking"/> kill only the blink without
    /// touching the movement tween on the transform.
    /// </summary>
    private string _blinkId;

    /// <summary>
    /// DOTween id of the orbit and return-to-spawn tweens, unique per instance. Killing by id instead of
    /// <c>transform.DOKill()</c> keeps the float/rotate/pulse loops started by <see cref="ScoreAreaAnimations"/> on
    /// the same transform alive.
    /// </summary>
    private string _moveId;

    /// <summary>Score multiplier while the area orbits.</summary>
    private const int MovingMultiplier = 2;
    /// <summary>Score multiplier while the area blinks; stacks with <see cref="MovingMultiplier"/> (moving + blinking = 6).</summary>
    private const int BlinkingMultiplier = 3;

    /// <summary>Points before multipliers: 8 for the elevated DOG area, 1 otherwise. Set in <see cref="Awake"/>.</summary>
    private int _baseScorePoints = 1;

    /// <summary>True while the orbit is on; set only by <see cref="AdjustMovement"/>.</summary>
    private bool _isMoving = false;
    /// <summary>True while the blink cycle is on; set only by <see cref="AdjustVisibility"/>.</summary>
    private bool _isBlinking = false;


    /// <summary>Found by the <c>GameManager</c> tag; destroys NORMAL areas when they are scored.</summary>
    private FrisbeeGameManager _frisbeeGameManager;

    /// <summary>Points awarded when the frisbee lands here; read by <see cref="Landed"/>.</summary>
    /// <remarks>
    /// Computed from the current state rather than multiplied in place, so re-adjusting an area any number of times
    /// (the manager redistributes all areas after every score) never compounds the multipliers.
    /// </remarks>
    public int ScorePoints => _baseScorePoints * (_isMoving ? MovingMultiplier : 1) * (_isBlinking ? BlinkingMultiplier : 1);

    /// <summary>Kind of score area: regular targets, the dog's catch area, or a tutorial practice area.</summary>
    public enum AreaType
    {
        Normal,
        Dog,
        Tutorial
    }

    /// <summary>Caches components, the spawn position and the tween ids; DOG areas are worth 8 points.</summary>
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _initialPosition = transform.position;

        _blinkId = "blink_" + GetInstanceID();
        _moveId = "move_" + GetInstanceID();


        if (_areaType == AreaType.Dog)
        {
            _baseScorePoints = 8;
        }

        _frisbeeGameManager = GameObject.FindGameObjectWithTag(GameManager.GameManagerTag).GetComponent<FrisbeeGameManager>();

        if (_frisbeeGameManager == null)
        {
            Debug.LogError("FrisbeeGameManager not found in the scene.");
        }
    }

    /// <summary>
    /// Starts or stops the circular orbit tween on NORMAL areas. Moving areas double their score value.
    /// Has no effect on DOG or TUTORIAL area types.
    /// </summary>
    /// <param name="isToMove">True to begin orbiting; false to return to the initial position.</param>
    /// <remarks>Idempotent: a call that does not change the state does nothing, so an orbit is never restarted.</remarks>
    public void AdjustMovement(bool isToMove)
    {
        if (_areaType != AreaType.Normal)
        {
            Debug.LogWarning("Movement adjustment is only applicable for NORMAL area type.");
            return;
        }

        if (isToMove == _isMoving)
        {
            return;
        }

        _isMoving = isToMove;

        if (isToMove)
        {
            StartMoving();
            return;
        }
        StopMoving();
    }

    /// <summary>
    /// Starts or stops the periodic blink sequence on NORMAL areas. Blinking areas triple their score value.
    /// Has no effect on DOG or TUTORIAL area types.
    /// </summary>
    /// <param name="canChangeVisibility">True to start blinking; false to restore permanent visibility.</param>
    /// <remarks>Idempotent: a call that does not change the state does nothing, so blink sequences never pile up.</remarks>
    public void AdjustVisibility(bool canChangeVisibility)
    {  if (_areaType != AreaType.Normal)
        {
            Debug.LogWarning("Visibility adjustment is only applicable for NORMAL area type.");
            return;
        }

        if (canChangeVisibility == _isBlinking)
        {
            return;
        }

        _isBlinking = canChangeVisibility;

        if (canChangeVisibility)
        {
            StartBlinking();
            return;
        }
        StopBlinking();
    }

    /// <summary>Starts an endless closed Catmull-Rom path through 8 points on a circle, one lap every <see cref="_moveSpeed"/> seconds.</summary>
    private void StartMoving()
    {
        DOTween.Kill(_moveId);

        const int Steps = 8;
        Vector3[] circularPath = GetCircularWaypoints(_initialPosition, _radius, Steps);

        transform.DOPath(circularPath, _moveSpeed, PathType.CatmullRom)
            .SetId(_moveId)
            .SetOptions(true)
            .SetEase(Ease.Linear)
            .SetLoops(InfiniteLoops);
    }

    /// <summary>Kills the orbit and eases back to the spawn position.</summary>
    private void StopMoving()
    {
        DOTween.Kill(_moveId);

        const float DurationToInitialPos = 0.5f;

        transform.DOMove(_initialPosition, DurationToInitialPos).SetId(_moveId);
    }

    /// <summary>Starts the endless visible → hidden (renderer and collider off) cycle, replacing any running one.</summary>
   private void StartBlinking()
    {
        DOTween.Kill(_blinkId);

        Sequence sequence = DOTween.Sequence();
        sequence.SetId(_blinkId);

        sequence.AppendInterval(_timeVisible);
        sequence.AppendCallback(() => ToggleVisuals(false));
        sequence.AppendInterval(_timeInvisible);
        sequence.AppendCallback(() => ToggleVisuals(true));
       
        sequence.SetLoops(InfiniteLoops);
    }

    /// <summary>Kills only the blink sequence (by id, leaving the movement tween alive) and makes the area visible again.</summary>
    private void StopBlinking()
    {
        DOTween.Kill(_blinkId);
        
        ToggleVisuals(true); 
    }

    /// <summary>Shows or hides the area, turning the renderer and collider on or off together.</summary>
    private void ToggleVisuals(bool visualState)
    {
        _renderer.enabled = visualState;
        _collider.enabled = visualState;
    }

    /// <summary>
    /// Waypoints of a horizontal circle of radius <paramref name="r"/> whose center is offset to the area's right, so the
    /// path starts near <paramref name="centerPos"/> instead of jumping out to the rim (exactly at it only while the
    /// area's right axis is world X; the area spins, so this is approximate).
    /// </summary>
    private Vector3[] GetCircularWaypoints(Vector3 centerPos, float r, int steps)
    {
        Vector3[] points = new Vector3[steps];
        
        // 360 degrees in radians
        const float FullCircleInRadians = 2 * Mathf.PI;

        float angleStep = FullCircleInRadians  / steps;
        
        Vector3 centerOfCircle = centerPos + (transform.right * r); 
        
        for (int i = 0; i < steps; i++)
        {
            float angle = i * angleStep + Mathf.PI; 
            float x = centerOfCircle.x + r * Mathf.Cos(angle);
            float z = centerOfCircle.z + r * Mathf.Sin(angle);
            
            points[i] = new Vector3(x, centerPos.y, z);
        }

        return points;
    }

    /// <summary>Kills the orbit and blink tweens, so the blink callbacks never touch a destroyed renderer/collider.</summary>
    /// <remarks>The blink sequence is not attached to the transform, so it must be killed by id.</remarks>
    private void OnDestroy()
    {
        DOTween.Kill(_moveId);
        DOTween.Kill(_blinkId);
    }

    /// <summary>Area type; used by <see cref="FrisbeeCheatCodes"/> to tell the dog's catch area from the extra ones.</summary>
    public AreaType GetAreaType()
    {
        return _areaType;
    }

    /// <summary>
    /// Called when the frisbee enters this score area.
    /// TUTORIAL areas fire <c>_tutorialTaskCompleted</c> and self-destroy.
    /// NORMAL areas delegate destruction to <see cref="FrisbeeGameManager.DestroyTarget"/>.
    /// DOG areas simply deactivate themselves.
    /// </summary>
    /// <remarks>Called by <see cref="ScoreAreaAnimations.ScoreAnimation"/> after the balloon burst.</remarks>
    public void OnPlayerScore(){
        if (_areaType == AreaType.Tutorial)
        {
            _tutorialTaskCompleted.Invoke();

            Destroy(gameObject);

            return;
        }

        if (_areaType == AreaType.Normal)
        {
             _frisbeeGameManager.DestroyTarget(gameObject);

             return;
        }

        gameObject.SetActive(false);
    }
}