using TMPro;
using UnityEngine;

/// <summary>
/// <see cref="TMP_Dropdown"/> that always opens below its button and lists only the options that are not selected.
///
/// <see cref="TMP_Dropdown.Show"/> flips the list above the button whenever it does not fit inside the root canvas.
/// In the session configuration scene the root canvas is the tent's canvas, which the panel sticks out of, so the
/// list would always open upwards. Neither behaviour is configurable, so both are corrected in
/// <see cref="CreateBlocker"/>, the last hook <c>Show</c> calls after building and positioning the list.
/// </summary>
public class SessionParamDropdown : TMP_Dropdown
{
    /// <summary>The list created by the last <see cref="TMP_Dropdown.Show"/>.</summary>
    private GameObject _list;

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
