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
/// through <see cref="Repetitions"/>, <see cref="SessionTimeMinutes"/> and <see cref="AffectedArm"/>.
/// </summary>
public class SessionParamsMenu : MonoBehaviour
{
    /// <summary>Arm affected by the stroke. The order matches the dropdown options.</summary>
    public enum AFFECTED_ARM
    {
        LEFT,
        RIGHT
    }

    /// <summary>Portuguese labels of the dropdown options, indexed by <see cref="AFFECTED_ARM"/>.</summary>
    private static readonly List<string> AffectedArmOptions = new() { "Esquerdo", "Direito" };

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

    /// <summary>Current number of repetitions.</summary>
    public int Repetitions => _repetitions.Value;

    /// <summary>Current session time, in minutes.</summary>
    public int SessionTimeMinutes => _sessionTime.Value;

    /// <summary>Currently selected affected arm.</summary>
    public AFFECTED_ARM AffectedArm { get; private set; } = AFFECTED_ARM.LEFT;

    /// <summary>Validates the references, sets the default values and fills the dropdown. Clicks are NOT listened to in code: the ray interaction events (OnSelect) are wired in the Inspector to the public methods below.</summary>
    private void Awake()
    {
        if (!AreParameterReferencesAssigned(_repetitions, nameof(_repetitions)) ||
            !AreParameterReferencesAssigned(_sessionTime, nameof(_sessionTime)) ||
            !Utils.RequireAssigned(_affectedArmDropdown, nameof(_affectedArmDropdown), this))
        {
            return;
        }

        InitializeParameter(_repetitions);
        InitializeParameter(_sessionTime);

        _affectedArmDropdown.ClearOptions();
        _affectedArmDropdown.AddOptions(AffectedArmOptions);
        _affectedArmDropdown.SetValueWithoutNotify((int)AffectedArm);
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
    /// Inspector entry point of the affected arm dropdown (wire it to its <c>On Value Changed (Int32)</c> event,
    /// using the dynamic-int variant).
    /// </summary>
    /// <param name="optionIndex">Index of the selected dropdown option.</param>
    public void OnAffectedArmChanged(int optionIndex)
    {
        int lastArm = Enum.GetValues(typeof(AFFECTED_ARM)).Length - 1;

        AffectedArm = (AFFECTED_ARM)Mathf.Clamp(optionIndex, 0, lastArm);
    }
}
