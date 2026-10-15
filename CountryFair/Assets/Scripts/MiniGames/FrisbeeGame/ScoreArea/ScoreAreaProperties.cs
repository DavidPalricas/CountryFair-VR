using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Serialization;

/// <summary>
/// Score area target in the Frisbee mini-game. Supports three area types: NORMAL (can move and blink,
/// score multipliers apply), DOG (fixed elevated area worth 8 points), and TUTORIAL (fires task-complete event).
/// Movement uses a circular DOTween path; visibility uses a timed blink sequence.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ScoreAreaProperties : MonoBehaviour
{
    /// <summary>Determines behavior and base score value; only NORMAL areas support movement/blinking modifiers.</summary>
    [SerializeField]
    [FormerlySerializedAs("areaType")]
    private AreaType _areaType = AreaType.Normal;

    /// <summary>Fired in tutorial mode when the player scores here — notifies <see cref="Tutorial"/> that a task completed.</summary>
    [SerializeField]
    [FormerlySerializedAs("tutorialTaskCompleted")]
    private UnityEvent _tutorialTaskCompleted;

    /// <summary>Circular orbit speed expressed as seconds per full revolution.</summary>
    [Header("Movement Settings")]
    [SerializeField]
    [FormerlySerializedAs("moveSpeed")]
    private float _moveSpeed = 2f;
    /// <summary>Radius of the circular orbit path.</summary>
    [SerializeField]
    [FormerlySerializedAs("radius")]
    private float _radius = 0.5f;

    /// <summary>Time in seconds the area stays visible during each blink cycle.</summary>
    [Header("Visibility Settings")]
    [SerializeField]
    [FormerlySerializedAs("timeVisible")]
    private float _timeVisible = 3f;
    /// <summary>Time in seconds the area stays invisible during each blink cycle.</summary>
    [SerializeField]
    [FormerlySerializedAs("timeInvisible")]
    private float _timeInvisible = 2f;

    /// <summary>Spawn position; center reference of the orbit and target of <see cref="StopMoving"/>.</summary>
    private Vector3 _initialPosition = Vector3.zero;
    private Renderer _renderer;
    private Collider _collider;

    private const int InfiniteLoops = -1;

    // Unique ID to differentiate Blinking from Moving
    private string _blinkId; 


    private FrisbeeGameManager _frisbeeGameManager;

    /// <summary>Points awarded when the frisbee lands here; read by <see cref="Landed"/>.</summary>
    /// <remarks>
    /// <see cref="AdjustMovement"/> and <see cref="AdjustVisibility"/> multiply it on every call and nothing resets it,
    /// so an area that is re-adjusted several times keeps accumulating the multipliers.
    /// </remarks>
    public int ScorePoints  { get; private set; } = 1;

    /// <summary>Kind of score area: regular targets, the dog's catch area, or a tutorial practice area.</summary>
    public enum AreaType
    {
        Normal,
        Dog,
        Tutorial
    }

    /// <summary>Caches components, the spawn position and the blink id; DOG areas are worth 8 points.</summary>
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _initialPosition = transform.position;
        
        _blinkId = "blink_" + GetInstanceID();


        if (_areaType == AreaType.Dog)
        {
            ScorePoints = 8;
        }

        _frisbeeGameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FrisbeeGameManager>();

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
    public void AdjustMovement(bool isToMove)
    {
        if (_areaType != AreaType.Normal)
        {
            Debug.LogWarning("Movement adjustment is only applicable for NORMAL area type.");
            return;
        }

        if (_areaType == AreaType.Normal && isToMove)
        {   
            ScorePoints *= 2;
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
    public void AdjustVisibility(bool canChangeVisibility)
    {  if (_areaType != AreaType.Normal)
        {
            Debug.LogWarning("Visibility adjustment is only applicable for NORMAL area type.");
            return;
        }
        
        if (canChangeVisibility)
        { 
            ScorePoints *= 3;
            StartBlinking();
            return;
        }
        StopBlinking();
    }
    
    /// <summary>Starts an endless closed Catmull-Rom path through 8 points on a circle, one lap every <see cref="_moveSpeed"/> seconds.</summary>
    private void StartMoving()
    {   
        transform.DOKill(false);

        const int Steps = 8;
        Vector3[] circularPath = GetCircularWaypoints(_initialPosition, _radius, Steps);

        transform.DOPath(circularPath, _moveSpeed, PathType.CatmullRom)
            .SetOptions(true)
            .SetEase(Ease.Linear)
            .SetLoops(InfiniteLoops);
    }

    /// <summary>Kills the orbit and eases back to the spawn position.</summary>
    private void StopMoving()
    {
        transform.DOKill(false);

        const float DurationToInitialPos = 0.5f;
        
        transform.DOMove(_initialPosition, DurationToInitialPos); 
    }

    /// <summary>Starts the endless visible → hidden (renderer and collider off) cycle.</summary>
   private void StartBlinking()
    {
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

    /// <summary>Kills the movement tween of NORMAL areas.</summary>
    /// <remarks>The blink sequence is not attached to the transform, so it is not killed here.</remarks>
    private void OnDestroy()
    {   
        if (_areaType == AreaType.Normal)
        {
              transform.DOKill(); 

              return;
        }
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