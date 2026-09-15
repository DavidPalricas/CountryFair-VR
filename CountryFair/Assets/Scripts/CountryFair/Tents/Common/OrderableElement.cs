using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Base class for a tent-related element (a physical <see cref="MiniGameTent"/> in the world or a
/// <see cref="TentPanel"/> in the wrist menu) that can be grabbed and reordered between
/// <see cref="TentPlaceHolder"/> slots. Handles the shared placeholder-tracking and snap logic;
/// grab handling and slot-specific visuals are supplied by derived classes.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OrderableTentElement : MonoBehaviour
{
     /// <summary>
    /// Raised when the tent grab state changes.
    /// Passes <c>true</c> when grabbed, <c>false</c> when released, along with this tent instance.
    /// </summary>
    [SerializeField]
    protected UnityEvent<bool, OrderableTentElement> OnElementSelectionChanged;

    /// <summary>The placeholder slot this tent currently occupies; determines snap position and badge number.</summary>
    [SerializeField]
    protected TentPlaceHolder currentPlaceHolder;

    /// <summary>Cached collider used to identify this element as the ray/hover target (e.g. in <see cref="MiniGameTent"/>).</summary>
    protected Collider _collider  = null;

    /// <summary>Last valid placeholder occupied before the current drag, used to revert when the element exits every trigger zone.</summary>
    protected TentPlaceHolder _previousPlaceHolder;

    /// <summary>Which mini-game this element represents; drives the tent's scene target and the wrist-menu panel's label/sprite.</summary>
    public MINI_GAMES miniGame = MINI_GAMES.ARCHERY;

    /// <summary>Mini-games available for personalization. Only <see cref="ARCHERY"/> and <see cref="FRISBEE"/> currently have a scene wired up.</summary>
    public enum MINI_GAMES
    {
        ARCHERY,
        DUCKGAME,
        FISHING,
        FRISBEE,
    }

    /// <summary>Validates the starting placeholder reference, caches this element's collider, and records the starting placeholder as the initial fallback for reverts.</summary>
    protected virtual void Awake()
    {
        if (currentPlaceHolder == null)
        {
            Debug.LogError("Current Tent PlaceHolder reference is null in OrderableElement");

            return;
        }

        _collider = GetComponent<Collider>();

        _previousPlaceHolder = currentPlaceHolder;
    }


    /// <summary>
    /// Waits one physics step after release before snapping back to the current placeholder,
    /// so the pending throw velocity write from the Grabbable/Interactable isn't overridden mid-write.
    /// </summary>
    protected IEnumerator SnapToPlaceHolderNextFixedUpdate()
    {
        // Wait one fixed step so the Grabbable/Interactable finishes its release logic
        // (which writes velocity to the Rigidbody for throw inertia).
        yield return new WaitForFixedUpdate();

        SnapToCurrentPlaceHolder();

        OnElementSelectionChanged.Invoke(false, this);
    }

    
    /// <summary>Returns the placeholder slot this tent currently occupies.</summary>
    public TentPlaceHolder GetCurrentPlaceHolder()
    {
        return currentPlaceHolder;
    }

    
    /// <summary>
    /// Sets the target placeholder for this tent.
    /// If <paramref name="newPlaceHolder"/> is null, reverts to the last valid placeholder —
    /// covers the case where the tent exits all trigger zones mid-drag.
    /// Called by <see cref="TentPlaceHolder"/> trigger enter/exit callbacks.
    /// </summary>
    public void UpdateTentPlaceHolder(TentPlaceHolder newPlaceHolder)
    {
        if (newPlaceHolder == null){
            currentPlaceHolder = _previousPlaceHolder;

            return;
        }

        currentPlaceHolder = newPlaceHolder;
    }

        /// <summary>
    /// Responds to a grab start or release event, toggling between selected and unselected states.
    /// </summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the tent.</param>
    /// <remarks>Invocado via Inspector nos eventos OnSelectEntered/OnSelectExited do componente XR Grab Interactable.</remarks>
    public virtual void HandleGrab(bool isGrabbed)
    {
        Debug.LogError("HandleGrab method must be implemented in a derived class.");
    }

     /// <summary>
    /// Teleports this element to the current placeholder's position and rotation.
    /// Derived classes (<see cref="MiniGameTent"/>, <see cref="TentPanel"/>) override this to additionally
    /// refresh their number badge and other slot-specific visuals.
    /// </summary>
    public virtual void SnapToCurrentPlaceHolder()
    {
        Transform PlaceHolderTransform = currentPlaceHolder.transform;

        transform.SetPositionAndRotation(PlaceHolderTransform.position, PlaceHolderTransform.rotation);
    }
}