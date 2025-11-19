using UnityEngine;

/// <summary>
/// Base class for all frisbee states in the Finite State Machine.
/// Provides common references and initialization for frisbee physics and visualization components.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(FrisbeeTrajectory))]
public class FrisbeeState : State
{   
    /// <summary>Reference to the rigidbody component for physics calculations.</summary>
    protected Rigidbody _rigidbody;
    
    /// <summary>Reference to the collider component for enabling/disabling collisions.</summary>
    protected Collider _collider;
    
    /// <summary>Reference to the trajectory visualization component.</summary>
    protected FrisbeeTrajectory _trajectoryLine;

    /// <summary>Current angle of attack in radians - updated during flight based on disc orientation.</summary>
    protected float _currentAlpha;

    /// <summary>
    /// Initializes the frisbee state by setting up references to rigidbody, collider, and trajectory line renderer and setting state properties(fsm and state name).
    /// Called when the state component is first created.
    /// </summary>
    protected virtual void Awake()
    {   
        SetStateProprieties();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _trajectoryLine = GetComponent<FrisbeeTrajectory>();
    }
}