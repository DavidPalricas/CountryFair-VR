using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private ScoreAndStreakSystem scoreSystem;

    bool launched = false;

    private Crowd _crowd;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        scoreSystem = GameObject.FindGameObjectWithTag("ScoreSystem").GetComponent<ScoreAndStreakSystem>();

        _crowd = GameObject.FindGameObjectWithTag("CrowdArcheryGame").GetComponent<Crowd>();

        if (_crowd == null)
        {
            Debug.LogError("Crowd GameObject not found in the scene or its crowd component is missing.");
        }
    }

    public void Launch(Vector3 direction, float force)
    {
        launched = true;
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider col)
    {
        // --- SE BATER NO BALÃO ---
        if (col.gameObject.CompareTag("Balloon"))
        {
            col.gameObject.GetComponent<BalloonScript>().Pop();
            _crowd.Cheer();
            Destroy(gameObject);
            return;
        }

        // --- SE BATER NO CHÃO ---
        if (col.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            scoreSystem.PlayerMissed();
            return;
        }

    }
}
