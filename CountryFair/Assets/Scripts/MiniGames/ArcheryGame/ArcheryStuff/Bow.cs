using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Implements the bow string mechanics for the Archery mini-game.
/// The grab itself is handled by the Meta XR Interaction SDK (a <c>Grabbable</c> on <see cref="_stringHandle"/>, with one
/// hand grab interactable per hand); this script only reads where the handle was pulled to, deforms the bow string via a
/// three-point <see cref="LineRenderer"/>, shows a parabolic trajectory preview and launches the arrow.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GrabString"/> and <see cref="ReleaseString"/> are meant to be wired in the Inspector to the grab
/// interactables' select/unselect events, like the frisbee's <c>OnPlayerFront.ThrowFrisbee</c>.
/// </para>
/// <para>
/// The bow is either locked to the player's view by a <see cref="FollowPlayerHead"/> or parented to one of the hands
/// (see <see cref="SetBowPositioningMode"/>); in every mode this transform moves and turns every frame, which is why the
/// string's rest point is always recomputed from local space instead of cached in world space.
/// </para>
/// <para>
/// Marked <c>[ExecuteAlways]</c> so the bow string is drawn in the Editor too; <see cref="Awake"/> and
/// <see cref="Update"/> therefore also run outside Play Mode.
/// </para>
/// </remarks>
[ExecuteAlways]
public class Bow : MonoBehaviour
{
 
    [Header("Object References")]

    /// <summary>Bow mesh; its <see cref="MeshRenderer"/> is faded while aiming so the bow does not hide the target. Required.</summary>
    [SerializeField]
    private Transform _bowModel;

    /// <summary>The single reusable arrow; its <see cref="Arrow.ReadyToLaunch"/>/<see cref="Arrow.InAir"/> flags gate the grab → pull → release cycle.</summary>
    [SerializeField]
    private Arrow _arrow;

    [Header("References for Bow Positioning")]
    /// <summary>
    /// View-locking component on the bow; enabled only in <see cref="BowPositioningMode.FollowHead"/> and disabled while
    /// the bow is parented to a hand, otherwise it would keep pulling the bow back in front of the eyes. Required.
    /// </summary>
    [SerializeField]
    private FollowPlayerHead _followPlayerHead;

    /// <summary>Parent of the bow in <see cref="BowPositioningMode.LeftHand"/>; the bow is snapped to its origin. Required.</summary>
    [SerializeField]
    private Transform _leftHandTransform;

    /// <summary>Parent of the bow in <see cref="BowPositioningMode.RightHand"/>; the bow is snapped to its origin. Required.</summary>
    [SerializeField]
    private Transform _rightHandTransform;

    /// <summary>
    /// Object the player grabs to draw the string (carries the Meta <c>Grabbable</c>). Its position drives the pull, and
    /// while no hand holds it, it is pinned to the string's rest point every frame. Required.
    /// </summary>
    [SerializeField]
    private Transform _stringHandle;
    
    [Header("Trajectory Settings")]
    /// <summary>Trajectory visualizer shown when <see cref="Arrow.ReadyToLaunch"/> is true.</summary>
    [SerializeField]
    private TrajectoryLine _trajectoryLine;

    /// <summary>Color applied to the trajectory line every frame while aiming.</summary>
    [SerializeField]
    private Color _trajectoryColor = Color.blue;
    

    [Header("Line Renderer Settings")]
  
    /// <summary>LineRenderer for the string; its point count is forced to 3 (top, mid, bottom) in <see cref="Awake"/> and redrawn every frame.</summary>
    [SerializeField]
    private LineRenderer _bowString;

     [Header("String Anchor Settings")]
    /// <summary>
    /// Midpoint of the string, moved back along the bow's axis as the pull grows; the arrow is placed relative to it.
    /// Required: if empty, <see cref="Awake"/> logs an error and stops initializing the bow.
    /// </summary>
    [SerializeField]
    private Transform _stringMidPoint;

