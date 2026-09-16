using DG.Tweening;
using UnityEngine;

/// <summary>
/// Marks a slot where a <see cref="MiniGameTent"/> can be placed and plays a looping
/// squash-and-stretch bounce animation to make the empty slot visible to the player.
/// Trigger collider callbacks inform the active tent which placeholder it is hovering over.
/// </summary>
public class MiniGameTentPlaceHolder : TentPlaceHolder
{
    /// <summary>Transform used to position the tent's play button when a tent snaps to this slot.</summary>
    public Transform miniGameButtonPlaceHolderTransform;

    [Header("Squash & Stretch")]
    /// <summary>Child transform that carries the visual model being animated.</summary>
    [SerializeField]
    private Transform _modelTransform;

    /// <summary>How high the model rises above its rest position during the bounce, in local units.</summary>
    [SerializeField]
    private float _bounceHeight = 0.15f;

    /// <summary>Total duration of one full bounce cycle (rise + fall + recover), in seconds.</summary>
    [SerializeField]
    private float _bounceDuration = 1.2f;

    /// <summary>Intensity of the squash and stretch deformation (0 = none, 0.5 = maximum).</summary>
    [Range(0f, 0.5f)]
    [SerializeField]
    private float _squashAmount = 0.12f;

    /// <summary>Fraction of <see cref="_bounceDuration"/> spent on the rise and on the fall phase (they share this ratio).</summary>
    private const float HalfDurationRatio = 0.45f;

    /// <summary>Fraction of <see cref="_bounceDuration"/> spent recovering to the original scale after the fall.</summary>
    private const float RecoverDurationRatio = 0.1f;

    /// <summary>How much the model narrows on the x/z axes, relative to <see cref="_squashAmount"/>, during the rise (stretch) phase.</summary>
    private const float StretchNarrowFactor = 0.7f;

    /// <summary>How much the model flattens on the y axis, relative to <see cref="_squashAmount"/>, during the fall (squash) phase.</summary>
    private const float SquashFlattenFactor = 0.5f;

    /// <summary>Loop count passed to DOTween to make the bounce sequence repeat forever.</summary>
    private const int InfiniteLoops = -1;

    /// <summary>The model's local scale before any squash/stretch deformation is applied; the baseline the animation returns to.</summary>
    private Vector3 _originalScale;

    /// <summary>The model's local rest position before the bounce animation offsets it upward.</summary>
    private Vector3 _originalLocalPosition;

    /// <summary>The currently running looping bounce animation, kept so it can be killed on destroy.</summary>
    private Sequence _squashStretchSequence;

    /// <summary>Caches the model's original scale and local position as the animation baseline.</summary>
    private void Awake()
    {
        if (miniGameButtonPlaceHolderTransform == null || _modelTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in TentPlaceHolder script.");

            return;
        }

        _originalScale = _modelTransform.localScale;
        _originalLocalPosition = _modelTransform.localPosition;
    }

    /// <summary>Starts the looping squash-and-stretch bounce sequence.</summary>
    private void Start()
    {
        StartSquashStretch();
    }

    /// <summary>
    /// Builds and plays the DOTween Sequence that cycles through rise (stretch), fall (squash),
    /// and recover phases on an infinite loop.
    /// </summary>
    private void StartSquashStretch()
    {
        float halfDuration = _bounceDuration * HalfDurationRatio;
        float recoverDuration = _bounceDuration * RecoverDurationRatio;

        Vector3 stretchScale = new (
            _originalScale.x * (1f - _squashAmount * StretchNarrowFactor),
            _originalScale.y * (1f + _squashAmount),
            _originalScale.z * (1f - _squashAmount * StretchNarrowFactor));

        Vector3 squashScale = new(
            _originalScale.x * (1f + _squashAmount),
            _originalScale.y * (1f - _squashAmount * SquashFlattenFactor),
            _originalScale.z * (1f + _squashAmount));

        _squashStretchSequence = DOTween.Sequence();

            // Rise: body elongates and narrows
        _squashStretchSequence.Append(
            _modelTransform.DOLocalMoveY(_originalLocalPosition.y + _bounceHeight, halfDuration)
                .SetEase(Ease.OutSine));

        _squashStretchSequence.Join(
            _modelTransform.DOScale(stretchScale, halfDuration)
                .SetEase(Ease.OutSine));

            // Fall: body flattens and widens
        _squashStretchSequence.Append(
            _modelTransform.DOLocalMoveY(_originalLocalPosition.y, halfDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.Join(
            _modelTransform.DOScale(squashScale, halfDuration)
                .SetEase(Ease.InSine));

            // Recover: body returns to original scale
        _squashStretchSequence.Append(
            _modelTransform.DOScale(_originalScale, recoverDuration)
                .SetEase(Ease.InSine));

        _squashStretchSequence.SetLoops(InfiniteLoops, LoopType.Restart);
    }

    /// <summary>Kills the bounce sequence and any stray tweens on the model to prevent callbacks after destruction.</summary>
    private void OnDestroy()
    {
        _squashStretchSequence?.Kill();
        _modelTransform.DOKill();
    }
}
