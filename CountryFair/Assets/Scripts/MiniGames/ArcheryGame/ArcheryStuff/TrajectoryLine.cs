using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Renders a parabolic arrow trajectory preview using a <see cref="LineRenderer"/>.
/// Samples <see cref="Points"/> positions along the predicted path, applying scaled gravity to the Y axis.
/// </summary>
public class TrajectoryLine : MonoBehaviour
{
    /// <summary>LineRenderer used to draw the trajectory arc.</summary>
    public LineRenderer lineRenderer;
    /// <summary>Number of sample points along the predicted trajectory.</summary>
    [FormerlySerializedAs("points")]
    public int Points = 50;
    /// <summary>Time interval in seconds between consecutive trajectory samples.</summary>
    [FormerlySerializedAs("timeStep")]
    public float TimeStep = 0.05f;
    /// <summary>Multiplier applied to <see cref="Physics.gravity"/> when computing the arc (1 = real gravity).</summary>
    [FormerlySerializedAs("gravityScale")]
    public float GravityScale = 1f;

    /// <summary>
    /// Computes and displays a parabolic arc for an arrow launched from <paramref name="startPos"/>
    /// with initial velocity <paramref name="startVelocity"/>.
    /// </summary>
    /// <param name="startPos">World-space launch position.</param>
    /// <param name="startVelocity">Initial velocity vector of the arrow at launch.</param>
    public void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        lineRenderer.positionCount = Points;

        for (int i = 0; i < Points; i++)
        {
            float t = i * TimeStep;
            Vector3 point = startPos + startVelocity * t;
            point.y += Physics.gravity.y * GravityScale * t * t / 2f;

            lineRenderer.SetPosition(i, point);
        }

        lineRenderer.enabled = true;
    }

    /// <summary>Disables the LineRenderer, hiding the trajectory preview.</summary>
    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }
}
