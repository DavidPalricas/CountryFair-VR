using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody rb;
    bool launched = false;

    void Awake() => rb = GetComponent<Rigidbody>();

    public void Launch(Vector3 direction, float force)
    {
        launched = true;
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col)
    {
        if (launched)
        {
            rb.isKinematic = true;
            transform.parent = col.transform; // fixa no alvo
        }
    }
}
