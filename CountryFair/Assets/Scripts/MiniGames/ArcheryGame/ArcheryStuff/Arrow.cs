using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Physical arrow projectile for the Archery mini-game. Handles launch physics, trigger detection for
/// balloon hits and ground/out-of-bounds misses, and fires <c>_playerScored</c> or <c>_playerMissed</c>
/// events accordingly. Resets to its bow-attachment position after each shot.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    /// <summary>Tag of the crowd object whose <see cref="Crowd"/> cheers on every scoring hit.</summary>
    private const string CrowdTag = "CrowdArcheryGame";

    /// <summary>Kinematic while the arrow rests on the bow; switched to dynamic only for the flight in <see cref="Launch"/>.</summary>
    private Rigidbody _rb;

    /// <summary>Set by <see cref="BowHandTracking"/> when the player grabs the string; allows the arrow to be fired.</summary>
    [HideInInspector]
    public bool ReadyToLaunch = false;

    /// <summary>While true (tutorial mode), hitting a balloon pops it but fires neither scoring nor miss events.</summary>
    /// <remarks>Set to false via Inspector by the tutorial's <c>_tutorialCompleted</c> event (ArcheryGame scene).</remarks>
    public bool IsTutorialActive{get; set;} = true;
    /// <summary>True while the arrow is in flight; prevents re-grabbing before it lands.</summary>
    public bool InAir { get ; private set; } = false;

    /// <summary>Crowd that cheers on every scoring hit; found by the <c>CrowdArcheryGame</c> tag.</summary>
    private Crowd _crowd;

    /// <summary>Bow attachment the arrow is re-parented to after each shot; cached in <see cref="Launch"/> before detaching.</summary>
    private Transform _parentTransform = null;

    /// <summary>Fired with the balloon score value when the arrow hits the scoring-color balloon outside tutorial mode.</summary>
    [SerializeField]
    private UnityEvent <int> _playerScored;

    /// <summary>Fired when the arrow hits a non-scoring balloon, the ground, or goes out of bounds.</summary>
    [SerializeField]
    private UnityEvent _playerMissed;


    /// <summary>Caches the Rigidbody and the scene <see cref="Crowd"/>, and starts kinematic so the arrow follows the bow.</summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    
        _crowd = GameObject.FindGameObjectWithTag(CrowdTag).GetComponent<Crowd>();

        if (_crowd == null)
        {
            Debug.LogError("Crowd GameObject not found in the scene or its crowd component is missing.");
        }

        _rb.isKinematic = true;
    }

    /// <summary>
    /// Detaches the arrow from the bow, enables physics, and applies <paramref name="launchForce"/> in the forward direction.
    /// </summary>
    /// <param name="launchForce">Force magnitude via <see cref="ForceMode.VelocityChange"/>; pass 0 for a drop-miss cheat.</param>
    /// <param name="targetPosition">Optional world position to move the arrow to after launching; <c>null</c> keeps the current position.</param>
    /// <remarks>
    /// Called by <see cref="BowHandTracking"/> on string release, and invoked via Inspector (ArcheryGame scene) by
    /// <c>ArcheryCheatCodes._launchArrow</c> for the <c>miss</c>/<c>score</c> cheats.
    /// </remarks>
    public void Launch(float launchForce, Vector3? targetPosition = null)
    {
        ReadyToLaunch = false;
        InAir = true;

        _parentTransform = transform.parent;


        _rb.isKinematic = false;

        transform.parent = null;

        _rb.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);

        if (targetPosition.HasValue)
        {
            transform.position = targetPosition.Value;
        }
    }

    /// <summary>
    /// Resolves the shot: a <c>Balloon</c> hit pops it and scores or misses depending on its color (no events during
    /// the tutorial); a <c>Ground</c> or <c>OutOfBounds</c> hit is a miss. Either way the arrow returns to the bow.
    /// </summary>
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(BalloonArcheryGame.BalloonTag))
        {   
            BalloonArcheryGame balloon = col.gameObject.GetComponent<BalloonArcheryGame>();

            // Read before Pop: the balloon goes back to the ObjectPool and can be reused (and reset) as its own replacement.
            int scoreValue = balloon.GetScoreValue();

            balloon.Pop();

            SetArrowToOrginalPosition();


            if (!IsTutorialActive)
            {
                if (scoreValue > 0)
                {
                    _playerScored.Invoke(scoreValue);

                    _crowd.Cheer();

                    return;
                }
               
                _playerMissed.Invoke();

                return;   
            }

            return;
        }

        if (col.gameObject.CompareTag(GameManager.GroundTag) || col.gameObject.CompareTag(GameManager.OutOfBoundsTag))
        {
            _playerMissed.Invoke();
            SetArrowToOrginalPosition();
            return;
        }
    }


    /// <summary>Stops the physics and snaps the arrow back onto the bow attachment so it can be grabbed again.</summary>
    private void SetArrowToOrginalPosition()
    {  
        _rb.isKinematic = true;
        InAir = false;
       
        transform.parent = _parentTransform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
