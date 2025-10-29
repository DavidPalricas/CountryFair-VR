using UnityEngine;
using System;


public static class Utils
{
    /// <summary>  
    /// The CastRayFromMeta method is responsible for casting a ray from the users's eyes in the Meta Quest 3 headset to the game world.
    /// </summary>  
    /// <returns> A Ray object representing the ray cast from the user's eyes to the game world.</returns>  
    public static Ray CastRayMetaQuest()
    {
        GameObject ovrCameraRig = GameObject.FindGameObjectWithTag("MainCamera");

        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig not found in the scene. Make sure you have an OVRCameraRig in your scene to use CastRayMetaQuest.");
            return new Ray();
        }

        Transform centerEyeTransform = ovrCameraRig.GetComponent<OVRCameraRig>().centerEyeAnchor;

        return new Ray(centerEyeTransform.position, centerEyeTransform.forward);
    }

    /// <summary>
    /// The RandomValueInRange method is responsible for generating a random float between a specified range.
    /// </summary>
    /// <remarks>
    /// This method is used instead of UnityEngine.Random.Range or System.Random.Next 
    /// to avoid some patterns in producing random numbers.
    /// Because computes cannot generate truly random numbers, they generate pseudo-random numbers using a seed.
    /// So, in this method, we are setting the seed to the current OS time in milliseconds 
    /// to avoid patterns in producing random numbers.
    /// </remarks>
    /// <param name="min">The minimum float value of the ranged specified.</param>
    /// <param name="max">The maximum float value of the range specified .</param>
    /// <returns>A random float between the specified range</returns>
    public static float RandomValueInRange(float min, float max)
    {   
        int seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed);

        return UnityEngine.Random.Range(min, max);
    }
}
