using UnityEngine;

/// <summary>
/// Represents the idle state of the dog in the Frisbee game.
/// Activates the catch area and transitions to catch state when the frisbee lands.
/// </summary>

public class DogIdle : DogState
{   
    /// <summary>
    /// Reference to the area where the frisbee can be caught.
    /// </summary>
    [SerializeField]
    private GameObject scoreArea;

    /// <summary>
    /// Initializes the state by deactivating the catch area and setting the state name.
    /// Unity callback called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {   
        base.Awake();
        
        scoreArea.SetActive(false);
    }

    public override void LateStart()
    {
        base.LateStart();
    }
    
    /// <summary>
    /// Activates the frisbee catch area when entering the idle state.
    /// </summary>
    public override void Enter()
    {
        base.Enter();


        if (_gameManager == null)
        {
            Debug.LogError("GameManager reference is null in DogIdle state.");
            return;
        }

        _gameManager.currentTargetTransform = transform;

        scoreArea.SetActive(true);
    }

    /// <summary>
    /// Monitors the catch area for a landed frisbee and transitions to catch state when detected.
    /// </summary>
    public override void Execute()
    {
        base.Execute();
    }

    public void FrisbeeLanded()
    {
        fSM.ChangeState("FrisbeeLanded");
    }

    /// <summary>
    /// Deactivates the frisbee catch area when exiting the idle state.
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        scoreArea.SetActive(false);
    }
}
