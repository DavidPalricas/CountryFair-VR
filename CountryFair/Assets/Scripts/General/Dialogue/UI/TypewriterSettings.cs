using System;
using UnityEngine;

/// <summary>Inspector-tunable timings of the <see cref="TypewriterEffect"/>.</summary>
/// <remarks>
/// Pause defaults are intentionally low: there is no evidence yet of ideal values for this population
/// (post-stroke patients), so they are a conservative starting point to be adjusted in user tests.
/// </remarks>
[Serializable]
public class TypewriterSettings
{
    /// <summary>Base reveal speed, in characters per second. Values &lt;= 0 show lines instantly.</summary>
    [Min(0f)]
    public float charactersPerSecond = 35f;

    /// <summary>Extra pause (seconds) after a short-break mark: <c>, ; :</c></summary>
    [Min(0f)]
    public float shortPauseSeconds = 0.08f;

    /// <summary>Extra pause (seconds) after a sentence-ending mark: <c>. ! ? …</c></summary>
    [Min(0f)]
    public float longPauseSeconds = 0.2f;

    /// <summary>Characters that trigger <see cref="shortPauseSeconds"/>.</summary>
    public string shortPauseCharacters = ",;:";

    /// <summary>Characters that trigger <see cref="longPauseSeconds"/>.</summary>
    public string longPauseCharacters = ".!?…";

    /// <summary>
    /// Fraction of the narration clip length the reveal is fitted to (only for lines with narration). Below 1 the text
    /// finishes before the voice, so the reader is never left waiting for the text, and it absorbs any silence left at
    /// the end of the clip. 1 = text and clip end together.
    /// </summary>
    [Range(0.5f, 1f)]
    public float narrationDurationScale = 0.7f;
}
