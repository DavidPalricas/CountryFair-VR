using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Queue of inactive GameObjects, reused instead of instantiating and destroying them every time, so spawning targets
/// and balloons does not cause garbage-collection hitches mid-game.
/// Used by <see cref="MiniGameManager"/> (targets) and <see cref="BalloonsSpawner"/> (decorative balloons).
/// </summary>
/// <remarks>
/// The pool only grows: <see cref="GetObject"/> instantiates when the queue is empty, and <see cref="UpdatePoolSize"/>
/// pre-creates the instances a higher difficulty needs, so they are created at the difficulty change and not mid-aim.
/// The queue can mix instances of different prefabs (e.g. balloon colors, see <see cref="SetPrefab"/>): what matters is
/// the total number of objects, so a reused object keeps the prefab it was created from.
/// </remarks>
public class ObjectPool
{
    /// <summary>Prefab new instances are created from; changed with <see cref="SetPrefab"/>.</summary>
    private GameObject _prefab;

    /// <summary>Inactive instances ready to be handed out.</summary>
    private readonly Queue<GameObject> _pool = new();

    /// <summary>Instances created by this pool so far (active + inactive).</summary>
    public int PoolSize { get; private set; } = 0;

    /// <summary>Creates an empty pool; nothing is instantiated until <see cref="GetObject"/> or <see cref="UpdatePoolSize"/>.</summary>
    /// <param name="prefab">Prefab new instances are created from, until <see cref="SetPrefab"/> changes it.</param>
    public ObjectPool(GameObject prefab)
    {
        _prefab = prefab;
    }

    /// <summary>Changes the prefab the next new instances are created from; instances already in the queue are not affected.</summary>
    public void SetPrefab(GameObject prefab)
    {
        _prefab = prefab;
    }

    /// <summary>Takes an inactive instance (or creates one if the queue is empty), places it and activates it.</summary>
    /// <remarks>The transform is set before activating it, so <c>OnEnable</c> already sees the new position.</remarks>
    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        if (_pool.Count == 0)
        {
            return CreateObject(position, rotation);
        }

        GameObject obj = _pool.Dequeue();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    /// <summary>Always instantiates a new instance of the current prefab, skipping the queue; it joins the pool when returned.</summary>
    /// <remarks>For when the caller needs that exact prefab and the queue may hold others.</remarks>
    public GameObject CreateObject(Vector3 position, Quaternion rotation)
    {
        PoolSize++;

        return Object.Instantiate(_prefab, position, rotation);
    }

    /// <summary>Deactivates <paramref name="obj"/> and queues it for reuse.</summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    /// <summary>Creates inactive instances of the current prefab until the pool holds <paramref name="newSize"/>; a smaller size is ignored.</summary>
    /// <remarks>Called when the difficulty changes the number of targets.</remarks>
    public void UpdatePoolSize(int newSize)
    {
        while (PoolSize < newSize)
        {
            ReturnObject(CreateObject(Vector3.zero, Quaternion.identity));
        }
    }
}
