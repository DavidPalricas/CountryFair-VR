using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Keeps a world-space UI panel positioned in front of the player's center eye every LateUpdate.
/// Replaces the deleted static <c>DisplayInPlayerFront</c> component for the Frisbee scene UI.
/// </summary>
/// <remarks>Execution order 1000 guarantees this runs after all VR camera updates.</remarks>
[DefaultExecutionOrder(1000)]
public class FollowPlayerHead : MonoBehaviour
{
    /// <summary>The VR center eye anchor transform (OVRCameraRig.centerEyeAnchor or equivalent).</summary>
    [SerializeField]
    [FormerlySerializedAs("centerEyeTransform")]
    private Transform _centerEyeTransform;

    /// <summary>Distance in front of the player at which the panel is placed.</summary>
    [Header("Positioning Settings")]
    [SerializeField]
     [FormerlySerializedAs("distanceFromPlayer")]
     private float _distanceFromPlayer = 2.0f;

    /// <summary>Vertical offset relative to eye height (negative = below eye level).</summary>
    [SerializeField]
    [FormerlySerializedAs("heightOffset")]
    private float _heightOffset = -0.5f;

    /// <summary>Lateral offset relative to the eye's right direction.</summary>
    [SerializeField]
    [FormerlySerializedAs("horizontalOffset")]
    private float _horizontalOffset = 0f;
    
    /// <summary>Validates the eye reference and snaps the panel to its target so it does not slide in on the first frame.</summary>
    private void Awake()
    {
           if (_centerEyeTransform == null)
        {
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");
            return;
        }

        transform.SetPositionAndRotation(CalculateBaseTarget(), Quaternion.identity);
    }

    /// <summary>Repositions the panel after the camera rig has updated the head pose this frame.</summary>
    private void LateUpdate()
    {
        HandlePosition();
    }

    private void HandlePosition()
    {
        transform.position = CalculateBaseTarget();
    }

    /// <summary>Eye position offset along the eye's own right/forward/up axes, so the panel follows head pitch and roll too.</summary>
    /// <remarks>Only the position follows the head; the rotation stays at the identity set in <see cref="Awake"/>.</remarks>
    private Vector3 CalculateBaseTarget()
    {
        Vector3 target = _centerEyeTransform.position;
        target += _centerEyeTransform.right * _horizontalOffset;
        target += _centerEyeTransform.forward * _distanceFromPlayer;
        target += _centerEyeTransform.up * _heightOffset;
        return target;
    }
}