using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Drives autonomous duck behaviour on a NavMesh water surface:
/// random wandering, vertical bobbing, and rotation wobble to simulate floating movement.
/// All positional offsets use local space so the effect follows a moving parent (e.g. a repositioned pond).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class DuckSwim : MonoBehaviour
{
    /// <summary>Radius around the duck's current world position in which a new NavMesh destination is sampled.</summary>
    [Header("Wandering")]
    [SerializeField] 
    private float _wanderRadius = 3f;

    /// <summary>Minimum distance the new destination must be from the previous one, preventing repeated visits to the same spot.</summary>
    [SerializeField] 
    private float _minMoveDistance = 1f;

    /// <summary>Peak vertical offset above the local rest position, in local units.</summary>
    [Header("Bobbing Animation")]
    [SerializeField] private float _bobAmplitude = 0.02f;

    /// <summary>Bobbing cycles per second.</summary>
    [SerializeField] private float _bobFrequency = 1.5f;

    /// <summary>Maximum tilt angle in degrees applied to Z and X axes to simulate floating sway.</summary>
    [Header("Rotation Wobble")]
    [SerializeField] private float _wobbleAmplitude = 3f;

    /// <summary>Wobble cycles per second.</summary>
    [SerializeField] private float _wobbleFrequency = 0.8f;

    /// <summary>How quickly the duck rotates to face its movement direction (lerp factor, degrees per second).</summary>
    [SerializeField] private float _rotationSpeed = 5f;

    /// <summary>Drives the wandering; its rotation updates are disabled so <see cref="ApplyWobble"/> owns the rotation.</summary>
    private NavMeshAgent _agent;

    /// <summary>Local Y at spawn; bobbing oscillates around this value so the effect stays relative to the parent.</summary>
    private float _baseLocalY;

    /// <summary>Per-instance sine phase so multiple ducks bob out of sync.</summary>
    private float _phaseOffset;

    /// <summary>Previous destination, compared against <see cref="_minMoveDistance"/> when picking the next one.</summary>
    private Vector3 _lastDestination;

    /// <summary>
    /// Captures the local rest Y, disables agent auto-rotation (handled manually in <see cref="ApplyWobble"/>),
    /// and seeds a unique phase offset for this instance.
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _baseLocalY = transform.localPosition.y;

        _phaseOffset = Utils.RandomValueInRange(0f, Mathf.PI * 2f);
        _lastDestination = transform.position;
    }

    /// <summary>Picks the first random NavMesh destination so the duck begins moving immediately.</summary>
    private void Start()
    {
        SetNewDestination();
    }

    /// <summary>Applies bobbing and wobble every frame, and requests a new destination once the current one is reached.</summary>
    private void Update()
    {
        ApplyBobbing();
        ApplyWobble();

        if (_agent.HasReachedDestination())
        {
            SetNewDestination();
        }
    }

    /// <summary>Sends the agent to a new random point; keeps the current destination if no valid point is found.</summary>
    private void SetNewDestination()
    {
        if (TryGetRandomPoint(out Vector3 target))
        {
            _agent.SetDestination(target);
        }
    }

    /// <summary>
    /// Attempts up to 15 times to find a valid NavMesh point within <see cref="_wanderRadius"/> of the duck.
    /// Uses a 2D circle sample (X/Z only) with a tight snap distance so off-surface candidates fail
    /// and retry rather than all converging on the same NavMesh edge.
    /// </summary>
    /// <param name="result">World position of the chosen destination, or the duck's current position on failure.</param>
    /// <returns>True if a suitable point was found; false if all attempts failed.</returns>
    private bool TryGetRandomPoint(out Vector3 result)
    {
        const int MaxTries = 15;
        const float MaxDistance = 0.5f;

        for (int i = 0; i < MaxTries; i++)
        {
            Vector2 circle = Random.insideUnitCircle * _wanderRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, MaxDistance, _agent.areaMask)
                && Vector3.Distance(hit.position, _lastDestination) >= _minMoveDistance)
            {
                _lastDestination = hit.position;
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    /// <summary>
    /// Oscillates the duck's local Y around <see cref="_baseLocalY"/> using a sine wave,
    /// keeping the effect parent-relative so a moving pond carries the ducks correctly.
    /// </summary>
    private void ApplyBobbing()
    {
        float time = Time.time + _phaseOffset;
        Vector3 localPos = transform.localPosition;
        localPos.y = _baseLocalY + Mathf.Sin(time * _bobFrequency * Mathf.PI * 2f) * _bobAmplitude;
        transform.localPosition = localPos;
    }

    /// <summary>
    /// Applies a sinusoidal tilt on X and Z to simulate floating sway.
    /// When the agent is moving, smoothly rotates Y to face the velocity direction.
    /// </summary>
    private void ApplyWobble()
    {
        float time = Time.time + _phaseOffset;
        float wobbleZ = Mathf.Sin(time * _wobbleFrequency * Mathf.PI * 2f) * _wobbleAmplitude;
        float wobbleX = Mathf.Cos(time * _wobbleFrequency * 0.7f * Mathf.PI * 2f) * _wobbleAmplitude * 0.5f;

        Vector3 euler = transform.localEulerAngles;

        if (_agent.velocity.sqrMagnitude > 0.01f)
        {
            float targetY = Quaternion.LookRotation(_agent.velocity.normalized).eulerAngles.y;
            euler.y = Mathf.LerpAngle(euler.y, targetY, Time.deltaTime * _rotationSpeed);
        }

        euler.z = wobbleZ;
        euler.x = wobbleX;
        transform.localEulerAngles = euler;
    }
}
