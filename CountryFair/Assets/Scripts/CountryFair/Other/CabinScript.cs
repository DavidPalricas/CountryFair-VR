using UnityEngine;

public class CabinScript : MonoBehaviour
{
    Transform wheel;

    void Start()
    {
        wheel = transform.parent;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            -wheel.eulerAngles.z
        );
    }
}