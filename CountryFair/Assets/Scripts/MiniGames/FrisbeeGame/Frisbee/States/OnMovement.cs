using UnityEngine;
using UnityEngine.Events;

public class OnMovement : FrisbeeState
{
    [Header("Configuração Simples")]
    [Tooltip("Força que mantém o frisbee no ar. Aumenta para ele planar mais.")]
    public float liftStrength = 0.2f; 

    // Variáveis para controlar a aterragem
    private bool _touchedGround = false;
    public UnityEvent frisbeeThrown;

    // Guardar valores originais do Rigidbody
    private float _defaultAngularDrag;

    protected override void Awake()
    {
        base.Awake();
        _defaultAngularDrag = _rigidbody.angularDamping;
    }

    public override void Enter()
    {
        base.Enter();
        frisbeeThrown.Invoke();
        
        // Garante que o disco roda livremente no ar
        _rigidbody.angularDamping = 0.1f; 
    }

    public override void Execute()
    {
        base.Execute();
        ApplySimpleAerodynamics();
    }

    public override void Exit()
    {
        base.Exit();
        // Restaura o atrito original
        _rigidbody.angularDamping = _defaultAngularDrag;
    }

    private void ApplySimpleAerodynamics()
    {
        Vector3 velocity = _rigidbody.linearVelocity; // Unity 6 (use .velocity antes)
        float speed = velocity.magnitude;

        const float STOP_THRESHOLD = 0.1f;

        // Verifica se parou
        if (speed < STOP_THRESHOLD)
        {
            if (_touchedGround)
            {
                fSM.ChangeState("StoppedOnGround");
            }
            return;
        }

        // --- CÁLCULO SIMPLIFICADO ---
        
        // 1. Sustentação (Lift)
        // Lógica: Se tem velocidade, empurra para cima (na direção do topo do disco)
        // Multiplicamos pela velocidade para que ele caia quando perde embalo.
        Vector3 liftForce = transform.up * (speed * liftStrength);

        // Opcional: Impedir que voe para o espaço se atirares com muita força
        // liftForce = Vector3.ClampMagnitude(liftForce, 20f); 

        _rigidbody.AddForce(liftForce);
        
        // O Arrasto (Drag) agora é tratado automaticamente pelo Rigidbody do Unity!
    }

    // Lógica de aterragem (Mata a rotação quando toca no chão)
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _touchedGround = true;
            _rigidbody.angularDamping = 10.0f; // Trava o spin no chão
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _touchedGround = false;
            _rigidbody.angularDamping = 0.1f; // Volta a voar
        }
    }
}