    /// <summary>Top end of the string, in this transform's local space (so it is scaled with the bow).</summary>
    [SerializeField]
    private Vector3 _topLocalPos = new (0f, 0.15f, 0f);

    /// <summary>Bottom end of the string, in this transform's local space. The string's rest midpoint is halfway between both anchors.</summary>
    [SerializeField]
    private Vector3 _bottomLocalPos = new (0f, -0.15f, 0f);
    
    [Header("Pull Settings")]
    /// <summary>
    /// Handle distance (metres, world space) behind the string's rest midpoint, measured along the bow's forward axis, that
    /// counts as a full draw (pull = 1). Lower values reach full power with a shorter arm movement.
    /// </summary>
    [Header("Pull Settings")]
    [SerializeField]
    private float _maxPullDistance = 0.35f;

    /// <summary>
    /// How far (metres, world space) the string midpoint is drawn back at full pull. Visual only: it does not affect force,
    /// so it can be smaller than <see cref="_maxPullDistance"/>, in which case the string trails behind the hand.
    /// </summary>
    [SerializeField]
    private float _maxStringBackward = 0.25f;

    /// <summary>
    /// Distance (metres, world space) the arrow is placed ahead of the string midpoint along the bow's forward axis, so its
    /// nock sits on the drawn string. Depends on where the arrow model's pivot is relative to its nock.
    /// </summary>
    [SerializeField]
    private float _arrowOffsetFromString = 0.12f;

    /// <summary>
    /// Responsiveness of the pull to hand movement. Used as a frame-rate-independent exponential filter with rate
    /// <c>_pullSmooth × <see cref="PullSmoothRateScale"/></c> per second (0.2 → ~63 % of the way in ≈0.17 s). Higher is
    /// snappier; 0 freezes the pull at 0, so every shot would use <see cref="_minForce"/>.
    /// </summary>
    [Range(0f, 1f)]
    [SerializeField]
    private float _pullSmooth = 0.2f;

    /// <summary>
    /// Arrow launch speed at zero pull. Applied with <see cref="ForceMode.VelocityChange"/>, so it is effectively a speed
    /// in m/s, independent of the arrow's mass. The force is interpolated linearly between this and <see cref="_maxForce"/>.
    /// </summary>
    [Header("Pull Force Settings")]
    [SerializeField]
    private float _minForce = 5f;

    /// <summary>Arrow launch speed (m/s, via <see cref="ForceMode.VelocityChange"/>) at full pull.</summary>
    [SerializeField]
    private float _maxForce = 60f;

    [Header("Events")]
    /// <summary>Fired on arrow release with the shot sound effect and the bow GameObject as the spatial audio source.</summary>
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> ArrowShot;

    /// <summary>Fired every time the arrow is armed by a grab of the string; wired to <c>CarnyWise.StartTaskTimer</c>.</summary>
    /// <remarks>
    /// Can fire a few frames after the grab itself, when the string was grabbed while the arrow was still in the air
    /// (see <see cref="TryArmArrow"/>). Also fires again after a cancelled pull; this is harmless because the timer
    /// ignores calls while it is already running.
    /// </remarks>
    [SerializeField]
    private UnityEvent _playerStartedPulling;

    /// <summary>Sound effect sent through <see cref="ArrowShot"/> on every release (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _shootSoundEffect = AudioManager.GameSoundEffects.ArrowShot;
     
    /// <summary>
    /// Converts the 0–1 <see cref="_pullSmooth"/> slider into a filter rate per second, so the whole slider range maps to
    /// useful responsiveness (up to 30/s, i.e. ~63 % of the way in ≈33 ms at 1).
    /// </summary>
    private const float PullSmoothRateScale = 30f;

    /// <summary>Smoothed pull amount (0 = rest, 1 = full draw) that maps to string offset and launch force.</summary>
    private float _currentPull = 0f;

