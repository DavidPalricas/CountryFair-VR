using TMPro;
using UnityEngine;

/// <summary>
/// Visual part shared by every tent in the world (<see cref="MiniGameTent"/> in the mini-game scene and
/// <see cref="ConfigTent"/> in the session configuration scene): the tent's descriptive text and the decorative
/// mini-game prop spawned above it.
///
/// It does not know which mini-game or kind of tent it belongs to: the owning tent builds the text and passes
/// it to <see cref="Initialize"/>.
/// </summary>
public class TentDisplay : MonoBehaviour
{
    [Header("Text elements")]
    /// <summary>Displays the tent's descriptive text label.</summary>
    [SerializeField]
    private TextMeshProUGUI _tentText;

    [Header("Mini-game prop")]
    /// <summary>Prefab for the decorative mini-game prop spawned above this tent.</summary>
    [SerializeField]
    private GameObject _miniGamePropPrefab;

    /// <summary>Anchor used when spawning <see cref="_miniGamePropPrefab"/> above the tent.</summary>
    [SerializeField]
    private Transform _miniGamePropPlaceHolderTransform;

    /// <summary>The decorative mini-game prop instance spawned above this tent.</summary>
    private GameObject _miniGameProp;

    /// <summary>
    /// Shows <paramref name="text"/> on the tent and spawns the mini-game prop.
    /// Called once by the owning tent in its <c>Awake</c>; the owner decides the text
    /// (play prompt on a <see cref="MiniGameTent"/>, game name on a <see cref="ConfigTent"/>).
    /// </summary>
    public void Initialize(string text)
    {
        if (!Utils.RequireAssigned(_tentText, nameof(_tentText), this))
        {
            return;
        }

        _tentText.text = text;

        if (!Utils.RequireAssigned(_miniGamePropPrefab, nameof(_miniGamePropPrefab), this))
        {
            return;
        }

        if (!Utils.RequireAssigned(_miniGamePropPlaceHolderTransform, nameof(_miniGamePropPlaceHolderTransform), this))
        {
            return;
        }

        AddMiniGameProp();
    }

    /// <summary>Shows or hides the tent text (e.g. while the tent is being grabbed).</summary>
    public void SetTextVisible(bool isVisible)
    {
        _tentText.gameObject.SetActive(isVisible);
    }

    /// <summary>Instantiates <see cref="_miniGamePropPrefab"/> slightly above the placeholder anchor and parents it to this tent.</summary>
    private void AddMiniGameProp()
    {
        _miniGameProp = Instantiate(
            _miniGamePropPrefab,
            _miniGamePropPlaceHolderTransform.position + _miniGamePropPlaceHolderTransform.up * 0.1f,
            _miniGamePropPrefab.transform.rotation
        );

        _miniGameProp.transform.parent = transform;
    }
}
