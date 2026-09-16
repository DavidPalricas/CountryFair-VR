using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PlayerScale : MonoBehaviour
{
   /// <summary>
   /// Root transform of the XR rig. Scaled up/down to grow/shrink the player.
   /// </summary>
   [SerializeField]
   private Transform _cameraRigTransform;

   /// <summary>
   /// Player's head (Center Eye Anchor), used as the pivot of the scale animation so the player does not drift sideways while growing.
   /// </summary>
   [SerializeField]
   [Tooltip("Player's head (Center Eye Anchor). Used as the pivot of the scale animation, so the player does not drift sideways while growing")]
   private Transform _headTransform;

   /// <summary>
   /// Multiplier applied to the rig's normal scale when in giant mode.
   /// </summary>
   [SerializeField]
   private float _giantScaleFactor = 7f;


   [Header("Scale Animation")]
   /// <summary>
   /// Duration, in seconds, of the growing animation.
   /// </summary>
   [SerializeField]
   [Tooltip("Duration of the growing animation")]
   private float _scaleUpDuration = 1f;

   /// <summary>
   /// Duration, in seconds, of the shrinking animation.
   /// </summary>
   [SerializeField]
   [Tooltip("Duration of the shrinking animation")]
   private float _scaleDownDuration = 0.8f;

   /// <summary>
   /// Anticipation squash before growing (Mario feel), as a fraction of the normal height. 0 disables the squash, which is the most comfortable setting.
   /// </summary>
   [SerializeField]
   [Tooltip("Anticipation squash before growing (Mario feel), as a fraction of the NORMAL height. 0 = no squash, the most comfortable setting")]
   [Range(0f, 0.15f)]
   private float _anticipationAmount = 0.04f;

   /// <summary>
   /// Duration, in seconds, of the anticipation squash before growing.
   /// </summary>
   [SerializeField]
   private float _anticipationDuration = 0.12f;


   [Header("Events")]
   /// <summary>
   /// Invoked when the player transitions into giant mode, carrying the sound effect to play.
   /// </summary>
   [SerializeField]
   private UnityEvent<AudioManager.GameSoundEffects> _onScaleToGiant;

    /// <summary>
    /// Invoked when the player transitions back to normal scale, carrying the sound effect to play.
    /// </summary>
    [SerializeField]
    private UnityEvent<AudioManager.GameSoundEffects> _onScaleToNormal;

   /// <summary>
   /// Whether the player is currently scaled up to giant mode. Toggled by <see cref="ToggleScale"/> to pick the next animation target.
   /// </summary>
   private bool _isGiantMode = false;

   /// <summary>
   /// Rig's uniform scale before any giant-mode animation, captured in <see cref="Awake"/>. Used as the "normal" target when shrinking back.
   /// </summary>
   private float _originalScale = 1f;

   /// <summary>
   /// Currently running grow/shrink tween sequence, kept so a new toggle can kill it and so <see cref="ToggleScale"/> can ignore input while it plays.
   /// </summary>
   private Sequence _scaleSequence;

   /// <summary>
   /// The scale being animated, stored in logarithmic space. See <see cref="AnimateScale"/> for the reason.
   /// </summary>
   private float _animatedLogScale;


   /// <summary>
   /// Sound effect passed to <see cref="_onScaleToGiant"/> when the player grows.
   /// </summary>
   private readonly AudioManager.GameSoundEffects _scaleToGiantSoundEffect = AudioManager.GameSoundEffects.SCALE_TO_GIANT;

   /// <summary>
   /// Sound effect passed to <see cref="_onScaleToNormal"/> when the player shrinks back.
   /// </summary>
   private readonly AudioManager.GameSoundEffects _scaleToNormalSoundEffect = AudioManager.GameSoundEffects.SCALE_TO_NORMAL;


  /// <summary>
  /// Caches the rig's starting scale.
  /// </summary>
  private void Awake()
   {
       if (_cameraRigTransform == null)
       {
           Debug.LogError("Camera Rig Transform reference is null in PlayerScale");
           return;
       }

       _originalScale = _cameraRigTransform.localScale.x;

       // Ensure that scale is uniform across all axes
       transform.localScale = Vector3.one * _originalScale;
   }

   /// <summary>
   /// Switches the player between normal and giant scale, playing the corresponding grow/shrink animation
   /// and sound effect. Ignored while a previous scale animation is still playing.
   /// </summary>
   /// <remarks>Invoked via Inspector in CountryFair scene (wrist menu / cheat code binding)</remarks>
   public void ToggleScale()
   {
       // Ignores the input while an animation is still playing, avoiding scale jumps
       if (_scaleSequence != null && _scaleSequence.IsActive() && _scaleSequence.IsPlaying())
       {
           return;
       }

       _isGiantMode = !_isGiantMode;

       if (_isGiantMode)
       {
           AnimateScale(_originalScale * _giantScaleFactor, _scaleUpDuration);
           _onScaleToGiant.Invoke(_scaleToGiantSoundEffect);
           return;
       }

       AnimateScale(_originalScale, _scaleDownDuration);
       _onScaleToNormal.Invoke(_scaleToNormalSoundEffect);
   }

   /// <summary>
   /// Plays a Mario like grow/shrink animation on the camera rig.
   /// Comfort rules applied here:
   /// - the scale is interpolated in logarithmic space, so the world grows at a constant *relative* rate.
   ///   A linear interpolation from 1x to 4x sweeps the world across the eyes very fast at the beginning
   ///   (when everything is still close) and slowly at the end, and that initial burst is what makes the player dizzy;
   /// - the scale is anchored on the player's head, so only the eye height changes (no sideways swoop);
   /// - the easing is smooth and has no overshoot/bounce, so there are no direction changes at the end.
   /// </summary>
   private void AnimateScale(float targetScale, float duration)
   {
       _scaleSequence?.Kill();

       float currentScale = _cameraRigTransform.localScale.x;

       // Ground point under the head, kept fixed during the whole animation
       Vector3 pivotWorldPosition = GetHeadPivotWorldPosition();
       Vector3 pivotLocalPosition = _cameraRigTransform.InverseTransformPoint(pivotWorldPosition);

       _animatedLogScale = Mathf.Log(currentScale);

       _scaleSequence = DOTween.Sequence();

       // Anticipation: small squash before growing, like Mario crouching before he gets big.
       // It is skipped when shrinking, where it would only add an unexpected extra drop.
       bool isGrowing = targetScale > currentScale;

       if (isGrowing && _anticipationAmount > 0f)
       {
           float squashScale = Mathf.Max(0.01f, currentScale - (_originalScale * _anticipationAmount));

           _scaleSequence.Append(CreateScaleTween(squashScale, _anticipationDuration, Ease.OutSine));
       }

       _scaleSequence.Append(CreateScaleTween(targetScale, duration, Ease.InOutSine));

       _scaleSequence.OnUpdate(() => KeepPivotAnchored(pivotWorldPosition, pivotLocalPosition));

       _scaleSequence.SetLink(gameObject).SetUpdate(UpdateType.Normal, true);
   }

   /// <summary>
   /// Builds a tween that drives the rig scale through <see cref="_animatedLogScale"/>, keeping the growth
   /// perceptually linear instead of numerically linear.
   /// </summary>
   private Tween CreateScaleTween(float targetScale, float duration, Ease ease)
   {
       return DOTween.To(() => _animatedLogScale,
                         logScale =>
                         {
                             _animatedLogScale = logScale;
                             _cameraRigTransform.localScale = Vector3.one * Mathf.Exp(logScale);
                         },
                         Mathf.Log(targetScale),
                         duration)
                     .SetEase(ease);
   }

   /// <summary>
   /// Returns the world position the scale animation grows around: the floor point under the player's head.
   /// Falls back to the rig position when there is no head reference.
   /// </summary>
   private Vector3 GetHeadPivotWorldPosition()
   {
       if (_headTransform == null)
       {
           return _cameraRigTransform.position;
       }

       return new Vector3(_headTransform.position.x, _cameraRigTransform.position.y, _headTransform.position.z);
   }

   /// <summary>
   /// Compensates the horizontal offset introduced by the scale, so the player only feels the height change.
   /// </summary>
   private void KeepPivotAnchored(Vector3 pivotWorldPosition, Vector3 pivotLocalPosition)
   {
       Vector3 offset = pivotWorldPosition - _cameraRigTransform.TransformPoint(pivotLocalPosition);

       _cameraRigTransform.position += new Vector3(offset.x, 0f, offset.z);
   }

   /// <summary>
   /// Stops any in-flight scale tween so it does not keep running against a destroyed object.
   /// </summary>
   private void OnDestroy()
   {
       _scaleSequence?.Kill();
   }
}