    /// <summary>Rest position of the string midpoint in this transform's local space (halfway between the anchors).</summary>
    private Vector3 _stringMidStartLocalPos;
    /// <summary>World-space rest position of the midpoint; the origin the handle's backward distance is measured from.</summary>
    private Vector3 _stringMidStartWorldPos;

    /// <summary>Predicted launch direction (the bow's forward axis), computed in <see cref="UpdatePull"/> only for the trajectory preview.</summary>
    private Vector3 _shootDirection;
    /// <summary>Predicted launch speed for the trajectory preview; <see cref="FireArrow"/> recomputes the real one with the same formula.</summary>
    private float _shootForce;
    /// <summary>Renderer of <see cref="_bowModel"/> whose shared material alpha is changed while aiming.</summary>
    private MeshRenderer _bowRenderer;

    /// <summary>Renderer of <see cref="_trajectoryLine"/>, recolored with <see cref="_trajectoryColor"/> while aiming.</summary>
    private Renderer _trajectoryLineRenderer;

    /// <summary>Bow material color at start; its alpha is restored when the player is not aiming.</summary>
    private Color _originalColor;
    /// <summary>True between <see cref="GrabString"/> and <see cref="ReleaseString"/>, i.e. while a hand holds the string handle.</summary>
    private bool _isHandleGrabbed = false;

    /// <summary>Where the bow is held; switched at runtime by the <c>head</c>/<c>left</c>/<c>right</c> cheat codes.</summary>
    public enum BowPositioningMode
    {
        /// <summary>Locked in front of the player's view by <see cref="FollowPlayerHead"/>; the default at startup.</summary>
        FollowHead,

        /// <summary>Parented to <see cref="_leftHandTransform"/>, so the player aims by moving the left hand.</summary>
        LeftHand,
        /// <summary>Parented to <see cref="_rightHandTransform"/>, so the player aims by moving the right hand.</summary>
        RightHand
    }

    /// <summary>
    /// Validates the references, places the string midpoint between the string anchors, puts the handle on it, caches the
    /// bow's original color and starts in <see cref="BowPositioningMode.FollowHead"/>.
    /// </summary>
    /// <remarks>
    /// Stops at the first missing reference, leaving the bow partially initialized. The final unparenting reproduces
    /// exactly the state <see cref="SetBowPositioningMode"/> sets up for <see cref="BowPositioningMode.FollowHead"/>.
    /// </remarks>
    private void Awake()
    {   
        if (_followPlayerHead == null || _leftHandTransform == null || _rightHandTransform == null)
        {
            Debug.LogError("FollowPlayerHead or hand transforms are not assigned in Bow script.");
            return;
        }


        if (_stringHandle == null)
        {
            Debug.LogError("String handle is not assigned in BowHandTracking script.");

            return;
        }

        if (_stringMidPoint == null)
        {
            Debug.LogError("String midpoint is not assigned in BowHandTracking script. A runtime midpoint will be created in Start().");
            return;      
        }
       
        _stringMidStartLocalPos = (_topLocalPos + _bottomLocalPos) * 0.5f;
        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = _stringMidPoint.position;

        ResetStringHandle();

        if (_bowString == null)
        {
            Debug.LogError("Bow string LineRenderer is not assigned in BowHandTracking script.");

            return;
        }
    
        _bowString.positionCount = 3;
        
        _bowRenderer = _bowModel.GetComponent<MeshRenderer>();

        _trajectoryLineRenderer = _trajectoryLine.GetComponent<Renderer>();

        if (_bowRenderer == null || _trajectoryLine == null)
        {
            Debug.LogError("Bow model or trajectory is missing a Renderer component.");
            return;
        }

        _originalColor = _bowRenderer.sharedMaterial.color;
         
         // Default mode is FollowHead, so the bow is visible in front of the player when the scene starts.
        _followPlayerHead.enabled = true;

        transform.SetParent(null);
    }

