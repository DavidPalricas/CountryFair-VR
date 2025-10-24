using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controls the behavior and animation of a balloon in the fair.
/// Handles rising animation and destruction when reaching maximum height.
/// </summary>
public class Balloon : MonoBehaviour
{
    /// <summary>Maximum height the balloon will rise to before popping.</summary>
    [SerializeField]
    private float maxHeight = 10f;

    /// <summary>Duration in seconds for the balloon to reach maximum height.</summary>
    [SerializeField]
    private float riseDuration = 3f;
 
    /// <summary>Reference to the spawner that created this balloon.</summary>
    [HideInInspector]
    public BalloonsSpawner spawner;

    /// <summary>Tween animation for the rising motion.</summary>
    private Tween riseTween;

    /// <summary>
    /// Initializes the balloon and starts the rising animation on creation.
    /// </summary>
    private void Start()
    {
        RiseAnimation();
    }

    /// <summary>
    /// Creates and plays the tween animation for the balloon rising upward.
    /// Calls PopUp() when the animation completes.
    /// </summary>
    private void RiseAnimation()
    {
        Vector3 targetPosition = transform.position + Vector3.up * maxHeight;

        riseTween = transform.DOMove(targetPosition, riseDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                PopUp();
            });
    }

    /// <summary>
    /// Handles the balloon popping behavior. Notifies the spawner and destroys the balloon object.
    /// </summary>
    private void PopUp()
    {
        if (riseTween != null && riseTween.IsActive())
        {
            riseTween.Kill();
        }

        if (spawner != null)
        {
            spawner.BalloonPopped();
        }
        
        Destroy(gameObject);
    }
}