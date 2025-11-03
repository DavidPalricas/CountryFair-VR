using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the visualization of the frisbee's trailing trajectory during flight.
/// 
/// This component displays a dynamic line that shows the complete flight path of the frisbee.
/// It creates a trailing effect by recording the frisbee's position at distance-based intervals,
/// maintaining a history of all movement during flight.
/// 
/// The trajectory line is rendered as a separate GameObject and can be enabled/disabled
/// independently from the frisbee physics simulation.
/// 
/// The <see cref="ThrowFrisbee"/> class controls when this script is active by toggling its enabled state,
/// enabling trajectory visualization only when the frisbee is in flight.
/// </summary>
public class FrisbeeTrajectory : MonoBehaviour
{
    /// <summary>Trajectory Visualization - Settings for the trajectory line renderer</summary>
    [Header("Trajectory Visualization")]
   
    /// <summary>Color of the trajectory line with alpha transparency.</summary>
    [SerializeField]
    private Color trajectoryColor = new(0f, 1f, 0f, 0.8f);
    
    /// <summary>Width of the trajectory line renderer in world units.</summary>
    [SerializeField]
    private float trajectoryWidth = 0.05f;

    /// <summary>Line renderer component that displays the frisbee trajectory.</summary>
    private LineRenderer _line;

    /// <summary>Last position where a trajectory point was recorded.</summary>
    private Vector3 _lastRecordedPosition;

    /// <summary>List of recorded trajectory points.</summary>
    private List<Vector3> _trajectoryPoints;

    /// <summary>
    /// Initializes the trajectory visualization system on startup.
    /// Creates the LineRenderer and disables this component by default.
    /// </summary>
    private void Awake()
    {
        SetupTrajectoryLine();
        _lastRecordedPosition = transform.position;
        _trajectoryPoints = new List<Vector3>();

        enabled = false;
    }
    
    /// <summary>
    /// Updates the trajectory visualization each frame.
    /// Called while the component is enabled during frisbee flight.
    /// </summary>
    private void Update()
    {
        DrawTrajectory();
    }

    /// <summary>
    /// Creates and configures the LineRenderer component for displaying the trajectory visualization.
    /// Sets up a separate GameObject with appropriate shader, colors, and width properties.
    /// </summary>
    private void SetupTrajectoryLine()
    {
        // Create a separate GameObject for the trajectory line
        GameObject trajectoryObj = new("TrajectoryLine");
        _line = trajectoryObj.AddComponent<LineRenderer>();
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = trajectoryColor;
        _line.endColor = trajectoryColor;
        _line.startWidth = trajectoryWidth;
        _line.endWidth = trajectoryWidth;
        _line.positionCount = 0;
        _line.useWorldSpace = true;
    }

    /// <summary>
    /// Updates the trajectory visualization line by recording the frisbee's position at distance-based intervals.
    /// 
    /// Uses distance-based recording with a fixed threshold (0.1 units) to add a new point only when the frisbee 
    /// has moved a minimum distance. This provides consistent spacing along the trajectory regardless of frame rate.
    /// 
    /// The trajectory maintains all recorded points in a dynamic list, displaying the complete flight path
    /// from the moment of throw until the frisbee stops.
    /// </summary>
    private void DrawTrajectory()
    {
        const float DISTANCE_THRESHOLD = 0.1f;
        
        // Only record a new point if the frisbee has moved far enough
        if (Vector3.Distance(transform.position, _lastRecordedPosition) < DISTANCE_THRESHOLD)
        {
            return;
        }

        // Add new point at the beginning of the list
        _trajectoryPoints.Insert(0, transform.position);
        _lastRecordedPosition = transform.position;

        // Update the line renderer with all points
        _line.positionCount = _trajectoryPoints.Count;
        for (int i = 0; i < _trajectoryPoints.Count; i++)
        {
            _line.SetPosition(i, _trajectoryPoints[i]);
        }
    }
   
    /// <summary>
    /// Clears the trajectory line when this component is disabled.
    /// Called when the component or GameObject is disabled to clean up the visual.
    /// </summary>
    private void OnDisable()
    {
        _line.positionCount = 0;
        _trajectoryPoints.Clear();
    }
}
