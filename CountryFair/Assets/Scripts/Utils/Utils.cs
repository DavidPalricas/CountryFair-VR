using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;

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

    /// <summary>
    /// Plays a one-shot particle effect that is created once and then reused: the first call instantiates
    /// <paramref name="effectPrefab"/> and switches its stop action to <c>Disable</c>, so the effect hides itself when it
    /// finishes instead of being destroyed; later calls only move it and restart it from the beginning.
    /// </summary>
    /// <param name="effect">The owner's cached effect, or null on the first call.</param>
    /// <param name="effectPrefab">Prefab with a <see cref="ParticleSystem"/> on its root.</param>
    /// <param name="position">World position to play the effect at.</param>
    /// <returns>The effect to cache for the next call.</returns>
    /// <remarks>Used by the balloon pops, which happen often and would otherwise instantiate and destroy an effect each time.</remarks>
    public static ParticleSystem PlayReusableEffect(ParticleSystem effect, GameObject effectPrefab, Vector3 position)
    {
        if (effect == null)
        {
            effect = UnityEngine.Object.Instantiate(effectPrefab, position, Quaternion.identity).GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = effect.main;
            main.stopAction = ParticleSystemStopAction.Disable;
        }

        effect.transform.position = position;
        effect.gameObject.SetActive(true);

        effect.Clear();
        effect.Play();

        return effect;
    }

    /// <summary>
    /// Releases an effect created by <see cref="PlayReusableEffect"/> when its owner is destroyed: a playing effect
    /// destroys itself once it finishes (so a pop is never cut short), an idle one is destroyed right away.
    /// </summary>
    public static void ReleaseReusableEffect(ParticleSystem effect)
    {
        if (effect == null)
        {
            return;
        }

        if (effect.gameObject.activeSelf && effect.isPlaying)
        {
            ParticleSystem.MainModule main = effect.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            return;
        }

        UnityEngine.Object.Destroy(effect.gameObject);
    }

    /// <summary>
    /// Checks that a field meant to be assigned in the Inspector is not null, reporting which field of which script is
    /// missing when it is.
    /// </summary>
    /// <remarks>
    /// <see cref="Debug.Assert(bool, object)"/> is stripped from release builds, so a scene shipped with an unassigned
    /// reference would only fail later with a <see cref="NullReferenceException"/> and no hint of what is missing. This
    /// check logs an error that is still there in the build and then stops the game: in the Editor it pauses play mode
    /// on the spot, in a build it quits the application.
    ///
    /// Call it where the dependency is first needed (typically <c>Awake</c>) so the missing reference is reported as soon
    /// as the scene loads. It returns whether the check passed, so a caller that cannot run without the dependency can
    /// stop early instead of carrying on half-initialised.
    ///
    /// Typical usage:
    /// <code>
    /// if (!Utils.RequireAssigned(scoreText, nameof(scoreText), this))
    /// {
    ///     return;
    /// }
    /// </code>
    /// </remarks>
    /// <param name="value">The field to check. Unassigned Unity objects are detected through Unity's null check.</param>
    /// <param name="fieldName">The field's name, preferably given with <c>nameof</c> so it follows renames.</param>
    /// <param name="owner">The script the field belongs to; used in the message and selected when the log is clicked.</param>
    /// <returns>True while the field is assigned; otherwise, false.</returns>
    public static bool RequireAssigned(object value, string fieldName, UnityEngine.Object owner)
    {
        // A missing Inspector reference is a "fake null" UnityEngine.Object, which only Unity's == operator reports as null.
        bool isAssigned = value is UnityEngine.Object unityObject ? unityObject != null : value != null;

        return RequireThat(isAssigned, NotAssignedMessage(fieldName, owner), owner);
    }

    /// <summary>
    /// <see cref="RequireAssigned(object, string, UnityEngine.Object)"/> for FMOD events: an <see cref="EventReference"/>
    /// is a struct, so an unassigned one is never null and is detected through <see cref="EventReference.IsNull"/> instead.
    /// </summary>
    /// <param name="value">The event reference to check.</param>
    /// <param name="fieldName">The field's name, preferably given with <c>nameof</c> so it follows renames.</param>
    /// <param name="owner">The script the field belongs to; used in the message and selected when the log is clicked.</param>
    /// <returns>True while the event is assigned; otherwise, false.</returns>
    public static bool RequireAssigned(EventReference value, string fieldName, UnityEngine.Object owner)
    {
        return RequireThat(!value.IsNull, NotAssignedMessage(fieldName, owner), owner);
    }

    /// <summary>
    /// Checks that an array or list filled in the Inspector has at least one element and that none of its elements is
    /// unassigned, reporting the first problem found.
    /// </summary>
    /// <remarks>
    /// Works with any <see cref="System.Collections.IList"/>, which covers both arrays and <see cref="List{T}"/>. An empty
    /// slot is reported with its index (e.g. <c>_handInteractables[2]</c>).
    /// </remarks>
    /// <param name="collection">The array or list to check.</param>
    /// <param name="fieldName">The field's name, preferably given with <c>nameof</c> so it follows renames.</param>
    /// <param name="owner">The script the field belongs to; used in the message and selected when the log is clicked.</param>
    /// <returns>True while the collection has elements and all of them are assigned; otherwise, false.</returns>
    public static bool RequireNotEmpty(System.Collections.IList collection, string fieldName, UnityEngine.Object owner)
    {
        if (!RequireThat(collection != null && collection.Count > 0, EmptyMessage(fieldName, owner), owner))
        {
            return false;
        }

        for (int i = 0; i < collection.Count; i++)
        {
            if (!RequireAssigned(collection[i], $"{fieldName}[{i}]", owner))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Checks that an <see cref="AnimationCurve"/> set in the Inspector has at least one key.</summary>
    /// <param name="curve">The curve to check.</param>
    /// <param name="fieldName">The field's name, preferably given with <c>nameof</c> so it follows renames.</param>
    /// <param name="owner">The script the field belongs to; used in the message and selected when the log is clicked.</param>
    /// <returns>True while the curve has keys; otherwise, false.</returns>
    public static bool RequireNotEmpty(AnimationCurve curve, string fieldName, UnityEngine.Object owner)
    {
        return RequireThat(curve != null && curve.length > 0, EmptyMessage(fieldName, owner), owner);
    }

    /// <summary>
    /// Checks <paramref name="condition"/>, logging <paramref name="message"/> as an error when it does not hold and then
    /// stopping the game: in the Editor play mode is paused, in a build the application quits.
    /// </summary>
    /// <remarks>
    /// For setup rules the other <c>Require</c> methods cannot express, such as a reference that is assigned but of the
    /// wrong type. Not meant for errors in the game's own logic (an unknown key, an unexpected state), which should not
    /// stop a session.
    ///
    /// The error is logged before stopping so it reaches the Console (or <c>adb logcat</c> on the Quest).
    /// <see cref="Application.Quit()"/> only takes effect at the end of the frame, so the caller still gets false back and
    /// should stop early instead of using the missing dependency in the meantime.
    /// </remarks>
    /// <param name="condition">The condition that must hold.</param>
    /// <param name="message">The error to log when it does not.</param>
    /// <param name="context">Object selected when the log is clicked in the Console.</param>
    /// <returns><paramref name="condition"/> itself.</returns>
    public static bool RequireThat(bool condition, string message, UnityEngine.Object context)
    {
        if (condition)
        {
            return true;
        }

        Debug.LogError(message, context);

#if UNITY_EDITOR
        Debug.Break();
#else
        Application.Quit();
#endif

        return false;
    }

    /// <summary>Error message for an Inspector field left unassigned.</summary>
    private static string NotAssignedMessage(string fieldName, UnityEngine.Object owner)
    {
        return $"{fieldName} was not assigned in the Inspector of {owner.GetType().Name} on '{owner.name}'.";
    }

    /// <summary>Error message for an Inspector array, list or curve left empty.</summary>
    private static string EmptyMessage(string fieldName, UnityEngine.Object owner)
    {
        return $"{fieldName} is empty in the Inspector of {owner.GetType().Name} on '{owner.name}'.";
    }
}
