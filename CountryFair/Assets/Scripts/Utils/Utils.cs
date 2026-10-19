using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility class providing helper methods for common game mechanics and calculations.
/// This static class contains reusable functionality for ray casting, random number generation,
/// and other utility operations used throughout the Country Fair VR game.
/// </summary>
public static class Utils
{
    /// <summary>Tag of the head-tracked camera object that <see cref="CastRayMetaQuest"/> casts from.</summary>
    private const string MainCameraTag = "MainCamera";

    /// <summary>
    /// Transform of the <c>MainCamera</c>-tagged object, cached by <see cref="CastRayMetaQuest"/> so the tag search runs
    /// once instead of on every call. It becomes null (Unity's destroyed-object check) when the scene unloads, which
    /// triggers a new search in the next scene.
    /// </summary>
    private static Transform _headTransform;

    /// <summary>
    /// Casts a ray from the user's head in the Meta Quest 3 headset into the game world, along the gaze direction.
    /// Used for gaze-based interactions, e.g. <c>MiniGameTent</c> showing its play button while the player looks at it.
    /// </summary>
    /// <returns>
    /// A Ray starting at the position of the object tagged <c>MainCamera</c> and pointing along its forward vector.
    /// </returns>
    /// <remarks>
    /// Uses the transform of the <c>MainCamera</c>-tagged object directly (the per-eye <c>centerEyeAnchor</c> version
    /// is commented out). That object must follow the head; if no object has the tag, a
    /// <see cref="NullReferenceException"/> is thrown (nothing is logged).
    /// The object is looked up by tag only once per scene and then cached in <see cref="_headTransform"/>.
    ///
    /// Typical usage:
    /// <code>
    /// Ray gazeRay = Utils.CastRayMetaQuest();
    /// if (Physics.Raycast(gazeRay, out RaycastHit hit))
    /// {
    ///     Debug.Log("Looking at: " + hit.collider.gameObject.name);
    /// }
    /// </code>
    /// </remarks>
    /// <exception cref="NullReferenceException">No GameObject is tagged <c>MainCamera</c>.</exception>
    public static Ray CastRayMetaQuest()
    {
        if (_headTransform == null)
        {
            _headTransform = GameObject.FindGameObjectWithTag(MainCameraTag).transform;
        }


        return new Ray(_headTransform.position, _headTransform.forward);
    }

    /// <summary>
    /// Returns a random float in [<paramref name="min"/>, <paramref name="max"/>], drawn from Unity's global random
    /// generator.
    /// </summary>
    /// <remarks>
    /// The generator is never reseeded here: Unity seeds it once at startup, which is already unpredictable. Reseeding
    /// on every call (with <see cref="Environment.TickCount"/>, as before) made calls in the same millisecond return the
    /// same value and reset the sequence of every other <see cref="UnityEngine.Random"/> caller.
    ///
    /// Typical usage:
    /// <code>
    /// float randomHealth = Utils.RandomValueInRange(10f, 100f);
    /// float randomSpeed = Utils.RandomValueInRange(5f, 15f);
    /// </code>
    /// </remarks>
    /// <param name="min">The minimum float value of the range (inclusive).</param>
    /// <param name="max">The maximum float value of the range (inclusive).</param>
    /// <returns>
    /// A random float value between min and max (inclusive).
    /// If min >= max, it logs an error and returns 0f.
    /// </returns>
    public static float RandomValueInRange(float min, float max)
    {
        if (min >= max)
        {
            Debug.LogError("Utils.RandomValueInRange(float min, float max): min should be less than max.");
            return 0f;
        }

        return UnityEngine.Random.Range(min, max);
    }


    /// <summary>
    /// Returns a random int in [<paramref name="min"/>, <paramref name="max"/>) — <b>max is exclusive</b>, like
    /// <see cref="UnityEngine.Random.Range(int, int)"/> — drawn from Unity's global random generator.
    /// </summary>
    /// <remarks>
    /// Because max is exclusive, it can be called directly with a collection's length to get a valid index.
    /// Like the float overload, it never reseeds the generator.
    ///
    /// Typical usage:
    /// <code>
    /// int index = Utils.RandomValueInRange(0, items.Length);
    /// </code>
    /// </remarks>
    /// <param name="min">The minimum int value of the range (inclusive).</param>
    /// <param name="max">The maximum int value of the range (exclusive).</param>
    /// <returns>
    /// A random int value from min to max - 1.
    /// If min >= max, it logs an error and returns 0.
    /// </returns>
    public static int RandomValueInRange(int min, int max)
    {
        if (min >= max)
        {
            Debug.LogError("Utils.RandomValueInRange(int min, int max): min should be less than max.");
            return 0;
        }

        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// Picks one of the least-used prefabs in <paramref name="counts"/>, keeping their distribution balanced, and
    /// increments its count.
    /// </summary>
    /// <remarks>Each caller owns its <paramref name="counts"/> and decides when to decrement or reset them.</remarks>
    /// <param name="counts">How many times each prefab is in use; must not be empty.</param>
    /// <param name="preferred">Returned whenever it is one of the least-used prefabs; otherwise a random one of them is.</param>
    /// <returns>The selected prefab.</returns>
    public static GameObject PickBalancedPrefab(Dictionary<GameObject, int> counts, GameObject preferred = null)
    {
        int minCount = counts.Min(typeCount => typeCount.Value);

        GameObject[] candidates = counts
            .Where(typeCount => typeCount.Value == minCount)
            .Select(typeCount => typeCount.Key)
            .ToArray();

        GameObject selected = preferred != null && candidates.Contains(preferred)
            ? preferred
            : candidates[RandomValueInRange(0, candidates.Length)];

        counts[selected]++;

        return selected;
    }

    /// <summary>
    /// Whether <paramref name="agent"/> has finished computing its path and is within its stopping distance of the destination.
    /// </summary>
    /// <param name="agent">The NavMesh agent to check.</param>
    /// <returns>True once the agent has reached its destination; otherwise, false.</returns>
    public static bool HasReachedDestination(this NavMeshAgent agent)
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    /// <summary>
    /// Returns the direct children of <paramref name="parent"/> (not grandchildren), in hierarchy order, including
    /// inactive ones.
    /// </summary>
    /// <param name="parent">The transform component of the game object whose children are to be retrieved.</param>
    /// <returns>The children's GameObjects; an empty array if it has none.</returns>
    public static GameObject[] GetChildren(Transform parent)
    {
        GameObject[] children = new GameObject[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i).gameObject;
        }

        return children;
    }
}
