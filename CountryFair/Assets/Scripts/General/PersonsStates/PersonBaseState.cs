using UnityEngine;

public class PersonBaseState : State
{   
    [SerializeField]
    protected Animator animator;

    protected virtual void Awake()
    {   
        if (animator == null)
        {
            Debug.LogError("Animator reference is missing in a Person State.");
        }

        SetStateProprieties();
    }
}