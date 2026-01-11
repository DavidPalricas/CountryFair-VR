using UnityEngine;
using DG.Tweening; // 1. Não esqueça de importar o namespace

public class IdlePerson : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField]
     private float jumpPower = 2f; 
    [SerializeField] 
    private float jumpDuration = 0.5f; 


    private bool _isOnGround = true;

    private void Awake()
    {
        DOTween.Init(); 
    }

    public void Jump()
    {
        if ( _isOnGround && !DOTween.IsTweening(transform))
        {
            Sequence sequence = DOTween.Sequence();

            _isOnGround = false;


            const int NUMBER_OF_JUMPS = 1;

            sequence.Append(transform.DOJump(transform.position, jumpPower, NUMBER_OF_JUMPS, jumpDuration));

         
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear));
            
            sequence.OnComplete(() => _isOnGround = true);
        } 
    }
}