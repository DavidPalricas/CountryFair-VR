using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
 public class DogState: State
{   
    [SerializeField]
    private float rotationSpeed = 2f;

    protected NavMeshAgent _agent;

    protected Transform _playerTransform;


    protected FrisbeeGameManager _gameManager;

    protected virtual void Awake()
    {   
        SetStateProprieties();

        _agent = GetComponent<NavMeshAgent>();

        _agent.updateRotation = false;
    }

    public override void LateStart()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_playerTransform == null)
        {
            Debug.LogError("Player GameObject not found in the scene.");
        }

        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FrisbeeGameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("GameManager game obejct not found or FrisbeeGameManager component.");
        }
    }

    protected bool DogStoped()
    {
        return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
    }

    protected void RotateDogTowardsTarget(Transform targetTransform)
    {
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        directionToTarget.y = 0; 

        if (directionToTarget != Vector3.zero)
        {   
            // The direction is inverted to make the dog face the target correctly
            Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            Debug.Log("Rotating dog towards target at position: " + targetTransform.position);
        }
    }
}