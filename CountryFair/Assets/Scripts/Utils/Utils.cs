using UnityEngine;
using System;

/// <summary>
/// Utility class providing helper methods for common game mechanics and calculations.
/// This static class contains reusable functionality for ray casting, random number generation,
/// and other utility operations used throughout the Country Fair VR game.
/// </summary>
public static class Utils
{
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
    /// Looks the object up by tag on every call, so avoid calling it many times per frame.
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
        GameObject ovrCameraRig = GameObject.FindGameObjectWithTag("MainCamera");

       /*
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig not found in the scene. Make sure you have an OVRCameraRig in your scene to use CastRayMetaQuest.");
            return new Ray();
        }


        Transform centerEyeTransform = ovrCameraRig.GetComponent<OVRCameraRig>().centerEyeAnchor;

        return new Ray(centerEyeTransform.position, centerEyeTransform.forward);

        */

        return new Ray(ovrCameraRig.transform.position, ovrCameraRig.transform.forward);
    }

    /// <summary>
    /// Returns a random float in [<paramref name="min"/>, <paramref name="max"/>], reseeding Unity's random generator
    /// with the system tick count (milliseconds) before every call.
    /// </summary>
    /// <remarks>
    /// The reseeding was meant to avoid predictable patterns, but it has two side effects worth knowing:
    /// <list type="bullet">
    /// <item><description>Calls made within the same millisecond (e.g. several in one frame) get the same seed and
    /// therefore return the same value.</description></item>
    /// <item><description><see cref="UnityEngine.Random.InitState"/> resets the global generator, so it also changes the
    /// sequence seen by every other <see cref="UnityEngine.Random"/> caller.</description></item>
    /// </list>
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

        int seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed);

        return UnityEngine.Random.Range(min, max);
    }


    /// <summary>
    /// Returns a random int in [<paramref name="min"/>, <paramref name="max"/>) — <b>max is exclusive</b>, like
    /// <see cref="UnityEngine.Random.Range(int, int)"/> — reseeding Unity's random generator with the system tick count
    /// before every call.
    /// </summary>
    /// <remarks>
    /// Because max is exclusive, it can be called directly with a collection's length to get a valid index.
    /// Same reseeding side effects as the float overload: calls in the same millisecond return the same value, and the
    /// global <see cref="UnityEngine.Random"/> state is reset.
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

        int seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed);

        return UnityEngine.Random.Range(min, max);
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
