using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Continuously rotates the GameObject around its local X axis to simulate a Ferris wheel.
/// </summary>
public class RotateWheel : MonoBehaviour
{
    /// <summary>Rotation speed in degrees per second.</summary>
    [FormerlySerializedAs("speed")]
    public float Speed = 20f;

    void Update()
    {
        transform.Rotate(Vector3.right * Speed * Time.deltaTime);
    }
}
