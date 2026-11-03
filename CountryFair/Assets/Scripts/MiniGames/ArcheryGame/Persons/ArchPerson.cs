using UnityEngine;
using DG.Tweening;

/// <summary>
/// Archery mini-game NPC animated entirely with DOTween (no Animator clips).
/// It performs a jump with a full 360° Y-axis spin when <see cref="Jump"/> is called (used by <see cref="Crowd"/>
/// to celebrate player scores), and falls over and gets back up when hit by an arrow.
/// </summary>
/// <remarks>
/// Hits are detected in <see cref="OnTriggerEnter"/> against objects tagged <c>Arrow</c>, so either the attached
/// <see cref="Collider"/> or the arrow's collider must be a trigger.
/// </remarks>

[RequireComponent(typeof(Collider))]
public class ArchPerson : MonoBehaviour
{
    /// <summary>Height of the jump arc.</summary>
    [Header("Jump Settings")]
    [SerializeField]
     private float _jumpPower = 2f;
    
    /// <summary>Total duration of the jump-and-spin sequence in seconds.</summary>
    [SerializeField]
    private float _jumpDuration = 0.5f;

    /// <summary>Angle in degrees the person tips over when hit (90 = lying flat).</summary>
    [Header("Hit Settings")]
    [SerializeField]
    private float _fallAngle = 80f;

    /// <summary>Distance the person slides away from the arrow while falling.</summary>
    [SerializeField]
    private float _knockbackDistance = 0.4f;

    /// <summary>Height of the small hop backwards on impact.</summary>
    [SerializeField]
    private float _knockbackHopPower = 0.3f;

    /// <summary>Duration of the squash punch at the moment of impact.</summary>
    [SerializeField]
    private float _impactDuration = 0.15f;

    /// <summary>Duration of the fall, including the bounces on the floor.</summary>
    [SerializeField]
    private float _fallDuration = 0.6f;

    /// <summary>Time the person stays down before getting up.</summary>
    [SerializeField]
    private float _timeOnGround = 1f;

    /// <summary>Duration of the wobble on the floor right before getting up.</summary>
    [Header("Get Up Settings")]
    [SerializeField]
    private float _getUpWobbleDuration = 0.4f;

    /// <summary>Strength (in degrees) of the wobble on the floor.</summary>
    [SerializeField]
    private float _getUpWobbleStrength = 8f;

    /// <summary>Duration of the springy motion back to the standing pose.</summary>
    [SerializeField]
    private float _getUpDuration = 0.6f;

    /// <summary>Height of the small hop made while getting up.</summary>
    [SerializeField]
    private float _getUpHopPower = 0.4f;

    /// <summary>Duration of the squash-and-stretch once the person is standing again.</summary>
    [SerializeField]
    private float _getUpLandingDuration = 0.3f;

    /// <summary>Current animation phase; gates which reactions (<see cref="Jump"/>, a new hit) are allowed.</summary>
    private ArchPersonState _currentState = ArchPersonState.OnGround;


    /// <summary>Animation phases of the person.</summary>
    private enum ArchPersonState
    {
        /// <summary>Standing still; can jump or be hit.</summary>
        OnGround,

        /// <summary>Celebrating with a jump; can still be hit (the jump is completed instantly first).</summary>
        Jumping,

        /// <summary>Falling or lying on the floor after a hit; further hits are ignored.</summary>
        Hitted,

        /// <summary>Getting back up; can be hit again, which knocks the person down before standing up.</summary>
        GetUp
    }

