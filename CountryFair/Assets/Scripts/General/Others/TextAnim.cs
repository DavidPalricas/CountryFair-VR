using UnityEngine;
using DG.Tweening;

/// <summary>
/// Animates UI text with a continuous up-and-down floating motion.
/// </summary>
public class TextAnim : MonoBehaviour
{
    /// <summary>Duration in seconds for the text to move up and then down.</summary>
    [Header("Animation Settings")]
    [SerializeField]
    private float animationDuration = 0.5f;

    /// <summary>Distance in units the text moves vertically during the animation.</summary>
    [SerializeField]
    private float moveDistance = 200f;

    /// <summary>Original local position stored on initialization.</summary>
    private Vector3 originalPosition;

    /// <summary>Sequence controlling the animation loop.</summary>
    private Sequence animationSequence;

    /// <summary>
    /// Stores the original position and creates the animation sequence.
    /// </summary>
    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Initializes and plays the animation sequence.
    /// </summary>
    private void Start()
    {
        CreateAnimation();
        animationSequence.Play();
    }

    /// <summary>
    /// Creates the animation sequence with up and down movement using a Yoyo loop.
    /// </summary>
    private void CreateAnimation()
    {
        animationSequence = DOTween.Sequence();

        Ease easeType = Ease.InOutSine;
        
        animationSequence.Append(transform.DOLocalMoveY(originalPosition.y + moveDistance, animationDuration)
            .SetEase(easeType));

        animationSequence.Append(transform.DOLocalMoveY(originalPosition.y, animationDuration)
            .SetEase(easeType));

        animationSequence.SetLoops(-1, LoopType.Yoyo);
    }
}