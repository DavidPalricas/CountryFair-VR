using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// State representing the frisbee floating in front of the player, ready to be grabbed and thrown.
/// Keeps it in front of the head (<see cref="FollowPlayerHead"/>), lets the Meta XR <see cref="Grabbable"/> handle the
/// grab (enabled through the per-hand <see cref="HandGrabInteractable"/>s), and hands it over to physics when the grab
/// is released.
/// </summary>
/// <remarks>
/// <para>
/// This state handles the frisbee while it waits in front of the player:
/// <list type="bullet">
/// <item><description>Resets it in front of the player when entered (game start or after the dog returns it)</description></item>
/// <item><description>Shows it semi-transparent and not grabbable while the dog is not yet in position, fully opaque
/// and grabbable once it is (always grabbable during the tutorial, which has no dog)</description></item>
/// <item><description>On release (<see cref="ThrowFrisbee"/>, Inspector-wired), enables physics and the trajectory line and fires "FrisbeeThrown"</description></item>
/// </list>
/// </para>
/// <para>
/// The release velocity comes from the Grabbable's throw handling; this state does not set velocity or spin itself.
/// </para>
/// </remarks>
[RequireComponent(typeof(FollowPlayerHead))]
public class OnPlayerFront : FrisbeeState
{
    /// <summary>FSM transition fired when the player throws the frisbee.</summary>
    private const string FrisbeeThrownTransition = "FrisbeeThrown";
    /// <summary>FSM transition fired by the score cheat, which lands the frisbee straight on a score area.</summary>
    private const string ForcedPointTransition = "ForcedPoint";

    /// <summary>
    /// Hand grab interactables of the frisbee (one per hand). Toggling them is what allows or blocks grabbing: they
    /// are disabled on throw and while the dog is not ready to catch.
    /// </summary>
   [SerializeField]
   private HandGrabInteractable[] _handInteractables;
   
    /// <summary>Wired to the audio manager to play the throw sound attached to the frisbee.</summary>
   [SerializeField]
    private UnityEvent <AudioManager.GameSoundEffects, GameObject> _throwSoundEffectEvent;

    /// <summary>
    /// Alpha (opacity) value for the frisbee materials when the player cannot throw.
    /// Lower values make the frisbee more transparent, providing visual feedback that throwing is disabled.
    /// </summary>
    [Header("Rendering Parameters")]
    
    [SerializeField]  
    private float _cannotThrowAlpha = 0.5f;

    /// <summary>Renderer whose material instances are switched to transparent mode and faded.</summary>
   [SerializeField]
   private Renderer _frisbeeRenderer;

    /// <summary>
    /// Array of materials from the frisbee's Renderer component.
    /// Used to modify opacity based on throw availability.
    /// </summary>
    private Material[] _materials;

    /// <summary>Sound effect passed to <see cref="_throwSoundEffectEvent"/> on every throw.</summary>
    private readonly AudioManager.GameSoundEffects _throwSoundEffect = AudioManager.GameSoundEffects.FrisbeeThrow;

    /// <summary>Keeps the frisbee in front of the head while waiting; disabled for the flight.</summary>
    private FollowPlayerHead _followPlayerHead;

    /// <summary>
    /// Whether the dog is in position to catch. Starts false until <see cref="DogIdle"/> reports the dog at its target.
    /// </summary>
    /// <remarks>
    /// Kept in sync by <see cref="OnDogReadyToCatchChanged"/>. Outside the tutorial it drives the opacity and whether
    /// the hand interactables are enabled, and gates <see cref="ForceScorePoint"/>. <see cref="ThrowFrisbee"/> does not
    /// check it directly: a frisbee that cannot be grabbed cannot be released.
    /// </remarks>
    private bool _isDogReadyToCatch = false;
    
    /// <summary>
    /// True if the frisbee is currently held by the right hand, false for the left.
    /// </summary>
    /// <remarks>
    /// Set via Inspector by each hand's grab interactable. <see cref="ThrowFrisbee"/> only accepts the release from the
    /// hand that is holding it, so releasing the other hand during a two-handed grab does not throw.
    /// </remarks>
    public bool IsBeingGrabbedByRightHand {get; set;} = false;

