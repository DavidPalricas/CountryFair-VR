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
    private float _animationDuration = 0.5f;

    /// <summary>Distance the text moves vertically, in the parent's local units (canvas units for UI text, hence the large default).</summary>
    [SerializeField]
    private float _moveDistance = 200f;

    /// <summary>Original local position stored on initialization.</summary>
    private Vector3 _originalPosition;

    /// <summary>Sequence controlling the animation loop.</summary>
    private Sequence _animationSequence;

    /// <summary>
    /// Stores the original position the float animation oscillates around.
    /// </summary>
    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Initializes and plays the animation sequence.
    /// </summary>
    private void Start()
    {
        CreateAnimation();
        _animationSequence.Play();
    }

    /// <summary>
    /// Creates the animation sequence with up and down movement using a Yoyo loop.
    /// </summary>
    private void CreateAnimation()
    {
        _animationSequence = DOTween.Sequence();

        Ease easeType = Ease.InOutSine;

        _animationSequence.Append(transform.DOLocalMoveY(_originalPosition.y + _moveDistance, _animationDuration)
            .SetEase(easeType));

        _animationSequence.Append(transform.DOLocalMoveY(_originalPosition.y, _animationDuration)
            .SetEase(easeType));

        _animationSequence.SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>Kills the float sequence and any stray tweens on the text to prevent tweens running on a destroyed transform after a scene change.</summary>
    private void OnDestroy()
    {
        _animationSequence?.Kill();
        transform.DOKill();
    }
}