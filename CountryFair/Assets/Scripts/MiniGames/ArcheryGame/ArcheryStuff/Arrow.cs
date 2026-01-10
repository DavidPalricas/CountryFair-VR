using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    private Rigidbody _rb;
    private ScoreAndStreakSystem scoreSystem;

    public bool ReadyToLaunch  { get; set; } = false;

    private Crowd _crowd;


    private Transform parentTransform = null;

    private Vector3 originalPosition = Vector3.zero;

    private Quaternion originalRotation = Quaternion.identity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        scoreSystem = GameObject.FindGameObjectWithTag("ScoreSystem").GetComponent<ScoreAndStreakSystem>();

        _crowd = GameObject.FindGameObjectWithTag("CrowdArcheryGame").GetComponent<Crowd>();

        if (_crowd == null)
        {
            Debug.LogError("Crowd GameObject not found in the scene or its crowd component is missing.");
        }

        _rb.isKinematic = true;
    }

    public void Launch(float launchForce)
    {
        ReadyToLaunch = false;

        parentTransform = transform.parent;

        originalPosition = transform.localPosition;

        originalRotation = transform.rotation;

        _rb.isKinematic = false;

        transform.parent = null;

        _rb.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider col)
    {
        // --- SE BATER NO BALÃO ---
        if (col.gameObject.CompareTag("Balloon"))
        {
            col.gameObject.GetComponent<BalloonScript>().Pop();
            _crowd.Cheer();
            SetArrowToOrginalPosition();
            return;
        }

        // --- SE BATER NO CHÃO ---
        if (col.gameObject.CompareTag("Ground"))
        {
            SetArrowToOrginalPosition();
            scoreSystem.PlayerMissed();
            return;
        }
    }


    private void SetArrowToOrginalPosition()
    {  
        _rb.isKinematic = true;

        transform.parent = parentTransform;
        transform.localPosition = originalPosition;
        _rb.rotation = originalRotation;
    }
}
