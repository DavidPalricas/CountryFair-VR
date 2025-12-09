using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private ScoreAndStreakSystem scoreSystem;

    bool launched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        scoreSystem = GameObject.FindGameObjectWithTag("ScoreSystem").GetComponent<ScoreAndStreakSystem>();
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
