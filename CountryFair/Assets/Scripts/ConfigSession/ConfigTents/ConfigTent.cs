using TMPro;
using UnityEngine;

/// <summary>
/// A tent in the session configuration scene. The healthcare professional / patient Distance Grabs it and
/// drops it on a <see cref="TentPlaceHolder"/> to reorder the fair's mini-games.
///
/// Ordering (placeholder tracking, snap, selection event) comes from <see cref="OrderableTentElement"/>;
/// the tent text and mini-game prop come from the <see cref="TentDisplay"/> on the same GameObject.
/// Unlike <see cref="MiniGameTent"/>, it has no play button.
/// </summary>
[RequireComponent(typeof(TentDisplay))]
public class ConfigTent : OrderableTentElement
{
    [Header("Text elements")]
    /// <summary>Displays the numbered ribbon badge assigned by <see cref="TentPlaceHolder.Number"/>.</summary>
    [SerializeField]
    private TextMeshProUGUI _tentNumber;

    [Header("Tent Objects")]
    /// <summary>Ribbon decoration that hosts the tent number badge.</summary>
    [SerializeField]
    private GameObject _ribbon;

    /// <summary>Dropdown entry representing this tent, hidden while it is grabbed.</summary>
    [SerializeField]
    private GameObject _dropdownItem;

    /// <summary>Text and prop of this tent.</summary>
    private TentDisplay _tentDisplay;

    /// <summary>
    /// Initialises the tent display, validates required references and snaps the tent to its starting placeholder.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _tentDisplay = GetComponent<TentDisplay>();
        _tentDisplay.Initialize($"Jogo {GameManager.GetMiniGameName(MiniGame)}");

        if (!Utils.RequireAssigned(_tentNumber, nameof(_tentNumber), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_ribbon, nameof(_ribbon), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_dropdownItem, nameof(_dropdownItem), this))
        {
            return;
        }

        // base.Awake() already logged the error if the starting placeholder is missing.
        if (_currentPlaceHolder == null)
        {
            return;
        }

        SnapToCurrentPlaceHolder();
    }

    /// <summary>Toggles between the selected (grabbed) and unselected visual states.</summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the tent.</param>
    /// <remarks>Invoked via Inspector by the OnSelectEntered/OnSelectExited events of this tent's grab interactable.</remarks>
    public override void HandleGrab(bool isGrabbed)
    {
        if (isGrabbed)
        {
            TentSelected();
            return;
        }

        TentUnselected();
    }

    /// <summary>
    /// Teleports this tent to the current placeholder, refreshes the ribbon number and records the placeholder
    /// as the fallback for the next drag.
    /// </summary>
    public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        SetTentNumber(_currentPlaceHolder.Number);

        _previousPlaceHolder = _currentPlaceHolder;
    }

    /// <summary>Hides the tent's UI and notifies <see cref="TentPlaceHolderManager"/> that this tent was picked up.</summary>
    private void TentSelected()
    {
        SetTentUIVisible(false);

        _onElementSelectionChanged.Invoke(true, this);
    }

    /// <summary>
    /// Restores the tent's UI and schedules a snap to the current placeholder after the physics release frame
    /// (the base coroutine then notifies <see cref="TentPlaceHolderManager"/>).
    /// </summary>
    private void TentUnselected()
    {
        SetTentUIVisible(true);

        StartCoroutine(SnapToPlaceHolderNextFixedUpdate());
    }

    /// <summary>Shows or hides the text, ribbon, number badge and dropdown entry together.</summary>
    private void SetTentUIVisible(bool isVisible)
    {
        _tentDisplay.SetTextVisible(isVisible);

        _ribbon.SetActive(isVisible);
        _tentNumber.gameObject.SetActive(isVisible);
        _dropdownItem.SetActive(isVisible);
    }

    /// <summary>Updates the ribbon badge text to the given slot number.</summary>
    private void SetTentNumber(int number)
    {
        _tentNumber.text = number.ToString();
    }
}