    /// <summary>
    /// Per frame: arms the arrow if the string is held, otherwise keeps the handle on the string; shows the trajectory
    /// and fades the bow while aiming, updates the pull and redraws the string.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>While aiming, the bow fades to alpha 0.35 instead of disappearing, so the player still sees where it points.</item>
    /// <item>While the handle is held, arming is retried every frame, so a grab made while the arrow was still in the air
    /// arms it as soon as the arrow is back on the bow.</item>
    /// <item>While the handle is not held, it is pinned to the string every frame, not only on release: the bow follows
    /// the head, and the Grabbable finishes its own release after the unselect event, so a one-off reset could leave
    /// the handle out of reach.</item>
    /// </list>
    /// </remarks>
    private void Update()
    {
        if (_isHandleGrabbed)
        {
            TryArmArrow();
        }
        else 
        {
            ResetStringHandle();
        }

        if (_arrow.ReadyToLaunch)
        {
            UpdatePull();

            Vector3 startVel = _shootDirection * _shootForce;
            _trajectoryLine.ShowTrajectory(_arrow.transform.position, startVel);

            _trajectoryLineRenderer.material.color = _trajectoryColor;

            SetBowTransparency(0.35f);
        }
        else
        {
            _trajectoryLine.HideTrajectory();
            SetBowTransparency(_originalColor.a);
        }

        UpdateBowString();
    }

    /// <summary>Marks the handle as held and tries to arm the arrow (see <see cref="TryArmArrow"/>).</summary>
    /// <remarks>Invoked via Inspector by the string handle's grab interactables when a hand grabs it (either hand).</remarks>
    public void GrabString()
    {
        _isHandleGrabbed = true;

        TryArmArrow();
    }

    /// <summary>
    /// Arms the arrow, resets the string to rest and raises <see cref="_playerStartedPulling"/>. Ignored while the
    /// arrow is in the air or already armed.
    /// </summary>
    /// <remarks>Called on grab and then every frame while the handle is held, see <see cref="Update"/>.</remarks>
    private void TryArmArrow()
    {
        if (_arrow.InAir || _arrow.ReadyToLaunch)
        {
            return;
        }

        _arrow.ReadyToLaunch = true;
        _currentPull = 0f;

        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = _stringMidPoint.position;

        _playerStartedPulling.Invoke();
    }

    /// <summary>Marks the handle as released, fires the arrow if it is armed, then puts the handle back on the string.</summary>
    /// <remarks>Invoked via Inspector by the string handle's grab interactables when the hand lets go.</remarks>
    public void ReleaseString()
    {
        _isHandleGrabbed = false;

        if (_arrow.ReadyToLaunch)
        {
            FireArrow();
        }

        ResetStringHandle();
    }

    /// <summary>Aborts the current pull without firing: resets the string and the handle and puts the arrow back on the bow.</summary>
    /// <remarks>
    /// Currently has no caller, neither in code nor in any scene or prefab: it was used by the old manual hand detection
    /// when tracking was lost mid-pull.
    /// </remarks>
    public void CancelPull()
    {
        _arrow.ReadyToLaunch = false;
        _currentPull = 0f;

        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _arrow.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        ResetStringHandle();
    }

