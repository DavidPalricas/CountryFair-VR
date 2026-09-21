using TMPro;
using UnityEngine;


/// <summary>
/// Manages the display and interaction of tent information in the Country Fair VR game.
///
/// This script is responsible for:
/// - Displaying tent UI panels when the user gazes at or points at a tent using the Meta Quest 3 ray interaction
/// - Showing associated mini-game preview objects for each tent
/// - Handling transitions to mini-game scenes when the user selects a tent
/// - Snapping the tent to a placeholder position when released after a Distance Grab
/// </summary>

public class MiniGameTent : OrderableTentElement
{
    [Header("Text elements")]
    /// <summary>Displays the tent's descriptive text label.</summary>
    [SerializeField]
    private TextMeshProUGUI _tentText;

    /// <summary>Displays the numbered ribbon badge assigned by <see cref="TentPlaceHolder.number"/>.</summary>
    [SerializeField]
    private TextMeshProUGUI _tentNumber;

    /// <summary>Dropdown entry representing this tent, hidden while it is grabbed.</summary>
    [SerializeField]
    private GameObject _dropdownItem;

    [Header("PlaceHolders")]

    /// <summary>Anchor used when spawning <see cref="_miniGamePropPrefab"/> above the tent.</summary>
    [SerializeField]
    private Transform _miniGamePropPlaceHolderTransform;

    [Header("Tent Objects")]
    /// <summary>Prefab for the decorative mini-game prop spawned above this tent on Start.</summary>
    [SerializeField]
    private GameObject _miniGamePropPrefab;

    /// <summary>Play button shown when the player's ray targets this tent.</summary>
    [SerializeField]
    private GameObject _buttonToPlayMiniGame;

    /// <summary>Ribbon decoration that hosts the tent number badge.</summary>
    [SerializeField]
    private GameObject _ribbon;

    /// <summary>Text displayed on the tent panel; derived from <see cref="OrderableTentElement.miniGame"/> and applied to <see cref="_tentText"/> on Awake.</summary>
    private string _textToShow = string.Empty;

    /// <summary>The decorative mini-game prop instance spawned above this tent.</summary>
    private GameObject _miniGameProp;

     /// <summary>True while the player is currently grabbing this tent; suppresses the ray check while grabbed.</summary>
     private bool _isSelected = false;

    /// <summary>
    /// Initialises UI text, validates required references, snaps the tent to its starting placeholder,
    /// and hides the play button until the player aims at the tent.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _textToShow = GetTextToShow(miniGame);
        _tentText.text = _textToShow;

        if (_buttonToPlayMiniGame == null)
        {
            Debug.LogError("Red Button is not assigned in MiniGameTent script.");
            return;
        }

        _buttonToPlayMiniGame.SetActive(false);

        if (_miniGamePropPrefab == null)
        {
            Debug.LogError("Mini Game Object is not assigned in MiniGameTent script.");
            return;
        }

        if (_tentNumber == null)
        {
            Debug.LogError("Ribbon Number Text is not assigned in MiniGameTent script.");
            return;
        }

        if (_miniGamePropPlaceHolderTransform == null)
        {
            Debug.LogError("One or more required transforms are not assigned in MiniGameTent script.");

            return;
        }

        SetTentNumber(currentPlaceHolder.number);

        SnapToCurrentPlaceHolder();

