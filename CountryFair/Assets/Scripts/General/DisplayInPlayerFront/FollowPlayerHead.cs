using Unity.VisualScripting;
using UnityEngine;

public class FollowPlayerHead : DisplayInPlayerFront
{


    [Header("Smoothing & Deadzone")]
    [SerializeField] private float smoothTime = 0.3f; // Tempo para alcançar o alvo (maior = mais lento/suave)
   
    private static float _movementThreshold = 0.5f; // Distância mínima para ativar o movimento
    [SerializeField] private bool lockRotationToHead = true;

    private Vector3 _currentVelocity;
    private bool _isFollowing = false;



    private void LateUpdate()
    {
        // LateUpdate é crucial para VR para evitar jitter, pois ocorre após o cálculo da posição da cabeça
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 targetPos = CalculateTargetPosition();
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);


        const float THRESHOLD_TO_AVOID_MOTION_SICKNESS = 0.05f;

        // Lógica de Histerese (Deadzone)
        // Só começa a seguir se a distância exceder o limite
        if (distanceToTarget > _movementThreshold)
        {
            _isFollowing = true;
        }
        // Opcional: Para de seguir se estiver muito perto (evita micro-ajustes constantes)
        else if (distanceToTarget < THRESHOLD_TO_AVOID_MOTION_SICKNESS)
        {
            _isFollowing = false;
        }

        if (_isFollowing)
        {
            // SmoothDamp é superior ao Lerp ou DoTween para este caso específico
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPos, 
                ref _currentVelocity, 
                smoothTime
            );
        }

        // Rotação: Geralmente queres que o UI olhe para o jogador
        if (lockRotationToHead)
        {
            // Rotação suave também é recomendada para evitar enjoo
            Quaternion targetRot = Quaternion.LookRotation(transform.position - centerEyeTransform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        // MATEMÁTICA CORRETA:
        // Usa-se os vetores direcionais da cabeça (forward, up, right) e não os eixos do mundo (Vector3.up, etc.)
        Vector3 target = centerEyeTransform.position;

        target += centerEyeTransform.right * horizontalOffset;

        target += centerEyeTransform.forward * distanceFromPlayer;

        target += centerEyeTransform.up * heightOffset;
        
        return target;
    }
}