    /// <summary>
    /// Validates the references, caches <see cref="FollowPlayerHead"/> and switches the frisbee materials to
    /// transparent mode so their alpha can be changed at runtime.
    /// </summary>
    protected override void Awake()
    {  
        base.Awake();
    
        if (_frisbeeRenderer == null)
        {
            Debug.LogError("Component Renderer is not assigned in OnPlayerFront script.");

            return;
        }

        if (_handInteractables.Length == 0)
        {
            Debug.LogError("HandGrabInteractors are not assigned in OnPlayerFront script.");

            return;
        }

        _followPlayerHead = GetComponent<FollowPlayerHead>();

        SetUpMaterialsTransparency();
    }
 
   

    /// <summary>
    /// Called when entering the OnPlayerFront state.
    /// Resets the frisbee in front of the player and configures it to be grabbed.
    /// </summary>
    /// <remarks>
    /// Updates the material opacity based on <see cref="_isDogReadyToCatch"/> status and calls
    /// <see cref="InPlayerFront"/> to reset physics and transform.
    /// This state is entered when the dog returns the frisbee to the player or at game start.
    /// </remarks>
    public override void Enter()
   {
        base.Enter();
        
       ChangeMaterialsOpacity();
        
     
        InPlayerFront();
   }

    /// <summary>
    /// Called every frame while in the OnPlayerFront state. No per-frame logic: the throw is event-driven through
    /// <see cref="ThrowFrisbee"/>.
    /// </summary>
    public override void Execute()
    {   
         base.Execute();
    }

    /// <summary>
    /// Called when exiting the OnPlayerFront state. The throw setup has already been done in <see cref="ThrowFrisbee"/>.
    /// </summary>
    public override void Exit()
    {   
        base.Exit();
    }

     /// <summary>
    /// Hands the released frisbee over to physics. Ignored unless this state is active and the release comes from the
    /// hand that is holding it (<see cref="IsBeingGrabbedByRightHand"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The throw sequence:
    /// <list type="number">
    /// <item><description>Stops following the head</description></item>
    /// <item><description>Enables physics simulation (non-kinematic, with gravity)</description></item>
    /// <item><description>Disables trigger mode on collider for physical interactions</description></item>
    /// <item><description>Enables trajectory visualization line and disables the hand grab interactables</description></item>
    /// <item><description>Plays the throw sound and transitions to "FrisbeeThrown" (<see cref="OnMovement"/>)</description></item>
    /// </list>
    /// </para>
    /// <para>Invoked via Inspector by the grab interactables' release events (one per hand).</para>
    /// <para>
    /// Physics Reference: https://web.mit.edu/womens-ult/www/smite/frisbee_physics.pdf
    /// </para>
    /// </remarks>
    /// <param name="thrownByRightHand">Which hand released the frisbee.</param>
    public void ThrowFrisbee(bool thrownByRightHand)
    {   
        if (_fsm.CurrentState == this && thrownByRightHand == IsBeingGrabbedByRightHand)
        {   
            _followPlayerHead.enabled = false;
            
            _rigidbody.isKinematic = false;

            _rigidbody.useGravity = true;

            _collider.isTrigger = false;

            _trajectoryLine.enabled = true; 

            HandleHandInteractables(false);

            _throwSoundEffectEvent.Invoke(_throwSoundEffect, gameObject);

            _fsm.ChangeState(FrisbeeThrownTransition);
        }
    }


    /// <summary>Enables or disables every hand grab interactable, i.e. whether the frisbee can be grabbed.</summary>
    private void HandleHandInteractables(bool enable)
    {
        foreach (var interactable in _handInteractables)
        {
            interactable.enabled = enable;
        }
    }

    /// <summary>
    /// Puts the frisbee back in its waiting configuration in front of the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Restoration sequence:
    /// <list type="bullet">
    /// <item><description>Resets the local rotation</description></item>
    /// <item><description>Keeps the Rigidbody non-kinematic but without gravity, zeroes its velocities and freezes X/Z rotation</description></item>
    /// <item><description>Makes the collider a trigger so it does not push anything while waiting</description></item>
    /// <item><description>Re-enables <see cref="FollowPlayerHead"/>, and the hand grab interactables only during the
    /// tutorial or when the dog is ready to catch</description></item>
    /// </list>
    /// Activation of the GameObject itself is handled by <see cref="GiveFrisbeeToPlayer"/>, not here.
    /// </para>
    /// </remarks>
    private void InPlayerFront()
    {   
        ResetTransform();

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        _collider.isTrigger = true;

        _followPlayerHead.enabled = true;

        // Grabbable only once the dog is in position to catch (it is usually still walking back when it hands the
        // frisbee over). The tutorial has no dog, so there it is always grabbable.
        bool enableGrabbable = TutorialActive || _isDogReadyToCatch;

        HandleHandInteractables(enableGrabbable);
    }

