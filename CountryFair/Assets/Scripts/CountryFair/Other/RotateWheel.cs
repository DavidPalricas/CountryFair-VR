using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        transform.Rotate(Vector3.right * speed * Time.deltaTime);
    }
}
