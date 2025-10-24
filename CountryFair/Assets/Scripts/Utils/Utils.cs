using UnityEngine;
using System;

public static class Utils
{   
    /// <summary>  
    /// The CastRayFromUI method is responsible for casting a ray from the UI to the game world.  
    /// This method is almost used to cast a ray from the player's crosshair to the game world.  
    /// </summary>  
    /// <param name="uiElement">The uiElement where the ray will be casted.</param>  
    /// <returns>A Ray object representing the ray cast from the UI element to the game world.</returns>  
    public static Ray CastRayFromUI(RectTransform uiElement)
    {
        Vector2 uiElementScreenPos = RectTransformUtility.WorldToScreenPoint(
            null,
            uiElement.position
        );

        return Camera.main.ScreenPointToRay(uiElementScreenPos);
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
