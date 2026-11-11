using UnityEngine;

/// <summary>
/// A tent in the mini-game scene. Shows its play button while the player looks at it and loads the tent's
/// mini-game when the button is pressed.
///
/// It cannot be grabbed or reordered: ordering happens in the session configuration scene (<see cref="ConfigTent"/>).
/// The tent text and mini-game prop come from the <see cref="TentDisplay"/> on the same GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(TentDisplay))]
public class MiniGameTent : MonoBehaviour
{
    /// <summary>Which mini-game this tent opens; also drives the text shown by <see cref="TentDisplay"/>.</summary>
    public GameManager.MiniGames MiniGame = GameManager.MiniGames.Archery;

    [Header("Tent Objects")]
    /// <summary>Play button shown when the player's gaze ray targets this tent.</summary>
    [SerializeField]
    private GameObject _buttonToPlayMiniGame;

    [Header("PlaceHolders")]
    /// <summary>Position and rotation where <see cref="_buttonToPlayMiniGame"/> is placed on Awake.</summary>
    [SerializeField]
    private Transform _buttonPlaceHolder;

    /// <summary>Cached collider used to check whether the gaze ray is hitting this tent.</summary>
    private Collider _collider;

    /// <summary>
    /// Furthest distance (meters) at which looking at a tent shows its play button. Bounds the gaze raycast, which
    /// previously ran to infinity.
    /// </summary>
    private const float MaxGazeDistance = 20f;

    /// <summary>
    /// <see cref="Time.frameCount"/> of the last gaze raycast. Shared by every tent so the tents do one raycast per
    /// frame between them instead of one each.
    /// </summary>
    private static int _gazeFrame = -1;

    /// <summary>Collider hit by the current frame's gaze raycast, or null if it hit nothing within <see cref="MaxGazeDistance"/>.</summary>
    private static Collider _gazedCollider;

    /// <summary>
    /// Caches the collider, initialises the tent display, places the play button on its placeholder and hides it
    /// until the player looks at the tent.
    /// </summary>
    private void Awake()
    {
        _collider = GetComponent<Collider>();

        GetComponent<TentDisplay>().Initialize(GetPlayPrompt(MiniGame));

        if (!Utils.RequireAssigned(_buttonToPlayMiniGame, nameof(_buttonToPlayMiniGame), this) ||
            !Utils.RequireAssigned(_buttonPlaceHolder, nameof(_buttonPlaceHolder), this))
        {
            enabled = false;
            return;
        }

        _buttonToPlayMiniGame.transform.SetPositionAndRotation(_buttonPlaceHolder.position, _buttonPlaceHolder.rotation);

        _buttonToPlayMiniGame.SetActive(false);
    }

    /// <summary>Shows or hides <see cref="_buttonToPlayMiniGame"/> based on whether the player is looking at this tent.</summary>
    private void LateUpdate()
    {
        CheckIfPlayerWantsToGoToMiniGame();
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

        bool isToShowButton = gazedCollider == _collider;

        if (_buttonToPlayMiniGame.activeSelf != isToShowButton)
        {
            _buttonToPlayMiniGame.SetActive(isToShowButton);
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

    /// <summary>Returns the tent text inviting the player to press the play button for the given mini-game.</summary>
    private static string GetPlayPrompt(GameManager.MiniGames miniGame)
    {
        switch (miniGame)
        {
            case GameManager.MiniGames.Archery:
                return "Carregue no botão para jogar arco e flecha";

            case GameManager.MiniGames.DuckGame:
                return "Carregue no botão para jogar o jogo do pato";

            case GameManager.MiniGames.Fishing:
                return "Carregue no botão para pescar";

            case GameManager.MiniGames.Frisbee:
                return "Carregue no botão para jogar frisbee";

            default:
                Debug.LogWarning($"MiniGame '{miniGame}' has no tent text assigned.");
                return string.Empty;
        }
    }

    /// <summary>Loads this tent's mini-game scene via <see cref="GameManager.GoToMiniGame"/> (a warning is logged for mini-games without a scene).</summary>
    /// <remarks>Invoked via Inspector by the play button's OnClick event on the tent prefab.</remarks>
    public void PlayerWantsToGoToMiniGame()
    {
        GameManager.GetInstance().GoToMiniGame(MiniGame);
    }
}
