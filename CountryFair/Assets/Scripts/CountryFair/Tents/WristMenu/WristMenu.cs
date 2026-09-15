using UnityEngine;

/// <summary>
/// Wrist-menu root that hosts the <see cref="TentPanel"/> reordering UI. Stays disabled until the
/// intro dialogue finishes, and toggles the menu panel open/closed on button press.
/// </summary>
public class WristMenu : MonoBehaviour
{
    /// <summary>The menu panel GameObject shown or hidden when the wrist button is pressed.</summary>
    [SerializeField]
    private GameObject _menu = null;

    /// <summary>Whether the menu panel is currently shown.</summary>
    private bool _menuActive = false;

    /// <summary>Validates the menu reference and hides both the panel and this whole wrist menu until the intro completes.</summary>
    private void Awake()
    {
        if (_menu == null)
        {
            Debug.LogError("Menu reference is null in TentPersonalizationMenu");

            return;
        }

        _menu.SetActive(_menuActive);
        gameObject.SetActive(false);
    }

    /// <summary>Toggles the menu panel open or closed.</summary>
    /// <remarks>Invoked via the Inspector in the wrist menu button's OnClick event.</remarks>
    public void ButtonClicked()
    {
        _menuActive = !_menuActive;

        _menu.SetActive(_menuActive);
    }

    /// <summary>Enables this wrist menu once the intro dialogue has finished.</summary>
    /// <remarks>Invoked via the Inspector in the <c>playerFinishedIntro</c> UnityEvent on <see cref="CountryFairDialogue"/>.</remarks>
    public void IntroCompleted()
    {
        gameObject.SetActive(true);
    }
}
