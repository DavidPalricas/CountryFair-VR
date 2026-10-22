using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Abstract base class for all dog behavior states in the Frisbee mini-game.
/// Extends <see cref="State"/> and provides common functionality for navigation, rotation, and game references.
/// </summary>
/// <remarks>
/// This class requires a NavMeshAgent component for AI navigation.
/// All dog states share common references to the player transform, game manager, and navigation agent.
/// Derived classes should override state lifecycle methods (Enter, Execute, Exit) to implement specific behaviors.
/// </remarks>
[RequireComponent(typeof(NavMeshAgent))]
 public abstract class DogState: AnimatableState
{   
    /// <summary>Wired to <see cref="FrisbeeAudioManager"/> to play the bark attached to the dog; raised by <see cref="Bark"/>.</summary>
    [SerializeField]
    protected UnityEvent<AudioManager.GameSoundEffects, GameObject> _barkEvent;

    /// <summary>
    /// Reference to the player's transform component.
    /// Assigned in the Inspector and used for navigation and positioning relative to the player.
    /// </summary>
    [SerializeField]
    protected Transform _playerTransform;

    /// <summary>
    /// Reference to the NavMeshAgent component used for AI navigation and pathfinding.
    /// Automatically retrieved in <see cref="Awake"/> and configured to disable automatic rotation.
    /// </summary>
    protected NavMeshAgent _agent;

    /// <summary>
    /// Destination chosen by <c>GoToTarget</c> and read by the other dog states. Static, so it is shared by all states
    /// (and would be shared by every dog in the scene).
    /// </summary>
    protected static Vector3 _currentTargetPos;

    /// <summary>Hash of the dog Animator's <c>Speed</c> float (0 = idle, 1 = walking).</summary>
    protected static readonly int SpeedParam = Animator.StringToHash("Speed");
    /// <summary>Hash of the dog Animator's <c>Jump</c> trigger, set when the frisbee lands in a score area.</summary>
    protected static readonly int JumpParam = Animator.StringToHash("Jump");
    /// <summary>Hash of the dog Animator's <c>StopAnim</c> bool, which blocks the walk triggered by a difficulty change.</summary>
    protected static readonly int StopAnimParam = Animator.StringToHash("StopAnim");

    /// <summary>Sound sent through <see cref="_barkEvent"/> by <see cref="Bark"/>, shared by every dog state (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _barkSoundEffect = AudioManager.GameSoundEffects.DogBark;

    /// <summary>
    /// Initializes the dog state by setting up state properties and configuring the NavMeshAgent.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// Calls <see cref="State.SetStateProperties"/> to initialize FSM reference and state name.
    /// Retrieves the NavMeshAgent component and disables automatic rotation (manual rotation is handled by <see cref="RotateDogTowardsTarget"/>).
    /// Derived classes should call base.Awake() when overriding this method.
    /// </remarks>
    protected override void Awake()
    {    
        base.Awake();

        if (!Utils.RequireAssigned(_playerTransform, nameof(_playerTransform), this))
        {
            return;
        }

        _agent = GetComponent<NavMeshAgent>();

        _agent.updateRotation = false;
    }


    public override void Enter()
    {
        base.Enter();
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>
    /// Called once during FSM initialization for all states. Adds nothing to <see cref="State.LateStart"/>; the player
    /// reference is assigned in the Inspector (<see cref="_playerTransform"/>).
    /// </summary>
    public override void LateStart()
    {
        base.LateStart();
    }

    /// <summary>Tweens the dog's yaw (0.5 s) to face <paramref name="targetTransform"/>, ignoring height; kills any running rotation tween first.</summary>
    protected void RotateDogTowardsTarget(Transform targetTransform)
    {
        RotateDogTowardsTarget(targetTransform.position);
    }

    /// <summary>Tweens the dog's yaw (0.5 s) to face <paramref name="targetPosition"/>, ignoring height; kills any running rotation tween first.</summary>
    protected void RotateDogTowardsTarget(Vector3 targetPosition)
    {   
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            transform.DOKill(); 
            transform.DORotateQuaternion(targetRotation, 0.5f).SetEase(Ease.InOutSine);
        }
    }



    /// <summary>Plays the bark sound effect through <see cref="_barkEvent"/>.</summary>
    protected void Bark()
    {
         _barkEvent.Invoke(_barkSoundEffect, gameObject);
    }
}