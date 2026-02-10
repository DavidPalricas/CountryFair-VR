using UnityEngine;

[DefaultExecutionOrder(100)] // Mantém isto. É obrigatório para VR sem jitter.
public class FollowPlayerHead : DisplayInPlayerFront
{
    [Header("Motion Settings")]
    [SerializeField] private float positionSmoothTime = 0.2f; // Baixei para 0.2f. 0.3f é muito lento para um objeto físico.
    [SerializeField] private float rotationSmoothTime = 5.0f; // Velocidade de Slerp

    private Vector3 _currentVelocity;
    
    // Removi a variável deadzoneRadius e _targetPosition persistente.
    // Elas eram a causa do efeito "travado".

    protected override void Awake()
    {
        base.Awake();
       
        transform.SetPositionAndRotation(CalculateBaseTarget(), GetTargetRotation());
    }

    private void LateUpdate()
    {
        // O segredo da fluidez: Calcular e aplicar SEMPRE. 
        // Não usar 'ifs' de distância.
        HandlePosition();
        HandleRotation();
    }

    private void HandlePosition()
    {
        Vector3 targetPos = CalculateBaseTarget();

        // O SmoothDamp resolve o jitter sozinho. 
        // Se o movimento da cabeça for minúsculo, o SmoothDamp filtra-o naturalmente.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref _currentVelocity,
            positionSmoothTime
        );
    }

    private void HandleRotation()
    {
        Quaternion targetRot = GetTargetRotation();

        // Para objetos 3D, Slerp contínuo é visualmente mais agradável que SmoothDamp na rotação.
        // Multiplicar por Time.deltaTime * velocidade é a forma correta de usar Slerp no Update.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmoothTime);
    }

    private Vector3 CalculateBaseTarget()
    {
        // Mantive a tua lógica de posicionamento relativa à câmara
        Vector3 target = centerEyeTransform.position;
        target += centerEyeTransform.right * horizontalOffset;
        target += centerEyeTransform.forward * distanceFromPlayer;
        target += centerEyeTransform.up * heightOffset;
        
        return target;
    }
    
    private Quaternion GetTargetRotation()
    {
        // Mantive a tua lógica de ignorar a inclinação vertical (Pitch/Roll)
        Vector3 forwardFlat = centerEyeTransform.forward;
        forwardFlat.y = 0;
        
        if (forwardFlat.sqrMagnitude < 0.001f)
            return transform.rotation;

        return Quaternion.LookRotation(forwardFlat);
    }
}