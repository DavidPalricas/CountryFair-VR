using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The wrist-menu counterpart of a <see cref="MiniGameTent"/>: a draggable card showing the tent's
/// name, slot number and mini-game sprite, kept in sync with the world tent via <see cref="TentPlaceHolderManager"/>.
/// </summary>
public class TentPanel : OrderableTentElement
{
    /// <summary>Displays the mini-game's Portuguese name, set from <see cref="OrderableTentElement.MiniGame"/> on Awake.</summary>
    [SerializeField]
    public TextMeshProUGUI TentNameText = null;

    /// <summary>Displays the numbered ribbon badge matching this panel's current placeholder slot.</summary>
    [SerializeField]
    private TextMeshProUGUI _tentNumberText = null;

    /// <summary>Image component whose sprite is set to <see cref="_miniGameSprite"/> on Awake.</summary>
    [SerializeField]
    private Image _tentImage = null;

    /// <summary>Icon representing this panel's mini-game.</summary>
    [SerializeField]
    private Sprite _miniGameSprite = null;

    /// <summary>Validates required references, applies the mini-game sprite and slot number, and sets the panel's name text.</summary>
    protected override void Awake()
    {
        base.Awake();

        if (!Utils.RequireAssigned(TentNameText, nameof(TentNameText), this) ||
            !Utils.RequireAssigned(_tentNumberText, nameof(_tentNumberText), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_tentImage, nameof(_tentImage), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_miniGameSprite, nameof(_miniGameSprite), this))
        {
            return;
        }

        _tentImage.sprite = _miniGameSprite;

        _tentNumberText.text = _currentPlaceHolder.Number.ToString();

        SetTentName();
    }


    /// <summary>Sets <see cref="TentNameText"/> to the Portuguese display name for <see cref="OrderableTentElement.MiniGame"/>.</summary>
    private void SetTentName()
    {
        string tentName = "Tenda de ";

        switch (MiniGame)
        {   
            case GameManager.MiniGames.Fishing:
                tentName += " pesca";
                break;

            case GameManager.MiniGames.Archery:
                tentName += " arco e flecha";
                break;

            case GameManager.MiniGames.Frisbee:
                tentName += "frisbee";
                break;

            case GameManager.MiniGames.DuckGame:
                tentName += "jogo do pato";
                break;

            default:
                Debug.LogError("Invalide MiniGame");
                return;
        }

        TentNameText.text = tentName;
    }

     /// <summary>Teleports this panel to its placeholder slot and refreshes the ribbon number to match.</summary>
     public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        _tentNumberText.text = _currentPlaceHolder.Number.ToString();
    }

    /// <summary>Raises the selection event on grab; schedules a snap back to the placeholder slot on release.</summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the panel.</param>
    /// <remarks>Invoked via the Inspector in the wrist menu panel's drag event.</remarks>
    public override void HandleGrab(bool isGrabbed)
    {
        if (!isGrabbed)
        {
            StartCoroutine(SnapToPlaceHolderNextFixedUpdate());

            return;
        }

       _onElementSelectionChanged.Invoke(true, this);
  }
}
