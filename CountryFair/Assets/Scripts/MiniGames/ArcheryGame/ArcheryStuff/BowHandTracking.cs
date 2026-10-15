using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Implements hand-tracking bow mechanics for the Archery mini-game.
/// Detects pinch gestures (or right-controller trigger as fallback) to grab, pull, and release the arrow;
/// deforms the bow string via a three-point <see cref="LineRenderer"/>; and shows a parabolic trajectory
/// preview while the string is drawn back.
/// </summary>
/// <remarks>
/// Marked <c>[ExecuteAlways]</c> so the bow string is drawn in the Editor too; <see cref="Start"/> and
/// <see cref="Update"/> therefore also run outside Play Mode.
/// </remarks>
[ExecuteAlways]
public class BowHandTracking : MonoBehaviour
{
    /// <summary>Fired every time the player grabs the string at the grab point; wired to <c>CarnyWise.StartTaskTimer</c>.</summary>
    /// <remarks>Also fires again after a cancelled pull; this is harmless because the timer ignores calls while it is already running.</remarks>
    [SerializeField]
    private UnityEvent _playerStartedPulling;

    /// <summary>
    /// Reference frame of the bow: its <c>forward</c> axis is both the shooting direction and the axis the pull is measured
    /// along, and the string anchors are expressed in its local space. Optional; defaults to this transform.
    /// </summary>
    [Header("References")]
    [SerializeField]
    private Transform _bowRoot;

    /// <summary>Bow mesh; its <see cref="MeshRenderer"/> is faded while aiming so the bow does not hide the target. Required.</summary>
    [SerializeField]
    private Transform _bowModel;

    /// <summary>The single reusable arrow; its <see cref="Arrow.ReadyToLaunch"/>/<see cref="Arrow.InAir"/> flags gate the grab → pull → release cycle.</summary>
    [SerializeField]
    private Arrow _arrow;

    /// <summary>Trajectory visualizer shown when <see cref="Arrow.ReadyToLaunch"/> is true.</summary>
    [SerializeField]
    private TrajectoryLine _trajectoryLine;

    /// <summary>
    /// <see cref="OVRHand"/> of the right (pulling) hand. Read for pinch strength while it is tracked; when it is not,
    /// input falls back to the right Touch controller. Required (not null-checked).
    /// </summary>
    [Header("Right Hand Override")]
    [SerializeField]
    private OVRHand _pullingHand;

    /// <summary>LineRenderer for the string; its point count is forced to 3 (top, mid, bottom) in <see cref="Start"/> and redrawn every frame.</summary>
    [Header("Line Renderer / String")]
    [SerializeField]
    private LineRenderer _bowString;

    /// <summary>
    /// Midpoint of the string, moved back along the bow's axis as the pull grows; the arrow is placed relative to it.
    /// Optional: if empty, a <c>StringMidPoint_Runtime</c> child of <see cref="_bowRoot"/> is created in <see cref="Start"/>.
    /// </summary>
    [SerializeField]
    private Transform _stringMidPoint;

    /// <summary>Top end of the string, in <see cref="_bowRoot"/> local space (scaled by its transform).</summary>
    [SerializeField]
    private Vector3 _topLocalPos = new (0f, 0.15f, 0f);

    /// <summary>Bottom end of the string, in <see cref="_bowRoot"/> local space. The string's rest midpoint is halfway between both anchors.</summary>
    [SerializeField]
    private Vector3 _bottomLocalPos = new (0f, -0.15f, 0f);

    /// <summary>
    /// Hand distance (metres, world space) behind the string's rest midpoint, measured along the bow's forward axis, that
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
    /// Responsiveness of the pull to hand movement. Used as a frame-rate-independent exponential filter with rate
    /// <c>_pullSmooth × 30</c> per second (0.2 → ~63 % of the way in ≈0.17 s). Higher is snappier; 0 freezes the pull at 0,
    /// so every shot would use <see cref="_minForce"/>.
    /// </summary>
    [Range(0f, 1f)]
     [SerializeField]
     private float _pullSmooth = 0.2f;

