using UnityEngine;
public class IdlePerson : MonoBehaviour
{   
    [SerializeField]
    private Animator animator;

    private void Awake()
    {
        if (animator == null)
        {
            Debug.LogError("Animator reference is missing in Idle Person.");
        }
    }


    public void Jump()
    { 
        animator.SetTrigger("Jump");
    }
}