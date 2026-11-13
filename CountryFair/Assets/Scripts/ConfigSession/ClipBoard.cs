using UnityEngine;

/// <summary>
/// Clipboard of the session configuration scene. It can be grabbed and thrown; once it has been released,
/// the first collision sends it back to its original pose.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ClipBoard : MonoBehaviour
{
   /// <summary>Renderer that displays the stroke survivor's file in its first material slot (see <see cref="SetFileMaterial"/>).</summary>
   [SerializeField]
   private Renderer _fileRenderer;

   /// <summary>World position at startup, restored after a throw.</summary>
   private Vector3 _originalPosition;

   /// <summary>World rotation at startup, restored after a throw.</summary>
   private Quaternion _originalRotation;

   /// <summary>Cached rigidbody, used to clear the throw velocity when resetting.</summary>
   private Rigidbody _rigidbody;

   /// <summary>True between the release of a grab and the next collision.</summary>
   private bool _wasThrown = false;

   /// <summary>Caches the rigidbody and records the starting pose.</summary>
   private void Awake()
   {   
      if (!Utils.RequireAssigned(_fileRenderer, nameof(_fileRenderer), this))
      {
         return;
      }

      _rigidbody = GetComponent<Rigidbody>();

      _originalPosition = transform.position;
      _originalRotation = transform.rotation;
   }

   /// <summary>Shows a stroke survivor's file on the clipboard.</summary>
   /// <param name="fileMaterial">Material with the survivor's file (<see cref="StrokeSurvivorProfile.fileMaterial"/>).</param>
   public void SetFileMaterial(Material fileMaterial)
   {
      // Only the first slot holds the file; the renderer has other materials that must be kept.
      Material[] materials = _fileRenderer.sharedMaterials;
      materials[0] = fileMaterial;
      _fileRenderer.sharedMaterials = materials;
   }

   /// <summary>Tracks the grab state so only a collision after a release resets the clipboard.</summary>
   /// <param name="isGrabbed">True when the grab begins; false when the player releases the clipboard.</param>
   /// <remarks>Invoked via Inspector by the select/unselect events of the clipboard's grab interactable.</remarks>
   public void HandleGrab(bool isGrabbed)
   {
      _wasThrown = !isGrabbed;
   }

   /// <summary>Returns the clipboard to its original pose on the first collision after being thrown.</summary>
   private void OnCollisionEnter(Collision collision)
   {
      if (!_wasThrown)
      {
         return;
      }

      ResetToOriginalPose();
   }

   /// <summary>Stops the rigidbody and teleports it back to the pose recorded in <see cref="Awake"/>.</summary>
   private void ResetToOriginalPose()
   {
      _wasThrown = false;

      if (!_rigidbody.isKinematic)
      {
         _rigidbody.linearVelocity = Vector3.zero;
         _rigidbody.angularVelocity = Vector3.zero;
      }

      _rigidbody.position = _originalPosition;
      _rigidbody.rotation = _originalRotation;
      transform.SetPositionAndRotation(_originalPosition, _originalRotation);
   }
}