    /// <summary>
    /// Arrow launch speed at zero pull. Applied with <see cref="ForceMode.VelocityChange"/>, so it is effectively a speed
    /// in m/s, independent of the arrow's mass. The force is interpolated linearly between this and <see cref="_maxForce"/>.
    /// </summary>
    [Header("Force")]
    [SerializeField]
    private float _minForce = 5f;
    /// <summary>Arrow launch speed (m/s, via <see cref="ForceMode.VelocityChange"/>) at full pull.</summary>
    [SerializeField]
    private float _maxForce = 60f;

    /// <summary>
    /// Pinch strength (0–1) above which the hand counts as closed; <b>any one</b> of the index, middle or ring pinches is
    /// enough. Only used with hand tracking; the controller fallback has a fixed 0.2 threshold.
    /// </summary>
    [Header("Finger Detection")]
    [SerializeField]
    private float _closeThreshold = 0.25f;
    /// <summary>
    /// Pinch strength (0–1) below which the hand counts as open and the arrow is released; <b>all three</b> pinches must be
    /// below it. Keep it lower than <see cref="_closeThreshold"/>: the gap is the hysteresis band against early releases.
    /// Only used with hand tracking; the controller fallback has a fixed 0.1 threshold.
    /// </summary>
    [SerializeField]
    private float _openThreshold = 0.10f;

    /// <summary>Point on the bow the hand must be near, while closed, to start a pull. Required.</summary>
    [Header("Arrow Grab Detection")]
    [SerializeField]
    private Transform _arrowGrabPoint;
    /// <summary>
    /// Radius (metres, world space) around <see cref="_arrowGrabPoint"/> within which a closed hand starts a pull.
    /// Measured from the right controller anchor (<see cref="_handSource"/>), not from the fingertips, so place the grab
    /// point with that offset in mind.
    /// </summary>
    [SerializeField]
    private float _grabRadius = 0.05f;

    /// <summary>Fired on arrow release with the shot sound effect and the bow GameObject as the spatial audio source.</summary>
    public UnityEvent<AudioManager.GameSoundEffects, GameObject> ArrowShot;

    /// <summary>Sound effect sent through <see cref="ArrowShot"/> on every release (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _shootSoundEffect = AudioManager.GameSoundEffects.ArrowShot;
    

    /// <summary>Right controller anchor of the <see cref="OVRCameraRig"/>; its position drives the pull (it also follows the tracked hand).</summary>
    private Transform _handSource;    


    /// <summary>Smoothed pull amount (0 = rest, 1 = full draw) that maps to string offset and launch force.</summary>
    private float _currentPull = 0f;
  
    /// <summary>Rest position of the string midpoint in <see cref="_bowRoot"/> local space (halfway between the anchors).</summary>
    private Vector3 _stringMidStartLocalPos;
    /// <summary>World-space rest position of the midpoint; the origin the hand's backward distance is measured from.</summary>
    private Vector3 _stringMidStartWorldPos;

    /// <summary>Predicted launch direction (the bow's forward axis), computed in <see cref="UpdatePull"/> only for the trajectory preview.</summary>
    private Vector3 _shootDirection;
    /// <summary>Predicted launch speed for the trajectory preview; <see cref="FireArrow"/> recomputes the real one with the same formula.</summary>
    private float _shootForce;
    /// <summary>Renderer of <see cref="_bowModel"/> whose shared material alpha is changed while aiming.</summary>
    private MeshRenderer _bowRenderer;
    /// <summary>Bow material color at start; its alpha is restored when the player is not aiming.</summary>
    private Color _originalColor;

