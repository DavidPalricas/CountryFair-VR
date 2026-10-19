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

    /// <summary>Displays the numbered ribbon badge assigned by <see cref="TentPlaceHolder.Number"/>.</summary>
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

    /// <summary>Text displayed on the tent panel; derived from <see cref="OrderableTentElement.MiniGame"/> and applied to <see cref="_tentText"/> on Awake.</summary>
    private string _textToShow = string.Empty;

    /// <summary>The decorative mini-game prop instance spawned above this tent.</summary>
    private GameObject _miniGameProp;

     /// <summary>True while the player is currently grabbing this tent; suppresses the ray check while grabbed.</summary>
     private bool _isSelected = false;

    /// <summary>
    /// Furthest distance (meters) at which looking at a tent shows its play button. Bounds the gaze raycast, which
    /// previously ran to infinity.
    /// </summary>
    private const float MaxGazeDistance = 20f;

    /// <summary>
    /// <see cref="Time.frameCount"/> of the last gaze raycast. Shared by every tent so the four tents do one raycast per
    /// frame between them instead of one each.
    /// </summary>
    private static int _gazeFrame = -1;

    /// <summary>Collider hit by the current frame's gaze raycast, or null if it hit nothing within <see cref="MaxGazeDistance"/>.</summary>
    private static Collider _gazedCollider;

    /// <summary>
    /// Initialises UI text, validates required references, snaps the tent to its starting placeholder,
    /// and hides the play button until the player aims at the tent.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _textToShow = GetTextToShow(MiniGame);
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

        SetTentNumber(_currentPlaceHolder.Number);

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

    /// <summary>
    /// Shows the play button only when the player's gaze ray is currently hitting this tent's collider. Keeps the
    /// current state when the ray hits nothing, and only calls <c>SetActive</c> when the state actually changes.
    /// </summary>
    private void CheckIfPlayerWantsToGoToMiniGame()
    {
        Collider gazedCollider = GetGazedCollider();

        if (gazedCollider == null)
        {
            return;
        }

        bool isToShowData = gazedCollider == _collider;

        if (_buttonToPlayMiniGame.activeSelf != isToShowData)
        {
            _buttonToPlayMiniGame.SetActive(isToShowData);
        }
    }

    /// <summary>
    /// Returns the collider the player is looking at this frame. The first tent to ask in a frame does the raycast;
    /// the others reuse its result.
    /// </summary>
    private static Collider GetGazedCollider()
    {
        if (_gazeFrame != Time.frameCount)
        {
            _gazeFrame = Time.frameCount;

            _gazedCollider = Physics.Raycast(Utils.CastRayMetaQuest(), out RaycastHit hitInfo, MaxGazeDistance)
                ? hitInfo.collider
                : null;
        }

        return _gazedCollider;
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
    private static string GetTextToShow(GameManager.MiniGames miniGame)
    {
        switch (miniGame)
        {
            case GameManager.MiniGames.Archery:
                return "Carregue no botao para jogar arco e flecha";

            case GameManager.MiniGames.DuckGame:
                return "Carregue no botao para jogar o jogo do pato";

            case GameManager.MiniGames.Fishing:
                return "Carregue no botao para pescar";

            case GameManager.MiniGames.Frisbee:
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

        _onElementSelectionChanged.Invoke(true, this);
    }

    /// <summary>
    /// Restores UI elements and schedules a snap to the current placeholder after the physics release frame.
    /// </summary>
    private void TentUnselected()
    {
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
    /// <remarks>Invoked via Inspector by the OnSelectEntered/OnSelectExited events of this tent's XR Grab Interactable component.</remarks>
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


    /// <summary>Loads this tent's mini-game scene via <see cref="GameManager.GoToMiniGame"/> (a warning is logged for mini-games without a scene).</summary>
    /// <remarks>Invoked via Inspector by the play button's OnClick event on the tent prefab.</remarks>
    public void PlayerWantsToGoToMiniGame()
    {
        GameManager.GetInstance().GoToMiniGame(MiniGame);
    }

        /// <summary>
    /// Teleports this tent and its play button to the current placeholder's position and rotation,
    /// and refreshes the ribbon number.
    /// </summary>
    public override void SnapToCurrentPlaceHolder()
    {
        base.SnapToCurrentPlaceHolder();

        MiniGameTentPlaceHolder currentTentPlaceHolder = _currentPlaceHolder as MiniGameTentPlaceHolder;

        if (currentTentPlaceHolder == null)
        {
            Debug.LogError($"Current PlaceHolder '{_currentPlaceHolder.name}' is not a TentPlaceHolder.");
            return;
        }

        Transform buttonToPlayMiniGamePlaceHolderTransform = currentTentPlaceHolder.MiniGameButtonPlaceHolderTransform;

        _buttonToPlayMiniGame.transform.SetPositionAndRotation(buttonToPlayMiniGamePlaceHolderTransform.position, buttonToPlayMiniGamePlaceHolderTransform.rotation);

        SetTentNumber(_currentPlaceHolder.Number);
        _previousPlaceHolder = _currentPlaceHolder;

    }
}