    /// <summary>
    /// Changes the opacity of all frisbee materials based on whether the player can throw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="_isDogReadyToCatch"/> or <see cref="FrisbeeState.TutorialActive"/> is true:
    /// <list type="bullet">
    /// <item><description>Alpha is set to 1.0 (fully opaque)</description></item>
    /// <item><description>Render queue is set to 2000 (opaque rendering)</description></item>
    /// <item><description>ZWrite is enabled for proper depth rendering</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Otherwise:
    /// <list type="bullet">
    /// <item><description>Alpha is set to <see cref="_cannotThrowAlpha"/> (semi-transparent)</description></item>
    /// <item><description>Render queue is set to 3000 (transparent rendering)</description></item>
    /// <item><description>ZWrite is disabled for proper transparency sorting</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This provides clear visual feedback to the player about whether they can throw the frisbee.
    /// </para>
    /// </remarks>
    private void ChangeMaterialsOpacity()
    {    
        float alphaToChange;

        int renderQueue, zWriteValue;

        if (_isDogReadyToCatch || TutorialActive)
        {   
            const float MaxAlpha = 1f;

            alphaToChange = MaxAlpha;
            renderQueue = 2000;
            zWriteValue = 1;
        }
        else
        {
            
            alphaToChange = _cannotThrowAlpha;
            renderQueue = 3000;
            zWriteValue = 0;
        }

        foreach (Material mat in _materials)
        {
            Color color = mat.color;
            color.a = alphaToChange;
            mat.color = color;

            mat.SetInt("_ZWrite", zWriteValue);
            mat.renderQueue = renderQueue;
        }
    }


    /// <summary>
    /// Configures all frisbee materials to support transparency effects.
    /// </summary>
    /// <remarks>
    /// Sets up URP/Lit shader properties for alpha blending:
    /// <list type="bullet">
    /// <item><description>Sets surface type to Transparent</description></item>
    /// <item><description>Configures alpha blend mode</description></item>
    /// <item><description>Sets up blend modes (SrcAlpha, OneMinusSrcAlpha)</description></item>
    /// <item><description>Configures render queue for transparent rendering</description></item>
    /// <item><description>Enables appropriate shader keywords</description></item>
    /// </list>
    /// This is called once during initialization to prepare materials for runtime opacity changes.
    /// </remarks>
    private void SetUpMaterialsTransparency()
    {   
        _materials = _frisbeeRenderer.materials;
        
        foreach (Material mat in _materials)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_AlphaClip", 0);
            
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_OFF");
        }
    }
  
    /// <summary>
    /// Resets the local rotation; the position is handled by <see cref="FollowPlayerHead"/>.
    /// </summary>
    private void ResetTransform()
    {
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Called (via Inspector, from <see cref="DogIdle"/>) whenever the dog becomes ready or stops being ready to catch.
    /// </summary>
    /// <param name="isReady">True when the dog enters <see cref="DogIdle"/> at its target; false when it leaves it.</param>
    /// <remarks>
    /// When the dog becomes ready while the frisbee is waiting in front of the player, the frisbee turns opaque and
    /// grabbable again.
    /// </remarks>
    public void OnDogReadyToCatchChanged(bool isReady)
    {
        _isDogReadyToCatch = isReady;

        if (isReady && _fsm.CurrentState == this)
        {
            ChangeMaterialsOpacity();
            HandleHandInteractables(true);
        }
    }

    /// <summary>
    /// Cheat shortcut: unparents the waiting frisbee, places it at <paramref name="targetPosition"/> and fires
    /// "ForcedPoint", skipping the throw. Ignored unless this state is active and the dog is ready to catch.
    /// </summary>
    /// <param name="targetPosition">World position of the score area to land in (the frisbee has no parent at that point).</param>
    /// <remarks>Invoked via Inspector (FrisbeeGame scene) by <c>FrisbeeCheatCodes._forceScorePoint</c>.</remarks>
    public void ForceScorePoint(Vector3 targetPosition)
    {
        if (_fsm.CurrentState == this && _isDogReadyToCatch)
        {    
            transform.parent = null;
            transform.localPosition = targetPosition;
            _fsm.ChangeState(ForcedPointTransition);
        }
    }
}
    