         AddMiniGameObject();
    }

    /// <summary>
    /// Casts a Meta Quest ray each frame and shows or hides <see cref="_buttonToPlayMiniGame"/>
    /// based on whether the ray hits this tent. Skipped while the tent is grabbed.
    /// </summary>
    private void LateUpdate()
    {
        if (!_isSelected)
        {
            CheckIfPlayerWantsToGoToMiniGame();
        }
    }

    /// <summary>Shows the play button only when the player's gaze/pointer ray is currently hitting this tent's collider.</summary>
    private void CheckIfPlayerWantsToGoToMiniGame()
    {
        Ray ray = Utils.CastRayMetaQuest();

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            bool isToShowData = hitInfo.collider == _collider;

            _buttonToPlayMiniGame.SetActive(isToShowData);
        }
    }

    /// <summary>Instantiates <see cref="_miniGamePropPrefab"/> slightly above the placeholder anchor and parents it to this tent.</summary>
    private void AddMiniGameObject()
    {
        _miniGameProp = Instantiate(
            _miniGamePropPrefab,
            _miniGamePropPlaceHolderTransform.position + _miniGamePropPlaceHolderTransform.up * 0.1f,
            _miniGamePropPrefab.transform.rotation
        );

        _miniGameProp.transform.parent = transform;
    }

    /// <summary>Returns the tent panel text for the given mini-game.</summary>
    private static string GetTextToShow(MINI_GAMES miniGame)
    {
        switch (miniGame)
        {
            case MINI_GAMES.ARCHERY:
                return "Carregue no botao para jogar arco e flecha";

            case MINI_GAMES.DUCKGAME:
                return "Carregue no botao para jogar o jogo do pato";

            case MINI_GAMES.FISHING:
                return "Carregue no botao para pescar";

            case MINI_GAMES.FRISBEE:
                return "Carregue no botao para jogar frisbee";

            default:
                Debug.LogWarning($"MiniGame '{miniGame}' has no tent text assigned.");
                return string.Empty;
        }
    }

    /// <summary>Updates the ribbon badge text to the given slot number.</summary>
    private void SetTentNumber(int number)
    {
        _tentNumber.text = number.ToString();
    }

    /// <summary>Hides UI elements and notifies <see cref="TentPlaceHolderManager"/> that this tent was picked up.</summary>
    private void TentSelected()
    {
        _buttonToPlayMiniGame.SetActive(false);

        _tentText.gameObject.SetActive(false);
        _tentNumber.gameObject.SetActive(false);
        _dropdownItem.SetActive(false);

        ToggleRibbonStuff(false);

        OnElementSelectionChanged.Invoke(true, this);
    }

    /// <summary>
    /// Restores UI elements and schedules a snap to the current placeholder after the physics release frame.
    /// </summary>
    private void TentUnselected()
    {
        Debug.Log("Tent Unselected");
        _buttonToPlayMiniGame.SetActive(true);

        _tentText.gameObject.SetActive(true);
        _tentNumber.gameObject.SetActive(true);
        _dropdownItem.SetActive(true);

        ToggleRibbonStuff(true);

        StartCoroutine(SnapToPlaceHolderNextFixedUpdate());
    }

    /// <summary>Shows or hides the ribbon decoration and its text together.</summary>
    private void ToggleRibbonStuff(bool isActive)
    {
        _ribbon.SetActive(isActive);
        _tentText.gameObject.SetActive(isActive);
    }

    /// <summary>Toggles between the selected (grabbed) and unselected visual states.</summary>
    /// <param name="isGrabbed">True when the grab begins; false when the player releases the tent.</param>
    /// <remarks>Invocado via Inspector nos eventos OnSelectEntered/OnSelectExited do componente XR Grab Interactable deste tent.</remarks>
    public override void HandleGrab(bool isGrabbed)
    {
       _isSelected = isGrabbed;

        if (isGrabbed)
        {
            TentSelected();
            return;
        }

        TentUnselected();
    }


    /// <summary>Inspector-wired entry point for the play button; loads this tent's mini-game scene via <see cref="GameManager.GoToMiniGame"/>.</summary>
    public void PlayerWantsToGoToMiniGame()
    {
        GameManager.GetInstance().GoToMiniGame(miniGame);
    }

        /// <summary>
    /// Teleports this tent and its play button to the current placeholder's position and rotation,
    /// and refreshes the ribbon number.
    /// </summary>
    public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        MiniGameTentPlaceHolder currentTentPlaceHolder = currentPlaceHolder as MiniGameTentPlaceHolder;

        if (currentTentPlaceHolder == null)
        {
            Debug.LogError($"Current PlaceHolder '{currentPlaceHolder.name}' is not a TentPlaceHolder.");
            return;
        }

        Transform buttonToPlayMiniGamePlaceHolderTransform = currentTentPlaceHolder.miniGameButtonPlaceHolderTransform;

        _buttonToPlayMiniGame.transform.SetPositionAndRotation(buttonToPlayMiniGamePlaceHolderTransform.position, buttonToPlayMiniGamePlaceHolderTransform.rotation);

        SetTentNumber(currentPlaceHolder.number);
        _previousPlaceHolder = currentPlaceHolder;

    }
}
