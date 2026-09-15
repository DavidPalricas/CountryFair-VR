using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The wrist-menu counterpart of a <see cref="MiniGameTent"/>: a draggable card showing the tent's
/// name, slot number and mini-game sprite, kept in sync with the world tent via <see cref="TentPlaceHolderManager"/>.
/// </summary>
public class TentPanel : OrderableTentElement
{
    /// <summary>Displays the mini-game's Portuguese name, set from <see cref="OrderableTentElement.miniGame"/> on Awake.</summary>
    [SerializeField]
    public TextMeshProUGUI tentNameText = null;

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

        if (tentNameText == null || _tentNumberText == null)
        {
            Debug.LogError("One or more Texts Components are null in TentPanel");

            return;
        }

        if (_tentImage == null)
        {
            Debug.LogError("Tent Image Component is null in TentPanel");

            return;
        }

        if (_miniGameSprite == null)
        {
            Debug.LogError("MiniGame Sprite is null in TentPanel");

            return;
        }

        _tentImage.sprite = _miniGameSprite;

        _tentNumberText.text = currentPlaceHolder.number.ToString();

        SetTentName();
    }


    /// <summary>Sets <see cref="tentNameText"/> to the Portuguese display name for <see cref="OrderableTentElement.miniGame"/>.</summary>
    private void SetTentName()
    {
        string tentName = "Tenda de ";

        switch (miniGame)
        {   
            case MINI_GAMES.FISHING:
                tentName += " pesca";
                break;

            case MINI_GAMES.ARCHERY:
                tentName += " arco e flecha";
                break;

            case MINI_GAMES.FRISBEE:
                tentName += "frisbee";
                break;

            case MINI_GAMES.DUCKGAME:
                tentName += "jogo do pato";
                break;

            default:
                Debug.LogError("Invalide MiniGame");
                return;
        }

        tentNameText.text = tentName;
    }

     /// <summary>Teleports this panel to its placeholder slot and refreshes the ribbon number to match.</summary>
     public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        _tentNumberText.text = currentPlaceHolder.number.ToString();
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

       OnElementSelectionChanged.Invoke(true, this);
  }
}
