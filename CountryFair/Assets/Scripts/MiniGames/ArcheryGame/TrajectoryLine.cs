using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int points = 50;
    public float timeStep = 0.05f;
    public float gravityScale = 1f;

    public void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        lineRenderer.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVelocity * t;
            point.y += Physics.gravity.y * gravityScale * t * t / 2f;

            lineRenderer.SetPosition(i, point);
        }

        lineRenderer.enabled = true;
    }

    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }
}