    /// <summary>
    /// Resolves the hand source, creates a runtime string midpoint if none was assigned, places it between the
    /// string anchors, and caches the bow's original color.
    /// </summary>
    /// <remarks>
    /// The hand source is essential: without it nothing can be grabbed. If the rig does not exist yet, it is retried
    /// every frame from <see cref="Update"/>.
    /// </remarks>
    private void Start()
    {
        if (_bowRoot == null)
        {
             _bowRoot = transform;
        }
           
        AssignRightHandSource();

        if (_stringMidPoint == null)
        {
            GameObject go = new("StringMidPoint_Runtime")
            {
                hideFlags = HideFlags.DontSaveInBuild
            };

            go.transform.SetParent(_bowRoot, false);
            go.transform.localPosition = Vector3.zero;
            _stringMidPoint = go.transform;
  
        }

        _stringMidStartLocalPos = (_topLocalPos + _bottomLocalPos) * 0.5f;
        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = _stringMidPoint.position;

        if (_bowString != null)
        {
            _bowString.positionCount = 3;
        }
        
        _bowRenderer = _bowModel.GetComponent<MeshRenderer>();

        if (_bowRenderer != null)
        {
            _originalColor = _bowRenderer.sharedMaterial.color;  
        }

    }

    /// <summary>Uses the rig's right controller anchor as the hand source; retried every frame from <see cref="Update"/> until the rig exists.</summary>
    private void AssignRightHandSource()
    {
        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();

        if (rig != null)
        {
            _handSource = rig.rightControllerAnchor;
        }
    }

    /// <summary>
    /// Per frame: shows the trajectory and fades the bow while aiming, cancels the pull if the hand is lost, then
    /// runs the grab → pull → release cycle and redraws the string.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>While aiming, the bow fades to alpha 0.35 instead of disappearing, so the player still sees where it points.</item>
    /// <item>If the hand leaves the headset's tracking range mid-pull, the pull is cancelled instead of letting the
    /// controller fallback read "open" and fire the arrow with an invalid hand pose.</item>
    /// </list>
    /// </remarks>
    private void Update()
    {
        if (_arrow.ReadyToLaunch)
        {
            Vector3 startVel = _shootDirection * _shootForce;
            _trajectoryLine.ShowTrajectory(_arrow.transform.position, startVel);

            SetBowTransparency(0.35f);
        }
        else
        {
            _trajectoryLine.HideTrajectory();
            SetBowTransparency(_originalColor.a);
        }

        if (_handSource == null)
        {
            AssignRightHandSource();
            return;
        }

        if (_arrow.ReadyToLaunch && !HasValidHandInput())
        {
            CancelPull();
            UpdateBowString();
            return;
        }

        bool handClosed = IsHandClosed();
        bool handOpen = IsHandOpen();

        if (handClosed && !_arrow.InAir && !_arrow.ReadyToLaunch && IsHandAtGrabPoint())
        {
            PrepareArrow();
        }
           
        if (_arrow.ReadyToLaunch)
        {
            UpdatePull();
        }
          
        if (_arrow.ReadyToLaunch && handOpen)
        {
             FireArrow();
        }
           

        UpdateBowString();
    }

    /// <summary>Arms the arrow, resets the string to rest and raises <see cref="_playerStartedPulling"/>.</summary>
    private void PrepareArrow()
    {   
        _arrow.ReadyToLaunch = true;
        _currentPull = 0f;

        _stringMidStartLocalPos = (_topLocalPos + _bottomLocalPos) * 0.5f;
        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _stringMidStartWorldPos = _stringMidPoint.position;

        _playerStartedPulling.Invoke();
    }

