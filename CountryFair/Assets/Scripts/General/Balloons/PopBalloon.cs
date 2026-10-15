using UnityEngine;

/// <summary>
/// Pops a decorative balloon: spawns its burst effect and destroys the balloon.
/// Called by <see cref="BalloonsSpawner"/> when the balloon's rise tween completes.
/// </summary>
public class PopBalloon : MonoBehaviour
{
    /// <summary>
    /// Visual effect prefab instantiated when a balloon pops.
    /// </summary>
    [SerializeField]
    private GameObject _popBalloonEffect = null;

   /// <summary>
   /// The duration in seconds before the pop effect is destroyed.
   /// </summary>
    [SerializeField]
    private float _effectDuration = 2f;

    /// <summary>Logs an error if the pop effect is not assigned, since <see cref="Pop"/> would instantiate null.</summary>
    protected void Awake()
    {
        if (_popBalloonEffect == null)
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
        GameObject effect = Instantiate(_popBalloonEffect, transform.position, Quaternion.identity);

        Destroy(effect, _effectDuration);

        Destroy(gameObject);
    }
}