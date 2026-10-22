using UnityEngine;

/// <summary>
/// Pops a decorative balloon: plays its burst effect (the balloon then goes back to its <see cref="ObjectPool"/>).
/// Called by <see cref="BalloonsSpawner"/> when the balloon's rise tween completes.
/// </summary>
public class PopBalloon : MonoBehaviour
{
    /// <summary>
    /// Visual effect prefab instantiated (once) the first time the balloon pops; needs a <see cref="ParticleSystem"/> on its root.
    /// </summary>
    [SerializeField]
    private GameObject _popBalloonEffect = null;

    /// <summary>
    /// This balloon's pop effect, created on the first pop and restarted on every later one (see
    /// <see cref="Utils.PlayReusableEffect"/>); it hides itself when it finishes.
    /// </summary>
    private ParticleSystem _popEffectInstance;

    /// <summary>Logs an error if the pop effect is not assigned, since <see cref="Pop"/> would instantiate null.</summary>
    protected void Awake()
    {
        if (!Utils.RequireAssigned(_popBalloonEffect, nameof(_popBalloonEffect), this))
        {
            return;
        }
    }

    /// <summary>Plays the pop effect at the balloon's position, reusing the one from the previous pop.</summary>
    /// <remarks>The balloon itself is not destroyed here: <see cref="BalloonsSpawner"/> returns it to its <see cref="ObjectPool"/>.</remarks>
    public void Pop()
    {
        _popEffectInstance = Utils.PlayReusableEffect(_popEffectInstance, _popBalloonEffect, transform.position);
    }

    /// <summary>Releases the cached pop effect, letting it finish first if it is still playing.</summary>
    private void OnDestroy()
    {
        Utils.ReleaseReusableEffect(_popEffectInstance);
    }
}