    /// <summary>Triggers a single jump with a full 360° Y-axis spin if the person is currently on the ground.</summary>
    /// <remarks>This method is called by <see cref="Crowd"/> to celebrate player scores, so only the crowd persons do this.</remarks>
    public void Jump()
    {
        if (_currentState == ArchPersonState.OnGround && !DOTween.IsTweening(transform))
        {
            Sequence sequence = DOTween.Sequence().SetTarget(transform);

            _currentState = ArchPersonState.Jumping;

            const int NumberOfJumps = 1;

            sequence.Append(transform.DOJump(transform.position, _jumpPower, NumberOfJumps, _jumpDuration));

         
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), _jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear));
            
            sequence.OnComplete(() => _currentState = ArchPersonState.OnGround);
        } 
    }



    /// <summary>
    /// Starts the hit reaction when an arrow enters the trigger, unless the person is already falling or lying down.
    /// </summary>
    /// <remarks>
    /// Hits during <see cref="ArchPersonState.GetUp"/> are allowed on purpose, so a player shooting repeatedly at the
    /// same NPC can keep it from getting up.
    /// </remarks>
    /// <param name="other">Collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other) {

        if(other.CompareTag("Arrow") && _currentState != ArchPersonState.Hitted)
        {
            PlayHittedAnimation(other.transform);
        }

    }

    /// <summary>
    /// Wii Bowling pin style reaction: squash on impact, tip over away from the arrow while sliding back,
    /// bounce on the floor and stay down for <see cref="_timeOnGround"/>, then hand over to
    /// <see cref="PlayGetUpAnimation"/>.
    /// </summary>
    /// <remarks>
    /// Any running sequence (jump or get-up) is completed instantly, not killed, so its <c>OnComplete</c> restores
    /// the standing pose. The start pose captured right after it is therefore always the standing one, and repeated
    /// hits do not add up the knockback offsets.
    /// </remarks>
    /// <param name="arrow">Transform of the arrow that hit the person, used to compute the knockback direction.</param>
    private void PlayHittedAnimation(Transform arrow)
    {
        // Completes an ongoing jump so the person is back on the ground before falling.
        transform.DOKill(complete: true);

        // Set after DOKill: completing a jump resets the state to OnGround via its OnComplete.
        _currentState = ArchPersonState.Hitted;

        transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        
        Vector3 startScale = transform.localScale;

        Vector3 hitDirection = Vector3.ProjectOnPlane(startPosition - arrow.position, Vector3.up).normalized;

        if (hitDirection == Vector3.zero)
        {
            hitDirection = Vector3.ProjectOnPlane(arrow.forward, Vector3.up).normalized;
        }

        // Rotating around this axis tips the top of the person towards the hit direction.
        Vector3 tiltAxis = Vector3.Cross(Vector3.up, hitDirection);

        Quaternion fallenRotation = Quaternion.AngleAxis(_fallAngle, tiltAxis) * startRotation;
        Vector3 fallenPosition = startPosition + hitDirection * _knockbackDistance;

        const int NumberOfJumps = 1;
        const int PunchVibrato = 8;
        const float PunchElasticity = 0.5f;

        Sequence sequence = DOTween.Sequence().SetTarget(transform);

        sequence.Append(transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0.2f), _impactDuration, PunchVibrato, PunchElasticity));

        sequence.Append(transform.DOJump(fallenPosition, _knockbackHopPower, NumberOfJumps, _fallDuration));

        sequence.Join(transform.DORotateQuaternion(fallenRotation, _fallDuration).SetEase(Ease.OutBounce));

        sequence.AppendInterval(_timeOnGround);

        sequence.OnComplete(() => PlayGetUpAnimation(startPosition, startRotation, startScale));
    }

    /// <summary>
    /// Get-up reaction: a short wobble while lying down (trying to recover), then the person springs back to
    /// the original pose with a small hop and a squash-and-stretch landing.
    /// </summary>
    /// <param name="startPosition">Position the person had before being hit.</param>
    /// <param name="startRotation">Rotation the person had before being hit.</param>
    /// <param name="startScale">Scale the person had before being hit.</param>
    private void PlayGetUpAnimation(Vector3 startPosition, Quaternion startRotation, Vector3 startScale)
    {
        _currentState = ArchPersonState.GetUp;

        const int NumberOfJumps = 1;
        const int WobbleVibrato = 10;
        const float WobbleRandomness = 20f;
        const int LandingVibrato = 6;
        const float LandingElasticity = 0.6f;

        Sequence sequence = DOTween.Sequence().SetTarget(transform);

        // Struggle on the floor before getting up.
        sequence.Append(transform.DOShakeRotation(_getUpWobbleDuration, _getUpWobbleStrength, WobbleVibrato, WobbleRandomness, fadeOut: false));

        // Spring back to the original pose with a small hop.
        sequence.Append(transform.DOJump(startPosition, _getUpHopPower, NumberOfJumps, _getUpDuration).SetEase(Ease.OutQuad));

        sequence.Join(transform.DORotateQuaternion(startRotation, _getUpDuration).SetEase(Ease.OutBack));

        // Squash-and-stretch on landing.
        sequence.Append(transform.DOPunchScale(new Vector3(-0.15f, 0.2f, -0.15f), _getUpLandingDuration, LandingVibrato, LandingElasticity));

        sequence.OnComplete(() =>
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            transform.localScale = startScale;
            _currentState = ArchPersonState.OnGround;
        });
    }
}