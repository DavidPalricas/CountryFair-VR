using UnityEngine;


public class PopBalloon : MonoBehaviour  
{  
    /// <summary>
    /// Visual effect prefab instantiated when a balloon pops.
    /// </summary>
    [SerializeField]
    private GameObject popBalloonEffect = null;

   /// <summary>
   /// The duration in seconds before the pop effect is destroyed.
   /// </summary>
    [SerializeField]
    private float effectDuration = 2f;

    protected void Awake()
    {
        if (popBalloonEffect == null)
        {
            Debug.LogError("Pop Balloon Effect is not assigned in ScoreAreaAnim script.");

            return;
        }
    }

    /// <summary>
    /// Instantiates a pop effect at the balloon's position and destroys both the effect and balloon.
    /// The pop effect is automatically cleaned up after a fixed duration.
    /// </summary>
    public void Pop()
    {
        GameObject effect = Instantiate(popBalloonEffect, transform.position, Quaternion.identity);

        Destroy(effect, effectDuration); 
                    
        Destroy(gameObject);
    }
}