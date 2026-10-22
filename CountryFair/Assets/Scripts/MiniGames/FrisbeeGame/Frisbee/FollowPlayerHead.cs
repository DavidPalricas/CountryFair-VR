using UnityEngine;

/// <summary>
/// Locks an object to the player's view: every LateUpdate it is placed at a fixed offset from the center eye and turned
/// with it. Used on the Frisbee prefab and on the archery Bow prefab, so both stay in front of the player.
/// Replaces the deleted static <c>DisplayInPlayerFront</c> component.
/// </summary>
/// <remarks>Execution order 1000 guarantees this runs after all VR camera updates.</remarks>
[DefaultExecutionOrder(1000)]
public class FollowPlayerHead : MonoBehaviour
{
    /// <summary>The VR center eye anchor transform (OVRCameraRig.centerEyeAnchor or equivalent).</summary>
    [SerializeField]
    private Transform _centerEyeTransform;

    /// <summary>Distance in front of the player at which the object is placed.</summary>
    [Header("Positioning Settings")]
    [SerializeField]
     private float _distanceFromPlayer = 2.0f;

    /// <summary>Vertical offset relative to eye height (negative = below eye level).</summary>
    [SerializeField]
    private float _heightOffset = -0.5f;

    /// <summary>Lateral offset relative to the eye's right direction.</summary>
    [SerializeField]
    private float _horizontalOffset = 0f;

    /// <summary>
    /// Rotation relative to the head: the authored rotation, captured in <see cref="Awake"/>. Taken as-is rather
    /// than relative to the head's pose at that moment, so it does not depend on where the player happened to be looking.
    /// </summary>
    private Quaternion _rotationOffset = Quaternion.identity;

    /// <summary>Validates the eye reference and snaps the object to its target so it does not slide in on the first frame.</summary>
    private void Awake()
    {
        if (_centerEyeTransform == null)
        {
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");
            enabled = false;
            return;
        }

        _rotationOffset = transform.rotation;

        HandlePosition();
    }

    /// <summary>Repositions the object after the camera rig has updated the head pose this frame.</summary>
    private void LateUpdate()
    {
        HandlePosition();
    }

    /// <summary>
    /// Locks the object to the head: the offset is expressed in the eye's local space and the rotation follows the eye,
    /// so it stays at the same spot in the player's view whichever way they turn or tilt the head.
    /// </summary>
    private void HandlePosition()
    {
        Vector3 offset = new(_horizontalOffset, _heightOffset, _distanceFromPlayer);
        transform.SetPositionAndRotation(_centerEyeTransform.TransformPoint(offset),
            _centerEyeTransform.rotation * _rotationOffset);
    }
}