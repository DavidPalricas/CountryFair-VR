using UnityEngine;
using DG.Tweening;

/// <summary>
/// Animates a sheep with a continuous bouncing motion including squashing and stretching effects.
/// </summary>
public class SheepAnimation : MonoBehaviour
{
    /// <summary>Maximum height the sheep bounces upward.</summary>
    [Header("Animation Settings")]
    [SerializeField]
    private float _bounceHeight = 0.5f;

    /// <summary>Duration in seconds for one complete bounce cycle.</summary>
    [SerializeField]
    private  float _bounceDuration = 0.5f;
    
    /// <summary>Amount of squashing and stretching deformation as a ratio of original scale.</summary>
    [SerializeField]
    private float _squashAmount = 0.3f;

    /// <summary>Original scale of the sheep stored on Start.</summary>
    private Vector3 _originalScale;

    /// <summary>Sequence controlling the bounce animation.</summary>
    private Sequence _bounceSequence;

    /// <summary>
    /// Stores the original scale and initiates the bounce animation.
    /// </summary>
    private void Start()
    {
        _originalScale = transform.localScale;
        StartBounceAnimation();
    }

    /// <summary>
    /// Creates and configures the bounce animation sequence with up/down motion and squash/stretch effects.
    /// Loops infinitely with a restart loop type.
    /// </summary>
    private void StartBounceAnimation()
    {
        _bounceSequence = DOTween.Sequence();
        
        _bounceSequence.Append(transform.DOMoveY(transform.position.y + _bounceHeight, _bounceDuration / 2)
            .SetEase(Ease.OutQuad));

        _bounceSequence.Join(transform.DOScale(new Vector3(
            _originalScale.x * (1f + _squashAmount),
            _originalScale.y * (1f - _squashAmount),
            _originalScale.z), _bounceDuration / 4));

        _bounceSequence.Append(transform.DOMoveY(transform.position.y, _bounceDuration / 2)
            .SetEase(Ease.InQuad));

        _bounceSequence.Join(transform.DOScale(_originalScale, _bounceDuration / 4)
            .SetDelay(_bounceDuration / 4));

        _bounceSequence.Insert(_bounceDuration / 4, transform.DOScale(new Vector3(
            _originalScale.x * (1f - _squashAmount / 2),
            _originalScale.y * (1f + _squashAmount / 2),
            _originalScale.z), _bounceDuration / 4));

        _bounceSequence.SetLoops(-1, LoopType.Restart);
        _bounceSequence.SetEase(Ease.OutQuad);
    }

    /// <summary>Kills the bounce sequence and any stray tweens on the sheep to prevent tweens running on a destroyed transform after a scene change.</summary>
    private void OnDestroy()
    {
        _bounceSequence?.Kill();
        transform.DOKill();
    }
}