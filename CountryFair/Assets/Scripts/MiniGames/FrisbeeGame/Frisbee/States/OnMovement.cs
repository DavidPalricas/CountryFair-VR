using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee during flight, applying realistic aerodynamic forces.
/// Manages physics simulation including lift, drag, angle of attack calculations,
/// and collision detection for landing and out-of-bounds scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This state handles the complete flight simulation:
/// <list type="bullet">
/// <item><description>Applies dynamic lift based on velocity squared and angle of attack</description></item>
/// <item><description>Calculates aerodynamic drag depending on disc orientation</description></item>
/// <item><description>Detects ground collision and adjusts physics accordingly</description></item>
/// <item><description>Monitors out-of-bounds conditions and triggers appropriate state changes</description></item>
/// </list>
/// </para>
/// <para>
/// Aerodynamic behavior:
/// - When flying flat (low angle of attack): minimal drag, stable flight
/// - When tilted (high angle of attack): increased drag acts as parachute effect
/// - Lift is proportional to speed squared, making faster throws glide further
/// </para>
/// </remarks>
public class OnMovement : FrisbeeState
{
    /// <summary>FSM transition fired when the frisbee has touched the ground and slowed down enough to count as landed.</summary>
    private const string StoppedOnGroundTransition = "StoppedOnGround";

    /// <summary>
    /// Lift force strength coefficient applied to the frisbee during flight.
    /// Higher values increase upward force, making the disc rise more during flight.
    /// Used in the lift calculation: lift (N) = speed² × liftStrength × aerodynamicFactor, capped at 15 N. With the
    /// default 0.15, the cap is reached at 10 m/s with the underside fully facing the air.
    /// </summary>
    [Header("Aerodynamic Settings")]
    [Tooltip("Lift force strength. Higher values make the disc rise more.")]
    [SerializeField]
    private float _liftStrength = 0.15f;
    
    /// <summary>
    /// Unity event invoked when the frisbee enters an <c>OutOfBounds</c> trigger during flight (counts as a miss).
    /// </summary>
    [SerializeField]
    private UnityEvent _playerMissed;

    /// <summary>
    /// Flag indicating whether the frisbee has made contact with the ground.
    /// Used to determine when to transition to the stopped state.
    /// </summary>
    private bool _touchedGround = false;

    /// <summary>
    /// Original angular drag value from the Rigidbody component.
    /// Stored to restore default physics behavior when exiting this state.
    /// </summary>
    private float _defaultAngularDrag;

    /// <summary>Speed (m/s) below which a frisbee that has touched the ground counts as stopped.</summary>
    private const float StopThreshold = 0.1f;

    /// <summary>
    /// Initializes the OnMovement state by storing default physics values.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Stores the default angular drag value for restoration when exiting flight state.
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        _defaultAngularDrag = _rigidbody.angularDamping;
    }

    /// <summary>
    /// Called when entering the OnMovement state.
    /// Configures physics for realistic flight by reducing angular drag.
    /// </summary>
    /// <remarks>
    /// Sets angular damping to a low value (0.1) to ensure the frisbee spins freely in the air,
    /// simulating realistic disc flight behavior with minimal rotational resistance.
    /// </remarks>
    public override void Enter()
    {
        base.Enter();

        _rigidbody.angularDamping = 0.1f;
    }

    /// <summary>
    /// Called every physics step while in the OnMovement state.
    /// Continuously applies aerodynamic forces to simulate realistic frisbee flight.
    /// </summary>
    /// <remarks>
    /// Invokes <see cref="ApplySimpleAerodynamics"/> once per physics step to calculate and apply lift and drag forces
    /// based on current velocity and orientation. Running it from <c>FixedUpdate</c> (not <c>Update</c>) keeps the
    /// flight identical at 72, 90 and 120 Hz.
    /// </remarks>
    public override void FixedExecute()
    {
        base.FixedExecute();
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
    /// <item><description>Resets ground contact flag</description></item>
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
    }

    /// <summary>
    /// Applies realistic aerodynamic forces to the frisbee during flight.
    /// Calculates lift and drag based on velocity, orientation, and angle of attack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Physics implementation based on real frisbee aerodynamics:
    /// <list type="number">
    /// <item><description><b>Angle of Attack (AoA):</b> Calculated using dot product between disc's up vector and inverted velocity direction.
    /// <list type="bullet">
    /// <item>AoA = -1: Air hits the top completely (pushes downward)</item>
    /// <item>AoA = 0: Air cuts through disc like a knife (zero lift)</item>
    /// <item>AoA = +1: Air hits the belly completely (maximum lift - parachute effect)</item>
    /// </list>
    /// </description></item>
    /// <item><description><b>Aerodynamic Factor:</b> Ensures lift is only generated when air hits the underside (AoA ≥ 0).
    /// Negative AoA values are clamped to zero to prevent upward lift when disc is inverted.</description></item>
    /// <item><description><b>Dynamic Lift Calculation:</b> Uses velocity squared (speed²) to simulate real aerodynamics:
    /// <list type="bullet">
    /// <item>Slow throws drop quickly (minimal lift)</item>
    /// <item>Fast throws glide far (strong lift)</item>
    /// <item>Multiplied by aerodynamic factor to correct for disc orientation</item>
    /// </list>
    /// </description></item>
    /// <item><description><b>Lift Clamping:</b> Maximum lift force is limited to 15N to prevent unrealistic behavior from VR tracking glitches.</description></item>
    /// <item><description><b>Induced Drag:</b> Whenever lift is generated (AoA &gt; 0), an extra drag of 10 % of the lift is applied opposite to the velocity direction.
    /// This prevents infinite gliding and creates realistic energy loss during flight.</description></item>
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

        Vector3 moveDirection = velocity.normalized;

        float angleOfAttack = Vector3.Dot(transform.up, -moveDirection);

        float aerodynamicFactor = Mathf.Max(0, angleOfAttack);

        float dynamicLift = speed * speed * _liftStrength * aerodynamicFactor;

        dynamicLift = Mathf.Clamp(dynamicLift, 0, 15f); 

        Vector3 liftForce = transform.up * dynamicLift;

        _rigidbody.AddForce(liftForce);

        if (aerodynamicFactor > 0)
        {
            _rigidbody.AddForce(-velocity.normalized * (dynamicLift * 0.1f));
        }
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
    /// <item><description>Sets <see cref="_touchedGround"/> flag to enable state transition when speed drops</description></item>
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
    /// Handles frisbee bouncing off the ground by resetting flight physics.
    /// </summary>
    /// <param name="other">The collision data associated with this collision exit event.</param>
    /// <remarks>
    /// When leaving collision with objects tagged "Ground", restores low angular damping (0.1) so the frisbee spins freely
    /// again and can keep flying after a bounce if it has enough speed.
    /// <see cref="_touchedGround"/> is <b>not</b> cleared, so once the frisbee has touched the ground it lands as soon as
    /// its speed drops below <see cref="StopThreshold"/>, even mid-bounce.
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