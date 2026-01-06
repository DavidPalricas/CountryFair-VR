using UnityEngine;
using DG.Tweening; // 1. Não esqueça de importar o namespace

public class IdlePerson : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField]
     private float jumpPower = 2f; 
    [SerializeField] 
    private float jumpDuration = 0.5f; //

    private void Awake()
    {
        DOTween.Init(); 
    }

    public void Jump()
    {
        // If the person is not jumping
        if (!DOTween.IsTweening(transform))
        {
              // Cria a sequência
            Sequence sequence = DOTween.Sequence();

            // 1. Adiciona o movimento do Pulo
            sequence.Append(transform.DOJump(transform.position, jumpPower, 1, jumpDuration));

            // 2. Adiciona a Rotação AO MESMO TEMPO (Join)
            // RotateMode.LocalAxisAdd: Adiciona 360 à rotação atual (para não resetar a rotação dele)
            // Ease.Linear: Para a rotação ser constante e não acelerar/desacelerar no meio
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear));
        } 
    }
}