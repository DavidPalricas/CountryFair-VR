using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee during flight. Normalizes the launch coming from hand tracking and then applies
/// simplified, bounded aerodynamic forces (lift and drag) until the frisbee lands or leaves the play area.
/// </summary>
/// <remarks>
/// <para>
/// This state handles the complete flight simulation:
/// <list type="bullet">
/// <item><description>On the first physics step, replaces the tracked release velocity with a controlled launch
/// (<see cref="NormalizeLaunch"/>)</description></item>
/// <item><description>Applies lift perpendicular to the velocity, never pointing backwards and capped relative to the
/// frisbee's weight</description></item>
/// <item><description>Applies quadratic drag opposite to the velocity</description></item>
/// <item><description>Detects ground collision and adjusts physics accordingly</description></item>
/// <item><description>Monitors out-of-bounds conditions and triggers appropriate state changes</description></item>
/// </list>
/// </para>
/// <para>
/// Why the launch is normalized: the release velocity is estimated by the Meta XR <c>Grabbable</c> from the last
/// tracked hand poses, which is noisy during fast gestures, and stroke patients often release late (finger extension
/// is impaired), with the arm already pulling back or the wrist rotated. Using that velocity and tilt as-is made the
/// same effort produce throws that dropped, shot off, or even flew backwards.
/// </para>
/// </remarks>

[RequireComponent(typeof(OnPlayerFront))]
public class OnMovement : FrisbeeState
{
    /// <summary>FSM transition fired when the frisbee has touched the ground and slowed down enough to count as landed.</summary>
    private const string StoppedOnGroundTransition = "StoppedOnGround";

    /// <summary>Hand speed (m/s) at or below which the throw uses <see cref="_minLaunchSpeed"/>.</summary>
    [Header("Launch Normalization")]
    [Tooltip("Tracked hand speed (m/s) mapped to the minimum launch speed.")]
    [SerializeField]
    private float _minHandSpeed = 0.8f;

    /// <summary>Hand speed (m/s) at or above which the throw uses <see cref="_maxLaunchSpeed"/>.</summary>
    [Tooltip("Tracked hand speed (m/s) mapped to the maximum launch speed.")]
    [SerializeField]
    private float _maxHandSpeed = 4f;

    /// <summary>
    /// Shape of the hand-speed → launch-speed mapping: <c>launch = lerp(min, max, effort^exponent)</c>, where effort is
    /// the hand speed normalized to [0, 1]. 1 is linear; higher values keep short/slow gestures short while still
    /// reaching <see cref="_maxLaunchSpeed"/> with a full gesture.
    /// </summary>
    [Tooltip("1 = linear. Higher = short/slow gestures throw shorter; a full gesture still reaches the max.")]
    [SerializeField]
    [Range(1f, 3f)]
    private float _effortExponent = 2f;

    /// <summary>
    /// Speed (m/s) that the Meta XR <c>RANSACVelocity</c> caps its synthetic poses at (<c>_maxSyntheticSpeed</c>,
    /// 5 m/s). A release velocity at exactly this speed means hand tracking lost confidence just before the release
    /// and the estimate is an artifact, not the gesture; the averaged velocity from <see cref="OnPlayerFront"/> is
    /// used instead.
    /// </summary>
    [Tooltip("RANSAC synthetic speed cap (m/s). A release at exactly this speed is a tracking artifact.")]
    [SerializeField]
    private float _syntheticSpeedCap = 5f;

    /// <summary>
    /// Launch speed (m/s) of the weakest throw. At 0, a release below <see cref="_minHandSpeed"/> simply drops the
    /// frisbee straight down (for score areas right next to the player).
    /// </summary>
    [Tooltip("Launch speed (m/s) of the weakest throw. 0 = a still release drops straight down.")]
    [SerializeField]
    private float _minLaunchSpeed = 0f;

    /// <summary>Launch speed (m/s) of the strongest throw. With the default aerodynamics it flies roughly 11–18 m depending on elevation (max dog distance: 14 m).</summary>
    [Tooltip("Launch speed (m/s) of the strongest throw.")]
    [SerializeField]
    private float _maxLaunchSpeed = 12f;

