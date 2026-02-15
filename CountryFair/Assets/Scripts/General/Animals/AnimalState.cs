using UnityEngine;

[RequireComponent(typeof(Animator))]

public class AnimalState: State
{
    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();

        SetStateProprieties();
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
}