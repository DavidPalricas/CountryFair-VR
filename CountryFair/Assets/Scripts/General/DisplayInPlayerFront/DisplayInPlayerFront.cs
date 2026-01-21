using UnityEngine;

public class DisplayInPlayerFront : MonoBehaviour
{
    [SerializeField] 
    protected Transform centerEyeTransform;

    [Header("Positioning Settings")]
    [SerializeField]
     protected float distanceFromPlayer = 2.0f; // Ajustado para valor realista
    [SerializeField] 
    protected float heightOffset = -0.5f;      // Ajustado para valor realista
    [SerializeField] 
    protected float horizontalOffset = 0f;

    protected virtual void Awake()
    {
        if (centerEyeTransform == null)
        {
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");
            return;
        }

        PositionInFrontOfPlayer();
    }

    protected void PositionInFrontOfPlayer()
    {
        Vector3 targetPosition = centerEyeTransform.position - centerEyeTransform.forward.normalized * distanceFromPlayer ;

        targetPosition.y = centerEyeTransform.position.y - heightOffset; 

        targetPosition.x = centerEyeTransform.position.x + horizontalOffset;

        transform.position = targetPosition; 
    }
}