    /// <summary>
    /// Maximum horizontal angle (degrees) between the throw and the direction the head is facing. Gestures further to
    /// the side, or backwards, are clamped to this angle so the frisbee always leaves forwards.
    /// </summary>
    [Tooltip("Max horizontal angle (deg) between the throw and the head's forward direction.")]
    [SerializeField]
    [Range(0f, 90f)]
    private float _maxYawDeviation = 45f;

    /// <summary>Lowest launch elevation (degrees above the horizon; negative aims down).</summary>
    [Tooltip("Lowest launch elevation (deg). Negative aims down.")]
    [SerializeField]
    [Range(-30f, 0f)]
    private float _minLaunchElevation = -5f;

    /// <summary>Highest launch elevation (degrees above the horizon).</summary>
    [Tooltip("Highest launch elevation (deg).")]
    [SerializeField]
    [Range(0f, 60f)]
    private float _maxLaunchElevation = 20f;

    /// <summary>
    /// Spin around the vertical axis (rad/s) applied at launch. Purely visual: the disc's tilt is frozen, so spin does
    /// not change the trajectory. The direction follows the tracked spin when there is one.
    /// </summary>
    [Tooltip("Spin around the vertical axis (rad/s) applied at launch. Visual only.")]
    [SerializeField]
    private float _spinSpeed = 20f;

    /// <summary>
    /// Lift coefficient: lift (N) = speed² × liftCoefficient, capped by <see cref="_maxLiftToWeightRatio"/>.
    /// With the default 0.008 and a 0.175 kg frisbee, the cap (0.9 × weight) is reached at about 14 m/s, so lift grows smoothly over the whole launch range.
    /// </summary>
    [Header("Aerodynamic Settings")]
    [Tooltip("Lift (N) = speed² × this value, capped relative to the weight.")]
    [SerializeField]
    private float _liftCoefficient = 0.008f;

    /// <summary>
    /// Maximum lift as a fraction of the frisbee's weight. Below 1 the lift never fully cancels gravity, so a throw can
    /// glide but always descends and never climbs on its own.
    /// </summary>
    [Tooltip("Max lift as a fraction of the weight. 1 = can glide but never climbs by itself.")]
    [SerializeField]
    [Range(0f, 1.5f)]
    private float _maxLiftToWeightRatio = 0.9f;

    /// <summary>Drag coefficient: drag (N) = speed² × dragCoefficient, opposite to the velocity.</summary>
    [Tooltip("Drag (N) = speed² × this value, opposite to the velocity.")]
    [SerializeField]
    private float _dragCoefficient = 0.004f;

    /// <summary>
    /// Unity event invoked when the frisbee enters an <c>OutOfBounds</c> trigger during flight (counts as a miss).
    /// </summary>
    [SerializeField]
    private UnityEvent _playerMissed;

    /// <summary>
    /// Flag indicating whether the frisbee has made contact with the ground.
    /// Used to determine when to transition to the stopped state, and to stop applying lift.
    /// </summary>
    private bool _touchedGround = false;

    /// <summary>
    /// True from <see cref="Enter"/> until the first physics step, where <see cref="NormalizeLaunch"/> runs. Deferred to
    /// that step because the Grabbable writes its throw velocity during the release, which may happen after the
    /// "FrisbeeThrown" transition in the same frame.
    /// </summary>
    private bool _launchPending = false;

    /// <summary>
    /// Original angular drag value from the Rigidbody component.
    /// Stored to restore default physics behavior when exiting this state.
    /// </summary>
    private float _defaultAngularDrag;

    /// <summary>Head reference used to keep the throw facing forwards (from <see cref="FollowPlayerHead"/>).</summary>
    private FollowPlayerHead _followPlayerHead;

    /// <summary>Provides the averaged release velocity used when the Grabbable's estimate is a tracking artifact.</summary>
    private OnPlayerFront _onPlayerFront;