    /// <summary>
    /// Projects the handle's backward distance onto the bow's forward axis, smooths it into <see cref="_currentPull"/>,
    /// and moves the string midpoint and the arrow accordingly.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The string's rest position is recomputed every frame: the bow moves with the player's head, so a value cached
    /// at grab time drifts.</item>
    /// <item>The arrow is placed <see cref="_arrowOffsetFromString"/> ahead of the string midpoint so it sits on the drawn string.</item>
    /// <item>The preview force uses the same formula as <see cref="FireArrow"/>, so the previewed trajectory matches the
    /// real shot.</item>
    /// </list>
    /// </remarks>
    private void UpdatePull()
    {
        _stringMidStartWorldPos = transform.TransformPoint(_stringMidStartLocalPos);

        Vector3 pullDir = transform.forward;

        float rawDist = Vector3.Dot(_stringHandle.position - _stringMidStartWorldPos, pullDir);

        float backwardDist = Mathf.Max(0f, -rawDist);

        float pullAmount = Mathf.Clamp01(backwardDist / _maxPullDistance);

        _currentPull = Mathf.Lerp(_currentPull, pullAmount,
            1f - Mathf.Exp(-_pullSmooth * PullSmoothRateScale * Time.deltaTime));

        Vector3 targetPos = _stringMidStartWorldPos - transform.forward * (_currentPull * _maxStringBackward);

        _stringMidPoint.position = targetPos;

        Vector3 offset = transform.forward * _arrowOffsetFromString;
        _arrow.transform.SetPositionAndRotation(
            _stringMidPoint.position + offset,
            transform.rotation
        );

        _shootDirection = transform.forward;

        _shootForce = Mathf.Lerp(_minForce, _maxForce, _currentPull);
    }

    /// <summary>Launches the arrow with a force interpolated from the current pull, plays the shot sound and resets the string.</summary>
    private void FireArrow()
    {
        float launchForce = Mathf.Lerp(_minForce, _maxForce, _currentPull);

        _arrow.Launch(launchForce);

        ArrowShot.Invoke(_shootSoundEffect, gameObject);

        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _currentPull = 0f;
    }

    /// <summary>
    /// Puts the string handle back on the string's rest midpoint, aligned with the bow, so the next grab starts from the string.
    /// </summary>
    private void ResetStringHandle()
    {
        _stringHandle.SetPositionAndRotation(transform.TransformPoint(_stringMidStartLocalPos), transform.rotation);
    }

    /// <summary>Redraws the three string points (top anchor, midpoint, bottom anchor) in world space.</summary>
    private void UpdateBowString()
    {
        Vector3 topWorld = transform.TransformPoint(_topLocalPos);
        Vector3 bottomWorld = transform.TransformPoint(_bottomLocalPos);

        _bowString.SetPosition(0, topWorld);
        _bowString.SetPosition(1, _stringMidPoint.position);
        _bowString.SetPosition(2, bottomWorld);
    }

    /// <summary>Sets the alpha of the bow material.</summary>
    /// <param name="alpha">0 = invisible, 1 = opaque. Only has a visible effect if the material uses a transparent surface type.</param>
    /// <remarks>Writes to <c>sharedMaterial</c>, so every renderer using the bow material fades too, and the change persists in the Editor.</remarks>
    private void SetBowTransparency(float alpha)
    {
        Color c = _bowRenderer.sharedMaterial.color;
        c.a = alpha;
        _bowRenderer.sharedMaterial.color = c;
    }


    /// <summary>
    /// Moves the bow to the given positioning mode: in front of the head (world-space, driven by
    /// <see cref="FollowPlayerHead"/>) or parented to a hand and snapped to its origin.
    /// </summary>
    /// <param name="mode">Target mode; an unknown value only logs an error.</param>
    /// <remarks>Invoked via Inspector in the ArcheryGame scene, by <c>ArcheryCheatCodes._setBowPositioningMode</c>.</remarks>
    public void SetBowPositioningMode(BowPositioningMode mode)
    {
        switch (mode)
        {
            case BowPositioningMode.FollowHead:
                _followPlayerHead.enabled = true;
                transform.SetParent(null);
                return;

            case BowPositioningMode.LeftHand:
                _followPlayerHead.enabled = false;
                transform.SetParent(_leftHandTransform);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                return;

            case BowPositioningMode.RightHand:
                _followPlayerHead.enabled = false;
                transform.SetParent(_rightHandTransform);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                return;
            default:
                Debug.LogError("Invalid BowPositioningMode: " + mode);
                return;
        }
    }
}
