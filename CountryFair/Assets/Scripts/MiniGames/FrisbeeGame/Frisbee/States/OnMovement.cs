using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee in flight after being thrown.
/// Simulates realistic frisbee physics including aerodynamic forces (lift and drag), spin dynamics,
/// and gravity.
/// Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
/// </summary>
public class OnMovement : FrisbeeState
{
    [Header("Aerodynamic Settings")]
    [Tooltip("Densidade do ar em kg/m^3. Padrão ao nível do mar: 1.225")]
    public float airDensity = 1.225f;

    [Tooltip("Área do frisbee em m^2. Um disco oficial tem ~0.057m^2")]
    public float area = 0.057f;

    [Header("Coefficients (Morrison Model)")]
    [Tooltip("Curva de Sustentação (CL) vs Ângulo de Ataque. Eixo X: Ângulo (-180 a 180), Eixo Y: CL")]
    public AnimationCurve liftCoefficientCurve;

    [Tooltip("Curva de Arrasto (CD) vs Ângulo de Ataque. Eixo X: Ângulo (-180 a 180), Eixo Y: CD")]
    public AnimationCurve dragCoefficientCurve;

    /// <summary>Whether the frisbee has touched the ground during flight.</summary>
    private bool _touchedGround = false;

    public UnityEvent frisbeeThrown;

    protected override void Awake()
    {
        base.Awake();
        // Setup padrão das curvas se não forem definidas no Inspector (Fallback de segurança)
        if (liftCoefficientCurve.length == 0) SetupDefaultLiftCurve();
        if (dragCoefficientCurve.length == 0) SetupDefaultDragCurve();
    }

    /// <summary>
    /// Called when entering the OnAir state (when frisbee is thrown).
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        // Configuração vital para o Spin funcionar bem:
        // Reduzimos o atrito angular para que o disco mantenha o spin por mais tempo.
        _rigidbody.angularDamping = 0.1f; 
        
        frisbeeThrown.Invoke();
    }

    /// <summary>
    /// Called every physics frame while in the OnAir state.
    /// </summary>
    public override void Execute()
    {
        base.Execute();
        // ApplyAerodynamicForces();
    }

    public override void Exit()
    {
        base.Exit();
        // Resetamos o atrito angular ao aterrar para ele não rolar para sempre
        _rigidbody.angularDamping = 0.5f; 
    }

    /// <summary>
    /// Calculates and applies Lift and Drag based on velocity and angle of attack.
    /// </summary>
    private void ApplyAerodynamicForces()
    {
        // 1. Obter Velocidade e verificar paragem
        Vector3 velocity = _rigidbody.linearVelocity; // Unity 6+ (use .velocity em versões anteriores)
        float vMagnitude = velocity.magnitude;
        const float STOP_THRESHOLD = 0.1f;

        if (vMagnitude < STOP_THRESHOLD)
        {
            if (_touchedGround)
            {
                fSM.ChangeState("StoppedOnGround");
            }
            return;
        }

        // --- CÁLCULOS AERODINÂMICOS ---

        // 2. Calcular o Ângulo de Ataque (Alpha)
        // Transformamos a velocidade para o espaço local do disco para saber
        // se o ar está a bater "por baixo" ou "por cima".
        // Assumindo que o eixo Z local é a "frente" e Y local é o "topo".
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        Debug.Log("Local Velocity: " + localVelocity.ToString("F2"));
        
        // Atan2 dá-nos o ângulo vertical da velocidade em relação ao plano do disco
        float angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;

        // 3. Obter Coeficientes (CL e CD) das curvas baseadas no Alpha
        float cl = liftCoefficientCurve.Evaluate(angleOfAttack);
        float cd = dragCoefficientCurve.Evaluate(angleOfAttack);

        // 4. Calcular Pressão Dinâmica (q = 0.5 * rho * v^2 * Area)
        // Esta é a magnitude base da força do ar
        float dynamicPressure = 0.5f * airDensity * vMagnitude * vMagnitude * area;

        // 5. Aplicar Força de Arrasto (Drag - D)
        // O arrasto atua SEMPRE na direção oposta à velocidade
        Vector3 dragDirection = -velocity.normalized;
        Vector3 dragForce = dragDirection * (cd * dynamicPressure);
        
        _rigidbody.AddForce(dragForce);

        // 6. Aplicar Força de Sustentação (Lift - L)
        // A sustentação atua perpendicularmente à velocidade.
        // O produto vetorial (Cross Product) entre a velocidade e o eixo "direita" do disco
        // dá-nos o vetor "para cima" relativo ao fluxo de ar.
        Vector3 liftDirection = Vector3.Cross(velocity, transform.right).normalized;
        
        // Correção de segurança: se a velocidade for zero ou vertical, o cross product pode falhar
        if (liftDirection == Vector3.zero) liftDirection = transform.up;

        Vector3 liftForce = liftDirection * (cl * dynamicPressure);

        _rigidbody.AddForce(liftForce);

        // DEBUG (Opcional: desenha as forças na Scene View)
        // Debug.DrawRay(transform.position, dragForce, Color.blue); // Arrasto
        // Debug.DrawRay(transform.position, liftForce, Color.green); // Sustentação
    }

    // --- Helpers para configurar curvas padrão se esqueceres no Inspector ---
    private void SetupDefaultLiftCurve()
    {
        // Aproximação simplificada de Morrison
        liftCoefficientCurve.AddKey(-10, -0.2f);
        liftCoefficientCurve.AddKey(0, 0.15f); // Ligeira sustentação a 0 graus
        liftCoefficientCurve.AddKey(15, 0.8f); // Pico de sustentação
        liftCoefficientCurve.AddKey(45, 0.0f); // Stall
    }

    private void SetupDefaultDragCurve()
    {
        // Arrasto aumenta exponencialmente com o ângulo
        dragCoefficientCurve.AddKey(0, 0.08f); // Mínimo arrasto
        dragCoefficientCurve.AddKey(20, 0.4f);
        dragCoefficientCurve.AddKey(90, 1.5f); // Máximo arrasto (lado plano contra o vento)
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _touchedGround = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _touchedGround = false;
        }
    }
}