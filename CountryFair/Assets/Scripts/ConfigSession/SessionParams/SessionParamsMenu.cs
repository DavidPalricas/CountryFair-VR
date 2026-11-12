using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Session parameters panel of a <see cref="ConfigTent"/>, used through ray interaction in the session configuration scene.
///
/// Holds three parameters: the number of repetitions and the session time (each changed with a <c>-</c> / <c>+</c>
/// button pair and clamped to its range) and the affected arm (chosen in a dropdown). The current values are exposed
/// through <see cref="Repetitions"/>, <see cref="SessionTimeMinutes"/> and <see cref="IsLeftArmAffected"/>.
/// </summary>
public class SessionParamsMenu : MonoBehaviour
{
    /// <summary>How the therapist wants the game difficulty to change in the session. The order matches the dropdown options.</summary>
    public enum DIFFICULTY_CHANGE
    {
        DECREASE,
        KEEP,
        INCREASE
    }

    /// <summary>Index of the "left arm" option in the affected arm dropdown (the other option is the right arm).</summary>
    private const int LeftArmOptionIndex = 0;

    /// <summary>Portuguese labels of the affected arm dropdown options; the left arm is at <see cref="LeftArmOptionIndex"/>.</summary>
    private static readonly List<string> AffectedArmOptions = new() { "Esquerdo", "Direito" };

    /// <summary>Portuguese labels of the dropdown options, indexed by <see cref="DIFFICULTY_CHANGE"/>.</summary>
    private static readonly List<string> DifficultyChangeOptions = new() { "Diminuir", "Manter", "Aumentar" };

    [Header("Parameters")]
    /// <summary>Number of repetitions (1 to 5).</summary>
    [SerializeField]
    private SessionClampedParameter _repetitions = new(1, 5, 1, 1);

    /// <summary>Session time in minutes (10 to 40, in steps of 10).</summary>
    [SerializeField]
    private SessionClampedParameter _sessionTime = new(10, 40, 10, 10);

    /// <summary>Dropdown where the affected arm is chosen; its options are filled in <see cref="Awake"/>.</summary>
    [SerializeField]
    private TMP_Dropdown _affectedArmDropdown;

    /// <summary>Dropdown where the game difficulty change is chosen; its options are filled in <see cref="Awake"/>.</summary>
    [SerializeField]
    private TMP_Dropdown _difficultyDropdown;

    /// <summary>Current number of repetitions.</summary>
    public int Repetitions => _repetitions.Value;

    /// <summary>Current session time, in minutes.</summary>
    public int SessionTimeMinutes => _sessionTime.Value;

    /// <summary>True if the affected arm is the left one, false if it is the right one.</summary>
    public bool IsLeftArmAffected { get; private set; } = true;

    /// <summary>Currently selected difficulty change (keeps the difficulty by default).</summary>
    public DIFFICULTY_CHANGE DifficultyChange { get; private set; } = DIFFICULTY_CHANGE.KEEP;

    /// <summary>Validates the references, sets the default values and fills the dropdown. Clicks are NOT listened to in code: the ray interaction events (OnSelect) are wired in the Inspector to the public methods below.</summary>
    private void Awake()
    {
        if (!AreParameterReferencesAssigned(_repetitions, nameof(_repetitions)) ||
            !AreParameterReferencesAssigned(_sessionTime, nameof(_sessionTime)) ||
            !Utils.RequireAssigned(_affectedArmDropdown, nameof(_affectedArmDropdown), this) ||
            !Utils.RequireAssigned(_difficultyDropdown, nameof(_difficultyDropdown), this))
        {
            return;
        }

        InitializeParameter(_repetitions);
        InitializeParameter(_sessionTime);

        _affectedArmDropdown.ClearOptions();
        _affectedArmDropdown.AddOptions(AffectedArmOptions);
        _affectedArmDropdown.SetValueWithoutNotify(IsLeftArmAffected ? LeftArmOptionIndex : 1 - LeftArmOptionIndex);

        _difficultyDropdown.ClearOptions();
        _difficultyDropdown.AddOptions(DifficultyChangeOptions);
        _difficultyDropdown.SetValueWithoutNotify((int)DifficultyChange);
    }

    /// <summary>Keeps the parameter ranges consistent while they are edited in the Inspector.</summary>
    private void OnValidate()
    {
        _repetitions?.Validate();
        _sessionTime?.Validate();
    }

    /// <summary>Sets the parameter to its default value.</summary>
    private static void InitializeParameter(SessionClampedParameter parameter)
    {
        parameter.Set(parameter.defaultValue);
    }

    /// <summary>Checks that the buttons and the value text of a parameter are assigned.</summary>
    private bool AreParameterReferencesAssigned(SessionClampedParameter parameter, string parameterName)
    {
        return Utils.RequireAssigned(parameter.decreaseButton, $"{parameterName}.{nameof(parameter.decreaseButton)}", this) &&
               Utils.RequireAssigned(parameter.increaseButton, $"{parameterName}.{nameof(parameter.increaseButton)}", this) &&
               Utils.RequireAssigned(parameter.valueText, $"{parameterName}.{nameof(parameter.valueText)}", this);
    }

    /// <summary>
    /// Inspector entry point of the <c>-</c> / <c>+</c> buttons of the repetitions parameter.
    /// </summary>
    /// <param name="increase">True to increase the value by one step, false to decrease it.</param>
    public void ChangeRepetitions(bool increase) => ChangeParameter(_repetitions, increase);

    /// <summary>
    /// Inspector entry point of the <c>-</c> / <c>+</c> buttons of the session time parameter.
    /// </summary>
    /// <param name="increase">True to increase the value by one step, false to decrease it.</param>
    public void ChangeSessionTime(bool increase) => ChangeParameter(_sessionTime, increase);

    /// <summary>Increases or decreases a stepped parameter.</summary>
    private static void ChangeParameter(SessionClampedParameter parameter, bool increase)
    {
        if (increase)
        {
            parameter.Increase();

            return;
        }
       
        parameter.Decrease();
    }

    /// <summary>
    /// Inspector entry point of the affected arm dropdown (wire it to the <c>Is First Option Selected (Boolean)</c> event
    /// of the <see cref="SessionParamDropdown"/>, using the dynamic-bool variant). The first option is the left arm.
    /// </summary>
    /// <param name="isLeft">True if the left arm was selected, false if the right arm was.</param>
    public void OnAffectedArmChanged(bool isLeft)
    {
        IsLeftArmAffected = isLeft;
    }

    /// <summary>
    /// Inspector entry point of the difficulty dropdown (wire it to its <c>On Value Changed (Int32)</c> event,
    /// using the dynamic-int variant).
    /// </summary>
    /// <param name="optionIndex">Index of the selected dropdown option.</param>
    public void OnDifficultyChanged(int optionIndex)
    {
        int lastOption = Enum.GetValues(typeof(DIFFICULTY_CHANGE)).Length - 1;

        DifficultyChange = (DIFFICULTY_CHANGE)Mathf.Clamp(optionIndex, 0, lastOption);
    }
}
