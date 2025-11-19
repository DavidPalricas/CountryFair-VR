using UnityEngine;
using DG.Tweening;

/// <summary>
/// Handles the animation of the score area including floating, rotating, and pulsing effects.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ScoreAreaAnim : MonoBehaviour
{
    /// <summary>
    /// Height offset for the floating animation.
    /// </summary>
    [Header("Animation Settings")]
    [SerializeField]
     private float floatHeight = 0.5f;

    /// <summary>
    /// Duration of one cycle of the floating animation.
    /// </summary>
    [SerializeField]
     private float floatDuration = 2f;

    /// <summary>
    /// Duration of one full rotation cycle.
    /// </summary>
    [SerializeField]
     private float rotationDuration = 3f;

    /// <summary>
    /// Duration of one cycle of the pulse scaling animation.
    /// </summary>
    [SerializeField]
     private float pulseDuration = 1.5f;

    /// <summary>
    /// Maximum scale multiplier for the pulse animation.
    /// </summary>
    [SerializeField]
     private float pulseScale = 1.2f;

    /// <summary>
    /// Cached starting position used as baseline for floating animation.
    /// </summary>
    private Vector3 _startPosition;

    /// <summary>
    /// Cached starting scale used as baseline for pulse animation.
    /// </summary>
    private Vector3 _startScale;

    /// <summary>
    /// DOTween sequence controlling all active animations.
    /// </summary>
    private Sequence _animSequence;

    /// <summary>
    /// Caches the initial position and scale for animation reference.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        _startPosition = transform.position;
        _startScale = transform.localScale;
    }

    /// <summary>
    /// Begins the animation sequence.
    /// Unity callback called before the first frame update.
    /// </summary>
    private void Start()
    {
        StartAnimation();
    }

    /// <summary>
    /// Initializes and starts the floating, rotating, and pulsing animations using DOTween.
    /// </summary>
    private void StartAnimation()
    {
        _animSequence = DOTween.Sequence();

        transform.DOMoveY(_startPosition.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        transform.DOScale(_startScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Cleans up DOTween animations when the object is destroyed.
    /// Unity callback called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
        transform.DOKill();
        _animSequence?.Kill();
    }
}