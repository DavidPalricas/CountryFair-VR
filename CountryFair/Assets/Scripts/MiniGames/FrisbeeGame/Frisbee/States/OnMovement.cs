using UnityEngine;
using UnityEngine.Events;

public class OnMovement : FrisbeeState
{
    [Header("Ajustes Aerodinâmicos")]
    [Tooltip("Força de sustentação. Quanto maior, mais ele sobe.")]
    [SerializeField]
    private float liftStrength = 0.15f; // Mantém baixo para não voar para o espaço
    
    [Tooltip("Arrasto quando o disco corta o ar (Faca).")]
     [SerializeField]
    private float baseDrag = 0.05f;     
    
    [Tooltip("Arrasto quando o disco cai de barriga (Paraquedas). Aumentei isto!")]
    [SerializeField]
    private float highAngleDrag = 3.5f; // AUMENTADO (era 1.5): Trava forte na queda
    
    // Variáveis para controlar a aterragem
    private bool _touchedGround = false;

    // Guardar valores originais do Rigidbody
    private float _defaultAngularDrag;

    public UnityEvent playerMissed;

    protected override void Awake()
    {
        base.Awake();
        _defaultAngularDrag = _rigidbody.angularDamping;
    }

    public override void Enter()
    {
        base.Enter();

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

        _trajectoryLine.enabled = false;

        _touchedGround = false;
    }

    private void ApplySimpleAerodynamics()
    {
        Vector3 velocity = _rigidbody.linearVelocity; 
        float speed = velocity.magnitude;
        const float STOP_THRESHOLD = 0.1f;

        if (speed < STOP_THRESHOLD)
        {
            if (_touchedGround) fSM.ChangeState("StoppedOnGround");
            return;
        }

        // --- CORREÇÃO AQUI ---

        // 1. Calcular o Ângulo de Ataque (AoA)
        // Normalizamos a velocidade para saber apenas a direção do movimento
        Vector3 moveDirection = velocity.normalized;


        // O Dot Product diz-nos o quão perpendiculares são os vetores.
        // -1: O ar bate totalmente no topo (empurra para baixo)
        //  0: O ar corta o disco como uma faca (zero sustentação)
        // +1: O ar bate totalmente na barriga (máxima sustentação - como um paraquedas)
        // Usamos -moveDirection porque queremos o vetor do "Vento" contra o disco
        float angleOfAttack = Vector3.Dot(transform.up, -moveDirection);


        // Se o ângulo for negativo (ar a bater no topo), não queremos lift positivo (ou queremos downforce)
        // Aqui usamos Mathf.Max(0, ...) para garantir que só gera lift se o ar bater por baixo
        float aerodynamicFactor = Mathf.Max(0, angleOfAttack);

        // 2. Calcular a Sustentação (Lift)
        // A física real usa velocidade ao quadrado (speed * speed), o que torna o lançamento
        // mais sensível: cai rápido se lento, plana bem se rápido.
        // Multiplicamos pelo aerodynamicFactor para corrigir a física da curva/faca.
        float dynamicLift = speed * speed * liftStrength * aerodynamicFactor;

        // 3. Clamp (Segurança)
        // Impede que o disco voe para a estratosfera em lançamentos glitchados do VR
        dynamicLift = Mathf.Clamp(dynamicLift, 0, 15f); 

        Vector3 liftForce = transform.up * dynamicLift;

        _rigidbody.AddForce(liftForce);

        Debug.Log("AeorDymanic Details: " +
                 "\n Frisbee Foward: " + transform.forward.ToString("F2") +
                  "\n Speed: " + speed.ToString("F2") +
                  "\n Velocity: " + velocity.ToString("F2") +
                  "\n MoveDir: " + moveDirection.ToString("F2") +
                  "\n Angle of attack: " + angleOfAttack.ToString("F2") +
                  "\n AeroFactor: " + aerodynamicFactor.ToString("F2") +
                  "\n DynamicLift: " + dynamicLift.ToString("F2") + 
                "\n LiftForce: " + liftForce.ToString("F2")
                  );
        
        // DICA EXTRA: Drag Induzido
        // Se o disco está a gerar muito lift (angleOfAttack alto), ele deve travar mais.
        // Isto impede que ele plane para sempre.
        if (aerodynamicFactor > 0)
        {
             // Adiciona uma força contrária ao movimento baseada no lift gerado
            _rigidbody.AddForce(-velocity.normalized * (dynamicLift * 0.1f));
        }
    }

    // Lógica de aterragem (Mata a rotação quando toca no chão)
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && fSM.CurrentState == this)
        {
            _touchedGround = true;
            _rigidbody.angularDamping = 10.0f; 
        }

        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && fSM.CurrentState == this)
        {
            _touchedGround = false;
            _rigidbody.angularDamping = 0.1f; // Volta a voar
        }
    }


    private void OnTriggerExit(Collider other)
    {
          if (other.gameObject.CompareTag("OutOfBounds") && fSM.CurrentState == this)
        {   
            playerMissed.Invoke();
            fSM.ChangeState("FrisbeeOutOfBounds");
        }
    }
}