    /// <summary>Tolerance (m/s) when comparing the release speed with <see cref="_syntheticSpeedCap"/>.</summary>
    private const float SyntheticSpeedTolerance = 0.01f;

    /// <summary>Speed (m/s) below which a frisbee that has touched the ground counts as stopped.</summary>
    private const float StopThreshold = 0.1f;

    /// <summary>
    /// Initializes the OnMovement state by storing default physics values and the head reference.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _defaultAngularDrag = _rigidbody.angularDamping;
        _followPlayerHead = GetComponent<FollowPlayerHead>();
        _onPlayerFront = GetComponent<OnPlayerFront>();
    }

    /// <summary>
    /// Called when entering the OnMovement state.
    /// Lowers angular drag so the frisbee spins freely and schedules the launch normalization.
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        _rigidbody.angularDamping = 0.1f;
        _launchPending = true;
    }

    /// <summary>
    /// Called every physics step while in the OnMovement state.
    /// Normalizes the launch on the first step, then applies aerodynamic forces.
    /// </summary>
    /// <remarks>
    /// Running from <c>FixedUpdate</c> (not <c>Update</c>) keeps the flight identical at 72, 90 and 120 Hz.
    /// </remarks>
    public override void FixedExecute()
    {
        base.FixedExecute();

        if (_launchPending)
        {
            _launchPending = false;
            NormalizeLaunch();
        }

        ApplySimpleAerodynamics();
    }

    /// <summary>
    /// Called when exiting the OnMovement state.
    /// Restores default physics values and resets state flags.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cleanup operations:
    /// <list type="bullet">
    /// <item><description>Restores original angular drag value</description></item>
    /// <item><description>Disables trajectory visualization line</description></item>
    /// <item><description>Makes the Rigidbody kinematic so the frisbee stays where it stopped</description></item>
    /// <item><description>Resets ground contact and launch flags</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Exit()
    {
        base.Exit();
        _rigidbody.angularDamping = _defaultAngularDrag;

        _trajectoryLine.enabled = false;

        _rigidbody.isKinematic = true;

        _touchedGround = false;
        _launchPending = false;
    }

    /// <summary>
    /// Replaces the hand-tracked release velocity, tilt and spin with a controlled launch.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item><description><b>Direction:</b> the horizontal direction of the gesture, clamped to
    /// ±<see cref="_maxYawDeviation"/> around the head's forward direction (a backward gesture becomes a forward throw).</description></item>
    /// <item><description><b>Elevation:</b> the vertical angle of the gesture, clamped to
    /// [<see cref="_minLaunchElevation"/>, <see cref="_maxLaunchElevation"/>].</description></item>
    /// <item><description><b>Speed:</b> the tracked hand speed mapped linearly from
    /// [<see cref="_minHandSpeed"/>, <see cref="_maxHandSpeed"/>] to [<see cref="_minLaunchSpeed"/>, <see cref="_maxLaunchSpeed"/>],
    /// so tracking spikes and weak releases both stay inside a playable range.</description></item>
    /// <item><description><b>Orientation:</b> the disc is levelled (only its yaw is kept), discarding the wrist tilt at release.
    /// X/Z rotation stays frozen (set in <see cref="OnPlayerFront"/>) until the first collision.</description></item>
    /// <item><description><b>Spin:</b> a fixed spin around the vertical axis replaces the tracked angular velocity.</description></item>
    /// </list>
    /// </remarks>
    private void NormalizeLaunch()
    {
        Vector3 handVelocity = GetReleaseVelocity(out _);
        Vector3 flatHandVelocity = Vector3.ProjectOnPlane(handVelocity, Vector3.up);

        Vector3 headForward = GetHeadForward(flatHandVelocity);

        Vector3 flatDirection = flatHandVelocity.sqrMagnitude > 0.0001f ? flatHandVelocity.normalized : headForward;
        float yaw = Vector3.SignedAngle(headForward, flatDirection, Vector3.up);
        yaw = Mathf.Clamp(yaw, -_maxYawDeviation, _maxYawDeviation);
        flatDirection = Quaternion.AngleAxis(yaw, Vector3.up) * headForward;

        float elevation = Mathf.Atan2(handVelocity.y, flatHandVelocity.magnitude) * Mathf.Rad2Deg;
        elevation = Mathf.Clamp(elevation, _minLaunchElevation, _maxLaunchElevation) * Mathf.Deg2Rad;

        Vector3 launchDirection = flatDirection * Mathf.Cos(elevation) + Vector3.up * Mathf.Sin(elevation);

        float effort = Mathf.InverseLerp(_minHandSpeed, _maxHandSpeed, handVelocity.magnitude);
        float launchSpeed = Mathf.Lerp(_minLaunchSpeed, _maxLaunchSpeed, Mathf.Pow(effort, _effortExponent));

        Quaternion levelRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
        _rigidbody.rotation = levelRotation;
        transform.rotation = levelRotation;

        float spinSign = _rigidbody.angularVelocity.y < 0f ? -1f : 1f;
        _rigidbody.maxAngularVelocity = Mathf.Max(_rigidbody.maxAngularVelocity, _spinSpeed);

        _rigidbody.linearVelocity = launchDirection * launchSpeed;
        _rigidbody.angularVelocity = Vector3.up * (_spinSpeed * spinSign);
    }

    /// <summary>
    /// Release velocity to normalize: the Grabbable's RANSAC estimate (already written to the Rigidbody), unless its
    /// speed equals <see cref="_syntheticSpeedCap"/>, in which case the averaged velocity from
    /// <see cref="OnPlayerFront.AverageReleaseVelocity"/> replaces it (when available).
    /// </summary>
    /// <param name="source">Which estimate was used, for the calibration log.</param>
    private Vector3 GetReleaseVelocity(out string source)
    {
        Vector3 ransacVelocity = _rigidbody.linearVelocity;
        bool isSyntheticArtifact = Mathf.Abs(ransacVelocity.magnitude - _syntheticSpeedCap) < SyntheticSpeedTolerance;

        if (!isSyntheticArtifact)
        {
            source = "RANSAC";
            return ransacVelocity;
        }

        if (_onPlayerFront.HasAverageReleaseVelocity)
        {
            source = $"média, RANSAC deu {ransacVelocity.magnitude:F2} (artefacto)";
            return _onPlayerFront.AverageReleaseVelocity;
        }

        source = "RANSAC, artefacto sem média disponível";
        return ransacVelocity;
    }

    /// <summary>
    /// Horizontal forward direction of the player's head. Falls back to the gesture direction, then to the frisbee's own
    /// forward, if no head reference is available.
    /// </summary>
    /// <param name="flatHandVelocity">Horizontal part of the tracked release velocity.</param>
    private Vector3 GetHeadForward(Vector3 flatHandVelocity)
    {
        if (_followPlayerHead.CenterEyeTransform != null)
        {
            Vector3 flatHead = Vector3.ProjectOnPlane(_followPlayerHead.CenterEyeTransform.forward, Vector3.up);

            if (flatHead.sqrMagnitude > 0.0001f)
            {
                return flatHead.normalized;
            }
        }

        if (flatHandVelocity.sqrMagnitude > 0.0001f)
        {
            return flatHandVelocity.normalized;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        return flatForward.sqrMagnitude > 0.0001f ? flatForward.normalized : Vector3.forward;
    }

    /// <summary>
    /// Applies bounded lift and drag to the frisbee during flight.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <list type="number">
    /// <item><description><b>Lift direction:</b> perpendicular to the velocity, in the vertical plane that contains it
    /// (<c>Cross(velocity, side)</c>). Any horizontal component pointing against the direction of travel is removed, so
    /// lift can never push the frisbee back towards the player. Unlike the previous model, it does not follow the
    /// disc's tilt, so a wrist rotated at release cannot redirect it.</description></item>
    /// <item><description><b>Lift magnitude:</b> speed² × <see cref="_liftCoefficient"/>, capped at
    /// <see cref="_maxLiftToWeightRatio"/> × weight (instead of a fixed 15 N, ~9× the weight). Only applied before the
    /// first ground contact.</description></item>
    /// <item><description><b>Drag:</b> speed² × <see cref="_dragCoefficient"/>, opposite to the velocity, always applied.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Transitions to the landed state ("StoppedOnGround" transition name) when velocity drops below threshold and frisbee has touched ground.
    /// </para>
    /// </remarks>
    private void ApplySimpleAerodynamics()
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < StopThreshold)
        {
            if (_touchedGround)
            {
                _fsm.ChangeState(StoppedOnGroundTransition);
            }

            return;
        }

        Vector3 moveDirection = velocity / speed;
        float speedSquared = speed * speed;

        _rigidbody.AddForce(-moveDirection * (speedSquared * _dragCoefficient));

        if (_touchedGround)
        {
            return;
        }

        Vector3 side = Vector3.Cross(Vector3.up, moveDirection);

        // Moving (almost) straight up or down: there is no well-defined lift plane.
        if (side.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 liftDirection = Vector3.Cross(moveDirection, side.normalized);

        Vector3 flatMoveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up).normalized;
        float backwardComponent = Vector3.Dot(liftDirection, flatMoveDirection);

        if (backwardComponent < 0f)
        {
            liftDirection = (liftDirection - flatMoveDirection * backwardComponent).normalized;
        }

        float maxLift = _maxLiftToWeightRatio * _rigidbody.mass * Physics.gravity.magnitude;
        float lift = Mathf.Min(speedSquared * _liftCoefficient, maxLift);

        _rigidbody.AddForce(liftDirection * lift);
    }

    /// <summary>
    /// Unity collision callback invoked when the frisbee collides with another object.
    /// Handles landing logic by killing rotation and setting ground contact flag.
    /// </summary>
    /// <param name="other">The collision data associated with this collision event.</param>
    /// <remarks>
    /// On any collision, removes all rigidbody constraints so the frisbee can tumble and settle naturally.
    /// When colliding with objects tagged "Ground", also:
    /// <list type="bullet">
    /// <item><description>Sets <see cref="_touchedGround"/> flag (stops lift and enables the landed transition when speed drops)</description></item>
    /// <item><description>Increases angular damping to 10.0 to rapidly kill spin rotation</description></item>
    /// </list>
    /// Only processes collisions when this state is currently active.
    /// </remarks>
    private void OnCollisionEnter(Collision other)
    {
        if (_fsm.CurrentState == this)
        {
            _rigidbody.constraints = RigidbodyConstraints.None;

            if (other.gameObject.CompareTag(GameManager.GroundTag))
            {
                _touchedGround = true;
                _rigidbody.angularDamping = 10.0f;

                return;
            }
        }
    }

    /// <summary>
    /// Unity collision callback invoked when the frisbee stops colliding with an object.
    /// Handles frisbee bouncing off the ground by resetting angular damping.
    /// </summary>
    /// <param name="other">The collision data associated with this collision exit event.</param>
    /// <remarks>
    /// When leaving collision with objects tagged "Ground", restores low angular damping (0.1) so the frisbee spins freely
    /// again after a bounce.
    /// <see cref="_touchedGround"/> is <b>not</b> cleared, so once the frisbee has touched the ground it gets no more lift
    /// and lands as soon as its speed drops below <see cref="StopThreshold"/>, even mid-bounce.
    /// Only processes collisions when this state is currently active.
    /// </remarks>
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag(GameManager.GroundTag) && _fsm.CurrentState == this)
        {
            _rigidbody.angularDamping = 0.1f;
        }
    }


    /// <summary>Entering an <c>OutOfBounds</c> trigger mid-flight is a miss: raises <see cref="_playerMissed"/> and fires "ReturnToPlayer".</summary>
    /// <param name="other">Trigger the frisbee entered; only colliders tagged <c>OutOfBounds</c> are handled.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GameManager.OutOfBoundsTag) && _fsm.CurrentState == this)
        {
            _playerMissed.Invoke();
            _fsm.ChangeState(ReturnToPlayerTransition);
        }
    }
}
