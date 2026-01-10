using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    private Rigidbody _rb;
    private ScoreAndStreakSystem scoreSystem;

    public bool ReadyToLaunch  { get; set; } = false;

    private Crowd _crowd;


    private Transform parentTransform = null;

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
            scoreSystem.PlayerMissed();
            SetArrowToOrginalPosition();
            return;
        }
    }


    private void SetArrowToOrginalPosition()
    {  
        _rb.isKinematic = true;

      
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.parent = parentTransform;
    }
}
