using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clinical file of a stroke survivor used in the session configuration user test: who the survivor is, the material
/// shown on the <see cref="ClipBoard"/> and the expected configuration of every tent.
///
/// The data mirrors the markdown files in <c>ConfigSessionStrokeSurvivorsFiles/</c>.
/// </summary>
[CreateAssetMenu(fileName = "StrokeSurvivorProfile", menuName = "ConfigSession/Stroke Survivor Profile")]
public class StrokeSurvivorProfile : ScriptableObject
{
    /// <summary>Name of the survivor.</summary>
    public string survivorName;

    /// <summary>Age of the survivor, in years.</summary>
    public int age;

    /// <summary>True if the affected arm is the left one, false if it is the right one.</summary>
    public bool isLeftArmAffected;

    /// <summary>Material with the survivor's file, applied to the <see cref="ClipBoard"/>.</summary>
    public Material fileMaterial;

    /// <summary>Expected configuration of every tent (the <see cref="TentConfigData.IsLeftArmAffected"/> field is ignored, see <see cref="isLeftArmAffected"/>).</summary>
    public TentConfigData[] tents;

    /// <summary>
    /// Checks whether the current tents configuration matches this survivor's file.
    /// </summary>
    /// <param name="current">Current configuration of each tent, by mini-game.</param>
    /// <param name="mismatch">Description of the first difference found, or null if everything matches.</param>
    /// <returns>True if every tent matches the expected configuration.</returns>
    public bool Matches(IReadOnlyDictionary<GameManager.MiniGames, TentConfigData> current, out string mismatch)
    {
        foreach (TentConfigData expected in tents)
        {
            if (!current.TryGetValue(expected.MiniGame, out TentConfigData actual))
            {
                mismatch = $"{expected.MiniGame}: tent not reported";
                return false;
            }

            mismatch = GetMismatch(expected, actual);

            if (mismatch != null)
            {
                return false;
            }
        }

        mismatch = null;
        return true;
    }

    /// <summary>Returns a description of the first field of <paramref name="actual"/> that differs from <paramref name="expected"/>, or null if none does.</summary>
    private string GetMismatch(TentConfigData expected, TentConfigData actual)
    {
        string tent = expected.MiniGame.ToString();

        if (actual.Order != expected.Order)
        {
            return $"{tent}: order {actual.Order}, expected {expected.Order}";
        }

        if (actual.Repetitions != expected.Repetitions)
        {
            return $"{tent}: repetitions {actual.Repetitions}, expected {expected.Repetitions}";
        }

        if (actual.SessionTimeMinutes != expected.SessionTimeMinutes)
        {
            return $"{tent}: session time {actual.SessionTimeMinutes}, expected {expected.SessionTimeMinutes}";
        }

        if (actual.DifficultyChange != expected.DifficultyChange)
        {
            return $"{tent}: difficulty {actual.DifficultyChange}, expected {expected.DifficultyChange}";
        }

        if (actual.IsLeftArmAffected != isLeftArmAffected)
        {
            return $"{tent}: left arm affected {actual.IsLeftArmAffected}, expected {isLeftArmAffected}";
        }

        return null;
    }
}
