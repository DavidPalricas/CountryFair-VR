using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

/// <summary>
/// Crowd member that performs a DOTween jump with a full 360° Y-axis spin when <see cref="Jump"/> is called.
/// Used by <see cref="Crowd"/> to celebrate player scores.
/// </summary>
public class IdlePerson : MonoBehaviour
{
    /// <summary>Height of the jump arc.</summary>
    [Header("Jump Settings")]
    [SerializeField]
     [FormerlySerializedAs("jumpPower")]
     private float _jumpPower = 2f;
    
    /// <summary>Total duration of the jump-and-spin sequence in seconds.</summary>
    [SerializeField]
    [FormerlySerializedAs("jumpDuration")]
    private float _jumpDuration = 0.5f;

    /// <summary>Blocks a new jump until the current one completes.</summary>
    private bool _isOnGround = true;

    /// <summary>Initializes DOTween in case this is the first tween user in the scene.</summary>
    private void Awake()
    {
        DOTween.Init(); 
    }

    /// <summary>Triggers a single jump with a full 360° Y-axis spin if the person is currently on the ground.</summary>
    public void Jump()
    {
        if ( _isOnGround && !DOTween.IsTweening(transform))
        {
            Sequence sequence = DOTween.Sequence();

            _isOnGround = false;

            const int NumberOfJumps = 1;

            sequence.Append(transform.DOJump(transform.position, _jumpPower, NumberOfJumps, _jumpDuration));

         
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), _jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear));
            
            sequence.OnComplete(() => _isOnGround = true);
        } 
    }
}