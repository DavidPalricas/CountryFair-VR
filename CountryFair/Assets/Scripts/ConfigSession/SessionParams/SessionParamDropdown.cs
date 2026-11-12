using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <see cref="TMP_Dropdown"/> that always opens below its button and lists only the options that are not selected.
///
/// <see cref="TMP_Dropdown.Show"/> flips the list above the button whenever it does not fit inside the root canvas.
/// In the session configuration scene the root canvas is the tent's canvas, which the panel sticks out of, so the
/// list would always open upwards. Neither behaviour is configurable, so both are corrected in
/// <see cref="CreateBlocker"/>, the last hook <c>Show</c> calls after building and positioning the list.
///
/// For two-option dropdowns it also emits <see cref="_isFirstOptionSelected"/>, a <see cref="bool"/> event, in addition
/// to the usual <see cref="TMP_Dropdown.onValueChanged"/> (an <see cref="int"/> index).
/// </summary>
public class SessionParamDropdown : TMP_Dropdown
{
    /// <summary>
    /// Raised when the user picks an option: <c>true</c> if it is the first one, <c>false</c> otherwise. Wire it in the
    /// Inspector with the dynamic-bool variant of the target method.
    /// </summary>
    [SerializeField]
    private UnityEvent<bool> _isFirstOptionSelected;

    /// <summary>The list created by the last <see cref="TMP_Dropdown.Show"/>.</summary>
    private GameObject _list;

    /// <summary>Starts forwarding the selected option to <see cref="_isFirstOptionSelected"/>.</summary>
    protected override void Awake()
    {
        base.Awake();

        onValueChanged.AddListener(NotifyFirstOptionSelected);
    }

    /// <summary>Stops forwarding the selected option.</summary>
    protected override void OnDestroy()
    {
        onValueChanged.RemoveListener(NotifyFirstOptionSelected);

        base.OnDestroy();
    }

    /// <summary>Emits whether the selected option is the first one.</summary>
    private void NotifyFirstOptionSelected(int optionIndex)
    {
        _isFirstOptionSelected?.Invoke(optionIndex == 0);
    }

    /// <summary>
    /// Keeps a reference to the list being shown.
    ///
    /// Each item of the template carries its own ray interaction whose select event sets the item's
    /// <see cref="UnityEngine.UI.Toggle"/> on, which makes <see cref="TMP_Dropdown"/> pick the option and close the
    /// list. That path does not depend on <c>PointableCanvasModule</c>, which would ignore clicks on this list because
    /// its canvas is nested (the module only accepts hits whose root canvas is the one that received the ray).
    /// </summary>
    protected override GameObject CreateDropdownList(GameObject template)
    {
        _list = base.CreateDropdownList(template);

        return _list;
    }

    /// <summary>Hides the selected option and moves the list back below the button before it is displayed.</summary>
    protected override GameObject CreateBlocker(Canvas rootCanvas)
    {
        if (_list != null)
        {
            HideSelectedItem();
            KeepBelowButton();
        }

        return base.CreateBlocker(rootCanvas);
    }

    /// <summary>Undoes the flip of <see cref="TMP_Dropdown.Show"/> by restoring the template's anchoring.</summary>
    private void KeepBelowButton()
    {
        RectTransform listRect = (RectTransform)_list.transform;

        listRect.anchorMin = template.anchorMin;
        listRect.anchorMax = template.anchorMax;
        listRect.pivot = template.pivot;
        listRect.anchoredPosition = template.anchoredPosition;
    }

    /// <summary>
    /// Hides the item of the selected option, closes the gap it leaves and shrinks the list by one item.
    /// The items are laid out from the bottom of the content, so only the items above the hidden one move.
    /// </summary>
    private void HideSelectedItem()
    {
        if (MultiSelect || options.Count < 2)
        {
            return;
        }

        // Only the created items are active at this point (Show disables the item template before calling CreateBlocker),
        // in option order.
        DropdownItem[] items = _list.GetComponentsInChildren<DropdownItem>(false);

        if (value < 0 || value >= items.Length)
        {
            return;
        }

        RectTransform hiddenRect = items[value].rectTransform;
        float itemHeight = hiddenRect.rect.height;

        items[value].gameObject.SetActive(false);

        for (int i = 0; i < value; i++)
        {
            items[i].rectTransform.anchoredPosition -= new Vector2(0f, itemHeight);
        }

        RectTransform contentRect = (RectTransform)hiddenRect.parent;
        RectTransform listRect = (RectTransform)_list.transform;
        float listMargin = listRect.rect.height - contentRect.rect.height;

        contentRect.sizeDelta -= new Vector2(0f, itemHeight);

        // Show only shrinks the list to fit its content when the template is taller; keep that fit.
        if (listMargin >= 0f)
        {
            listRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRect.rect.height + listMargin);
        }
    }
}