    /// <summary>
    /// Projects the hand's backward distance onto the bow's forward axis, smooths it into <see cref="_currentPull"/>,
    /// and moves the string midpoint and the arrow accordingly.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The string's rest position is recomputed every frame: the bow moves with the other hand, so a value cached
    /// at grab time drifts.</item>
    /// <item>The arrow is placed 0.12 m ahead of the string midpoint so it sits on the drawn string.</item>
    /// <item>The preview force uses the same formula as <see cref="FireArrow"/>, so the previewed trajectory matches the
    /// real shot.</item>
    /// </list>
    /// Cancels the pull if the hand position is not finite (see <see cref="IsFinite"/>).
    /// </remarks>
    private void UpdatePull()
    {
        Vector3 handPos = _handSource.position;

        if (!IsFinite(handPos))
        {
            CancelPull();
            return;
        }

        _stringMidStartWorldPos = _bowRoot.TransformPoint(_stringMidStartLocalPos);

        Vector3 pullDir = _bowRoot.forward;

        float rawDist = Vector3.Dot(handPos - _stringMidStartWorldPos, pullDir);

        float backwardDist = Mathf.Max(0f, -rawDist);

        float pullAmount = Mathf.Clamp01(backwardDist / _maxPullDistance);

        _currentPull = Mathf.Lerp(_currentPull, pullAmount,
            1f - Mathf.Exp(-_pullSmooth * 30f * Time.deltaTime));

        Vector3 targetPos = _stringMidStartWorldPos - _bowRoot.forward * (_currentPull * _maxStringBackward);

        _stringMidPoint.position = targetPos;

        Vector3 offset = _bowRoot.forward * 0.12f;
        _arrow.transform.SetPositionAndRotation(
            _stringMidPoint.position + offset,
            _bowRoot.rotation
        );
        
        _shootDirection = _bowRoot.forward;

        _shootForce = Mathf.Lerp(_minForce, _maxForce, _currentPull);
    }


    /// <summary>Launches the arrow with a force interpolated from the current pull, plays the shot sound and resets the string.</summary>
    private void FireArrow()
    {       
        float launchForce = Mathf.Lerp(_minForce, _maxForce, _currentPull);

        _arrow.Launch(launchForce);

        ArrowShot?.Invoke(_shootSoundEffect, gameObject);
        
        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _currentPull = 0f;
    }

    /// <summary>True if the pulling hand is tracked or the right controller is connected.</summary>
    private bool HasValidHandInput()
    {
        return _pullingHand.IsTracked || OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
    }

    /// <summary>Guards against NaN/Infinity hand positions, which would otherwise propagate into the string and arrow transforms.</summary>
    private static bool IsFinite(Vector3 v)
    {
        return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
    }

    /// <summary>Aborts the current pull without firing: resets the string and puts the arrow back on the bow.</summary>
    private void CancelPull()
    {
        _arrow.ReadyToLaunch = false;
        _currentPull = 0f;

        _stringMidPoint.localPosition = _stringMidStartLocalPos;
        _arrow.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>Redraws the three string points (top anchor, midpoint, bottom anchor) in world space.</summary>
    private void UpdateBowString()
    {
        Vector3 topWorld = _bowRoot.TransformPoint(_topLocalPos);
        Vector3 bottomWorld = _bowRoot.TransformPoint(_bottomLocalPos);

        _bowString.SetPosition(0, topWorld);
        _bowString.SetPosition(1, _stringMidPoint.position);
        _bowString.SetPosition(2, bottomWorld);
    }

    /// <summary>True when any of the index/middle/ring pinches exceeds <see cref="_closeThreshold"/>, or the right trigger/grip is pressed past 0.2 when hand tracking is lost.</summary>
    private bool IsHandClosed()
    {
        if (_pullingHand.IsTracked)
        {
            float i = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            float m = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float r = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            return i > _closeThreshold || m > _closeThreshold || r > _closeThreshold;
        }

        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.2f ||
               OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.2f;
    }

    /// <summary>
    /// True when all three pinches are below <see cref="_openThreshold"/> (or trigger and grip below 0.1). The gap
    /// between the open and close thresholds is a hysteresis band so a half-relaxed pinch does not fire early.
    /// </summary>
    private bool IsHandOpen()
    {
        if (_pullingHand.IsTracked)
        {
            float i = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            float m = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            float r = _pullingHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            return i < _openThreshold && m < _openThreshold && r < _openThreshold;
        }

        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) < 0.1f &&
               OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) < 0.1f;
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

    /// <summary>True when the hand source is within <see cref="_grabRadius"/> of <see cref="_arrowGrabPoint"/>.</summary>
    private bool IsHandAtGrabPoint()
    {
        float dist = Vector3.Distance(_handSource.position, _arrowGrabPoint.position);

        return dist <= _grabRadius;
    }
}
