using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An integer parameter changed in fixed steps by a <c>-</c> / <c>+</c> button pair and clamped to [min, max].
/// </summary>
[Serializable]
public class SessionClampedParameter
{
    /// <summary>Lowest allowed value.</summary>
    public int min;

    /// <summary>Highest allowed value.</summary>
    public int max;

    /// <summary>Amount added or removed by each button press.</summary>
    public int step = 1;

    /// <summary>Value set when the panel starts.</summary>
    public int defaultValue;

    /// <summary>Button that decreases the value by <see cref="step"/>.</summary>
    public Button decreaseButton;

    /// <summary>Button that increases the value by <see cref="step"/>.</summary>
    public Button increaseButton;

    /// <summary>Text that displays the current value.</summary>
    public TextMeshProUGUI valueText;

    /// <summary>Current value, always within [min, max].</summary>
    public int Value { get; private set; }

    public SessionClampedParameter(int min, int max, int step, int defaultValue)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultValue = defaultValue;
    }

    /// <summary>Clamps and stores the value, refreshes its text and disables the button that would leave the range.</summary>
    /// <param name="value">The requested value.</param>
    public void Set(int value)
    {
        Value = Mathf.Clamp(value, min, max);

        valueText.text = Value.ToString();
        SetButtonEnabled(decreaseButton, Value > min);
        SetButtonEnabled(increaseButton, Value < max);
    }

    /// <summary>Opacity of a button (background and text) while it is disabled.</summary>
    private const float DisabledButtonAlpha = 0.35f;

    /// <summary>
    /// Enables or disables a button and fades it out while disabled. The button's own color tint only affects its
    /// background, so a <see cref="CanvasGroup"/> is used to also fade the <c>-</c> / <c>+</c> text.
    /// </summary>
    private static void SetButtonEnabled(Button button, bool isEnabled)
    {
        button.interactable = isEnabled;

        if (!button.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isEnabled ? 1f : DisabledButtonAlpha;
    }

    /// <summary>Increases the value by one step.</summary>
    public void Increase() => Set(Value + step);

    /// <summary>Decreases the value by one step.</summary>
    public void Decrease() => Set(Value - step);

    /// <summary>Keeps the Inspector values consistent: positive step, min ≤ max and the default inside the range.</summary>
    public void Validate()
    {
        step = Mathf.Max(1, step);
        max = Mathf.Max(min, max);
        defaultValue = Mathf.Clamp(defaultValue, min, max);
    }
}
