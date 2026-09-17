using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Drives the hub-world dialogue flow: plays the intro sequence on first visit, or
/// congratulates the player after completing a mini-game session.
/// Loads dialogue content from JSON via <see cref="UIDialog"/>. When the dialogue ends it invokes
/// <see cref="_finishedDialogue"/> (which enables the post-intro UI, e.g. the wrist-menu tent
/// personalization, and notifies <see cref="ConnectToWebApp"/>) and destroys the dialogue canvas.
/// </summary>
public class CountryFairDialogue : UIDialog
{
    [Header("Characters")]
    /// <summary>Zeca Bigodes character GameObject, shown during his parts of the intro.</summary>
    [SerializeField]
    private GameObject _zeca;

    /// <summary>Carny Wise character GameObject, shown during his part of the intro.</summary>
    [SerializeField]
    private GameObject _carnyWise;

    /// <summary>Text field displaying the name of whichever character is currently speaking.</summary>
    [SerializeField]
    private TextMeshProUGUI _characterNameText;

    [Header("Events")]
    /// <summary>Fired once the dialogue sequence (intro or session-complete) ends; enables the post-dialogue UI and notifies <see cref="ConnectToWebApp.PlayerFinishedDialogue"/>.</summary>
    [SerializeField]
    private UnityEvent _finishedDialogue;

    /// <summary>Parsed intro dialogue lines, set once <see cref="OnDataLoaded"/> deserializes <c>intro.json</c>.</summary>
    private IntroData _introData;

    /// <summary>Parsed session-completed congratulation lines, set once <see cref="OnDataLoaded"/> deserializes the session-completed JSON.</summary>
    private SessionCompletedData _sessionCompleteData;

    /// <summary>Remaining lines of the intro section currently being shown; consumed one at a time by <see cref="ShowIntroLines"/>.</summary>
    private List<string> _currentDialogueLines = null;

    /// <summary>Steps of the intro sequence, one character section at a time, ending with <see cref="INTRO_COMPLETED"/> once the intro is done (or reused to display a session-completed message instead).</summary>
    private enum DialogueState
    {
        /// <summary>Not started yet; the next <see cref="SetIntroCurrentState"/> call loads Zeca's first part.</summary>
        BEGIN_INTRO,
        /// <summary>Zeca's first part is being shown.</summary>
        ZECA_INTRO_PART1,
        /// <summary>Zeca's second (closing) part is being shown.</summary>
        ZECA_INTRO_PART2,
        /// <summary>Carny Wise's part is being shown.</summary>
        CARNY_WISE_INTRO,
        /// <summary>The intro has been shown before (or was just finished); a session-completed message is shown instead, if there is one.</summary>
        INTRO_COMPLETED,
    }

    /// <summary>Tracks progress through the intro sequence, or <see cref="DialogueState.INTRO_COMPLETED"/> when showing a session-completed message instead.</summary>
    private DialogueState _currentDialogueState = DialogueState.BEGIN_INTRO;

    /// <summary>
    /// Validates character references, wires <see cref="_finishedDialogue"/> to <see cref="ConnectToWebApp.PlayerFinishedDialogue"/>,
    /// and short-circuits (skipping the dialogue entirely) when the intro was already completed and there is no
    /// mini-game session to congratulate the player for. Otherwise defers to <see cref="UIDialog.Awake"/> to load
    /// the appropriate JSON.
    /// </summary>
    protected override void Awake()
    {
        if (_zeca == null || _carnyWise == null || _characterNameText == null )
        {
            Debug.LogError("Characters missing.");
            return;
        }

        _finishedDialogue.AddListener(ConnectToWebApp.Instance.PlayerFinishedDialogue);

        GameManager gameManager = GameManager.GetInstance();

        if (gameManager.IntroCompleted)
        {   
            _currentDialogueState = DialogueState.INTRO_COMPLETED;
            
            if (!gameManager.FrisbeeSessionCompleted && !gameManager.ArcherySessionCompleted)
            {
                // The web app assumes a dialogue is up as soon as it sees "updateScene" land on
                // the hub (a session-completed dialogue may be about to play here). When there
                // is nothing to show, as here, tell it right away instead of leaving it waiting.
                ConnectToWebApp.Instance.PlayerFinishedDialogue();

                _finishedDialogue.Invoke();

                Destroy(transform.parent.gameObject);
                return;
            }
        }

        base.Awake();

        _carnyWise.SetActive(false);
    }

    /// <summary>
    /// Converts the JSON just loaded by <see cref="UIDialog"/> into <see cref="IntroData"/> or
    /// <see cref="SessionCompletedData"/> depending on <see cref="_currentDialogueState"/>, then shows the first line.
    /// </summary>
    protected override void OnDataLoaded()
    {
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
        {
             if (_data is not IntroData introData)
            {
                Debug.LogError("Error Converting data to IntroData.");
                return;
            }

            _introData = introData;

            SetIntroCurrentState();
            ShowIntroLines(); 
        }
        else
        {
            if (_data is not SessionCompletedData sessionCompleteData)
            {
                Debug.LogError("Error Converting data to SessionCompletedData.");
                return;
            }

            _sessionCompleteData = sessionCompleteData;
            ShowSessionCompletedLine();
        }
    }

    /// <summary>
    /// Advances the dialogue to the next line or section.
    /// When all lines are consumed, reveals post-intro elements and destroys the dialogue canvas.
    /// </summary>
    /// <remarks>Invoked via Inspector by the advance-dialogue button in the CountryFair scene.</remarks>
    public override void NextStep()
    {
        if (_data == null)
        {
            return;
        }

        if (_currentDialogueState == DialogueState.INTRO_COMPLETED)
        {   
            _finishedDialogue.Invoke();
            Destroy(transform.parent.gameObject);
            return;
        }
       
        if (_currentDialogueLines.Count == 0)
        {
            SetIntroCurrentState();
        }

        ShowIntroLines();
    }


    /// <summary>
    /// Advances the <see cref="DialogueState"/> machine to the next character section, or calls
    /// <see cref="IntroComplete"/> once <see cref="DialogueState.ZECA_INTRO_PART2"/> has finished.
    /// </summary>
    private void SetIntroCurrentState()
    {
        switch (_currentDialogueState)
        {
            case DialogueState.BEGIN_INTRO:
                _currentDialogueLines = _introData.ZecaPart1;
                _characterNameText.text = "Zeca Bigodes";
                _currentDialogueState = DialogueState.ZECA_INTRO_PART1;
                _zeca.SetActive(true);
                _carnyWise.SetActive(false);
                return;

            case DialogueState.ZECA_INTRO_PART1:
                _currentDialogueLines = _introData.CarnyWise;
                _characterNameText.text = "Carny Wise";
                _currentDialogueState = DialogueState.CARNY_WISE_INTRO;
                _zeca.SetActive(false);
                _carnyWise.SetActive(true);
                return;

            case DialogueState.CARNY_WISE_INTRO:
                _currentDialogueLines = _introData.ZecaPart2;
                _characterNameText.text = "Zeca Bigodes";
                _currentDialogueState = DialogueState.ZECA_INTRO_PART2;
                _zeca.SetActive(true);
                _carnyWise.SetActive(false);
                return;
            case DialogueState.ZECA_INTRO_PART2:
                _currentDialogueState = DialogueState.INTRO_COMPLETED;
                IntroComplete();
                return;
            default:
                Debug.LogError("Invalid dialogue state.");
                return;
        }
    }

    /// <summary>
    /// Marks the intro as completed in <see cref="GameManager"/>, reveals post-intro elements,
    /// and destroys this dialogue canvas.
    /// </summary>
    /// <remarks>Invoked via Inspector by the final button of the intro dialogue.</remarks>
    public void IntroComplete()
    {   
        _finishedDialogue.Invoke();
        GameManager.GetInstance().IntroCompleted = true;

        Destroy(transform.parent.gameObject);
    }

    /// <summary>Returns <see cref="IntroData"/> while the intro is in progress, otherwise <see cref="SessionCompletedData"/>.</summary>
    protected override System.Type GetJSONDataType()
    {
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
            return typeof(IntroData);

        return typeof(SessionCompletedData);
    }

    /// <summary>
    /// Picks <c>intro.json</c> while the intro is in progress; once it is completed, picks whichever
    /// mini-game's session-completed file matches the flag <see cref="GameManager"/> reports (clearing that
    /// flag so the message isn't shown again on the next hub visit).
    /// </summary>
    protected override void SetJSONFileName()
    {
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
        {
            _jsonFileName = "intro.json";
            return;
        }

        GameManager gameManager = GameManager.GetInstance();

        if (gameManager.FrisbeeSessionCompleted)
        {
            _jsonFileName = "frisbee_session_completed.json";
            gameManager.FrisbeeSessionCompleted = false;
            return;
        }

        if (gameManager.ArcherySessionCompleted)
        {
            _jsonFileName = "archery_session_completed.json";
            gameManager.ArcherySessionCompleted = false;
            return;
        }

        Debug.LogError("No session completed to show.");
    }

    /// <summary>Displays and consumes the first remaining line of <see cref="_currentDialogueLines"/>, if any.</summary>
    private void ShowIntroLines()
    {
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED && _currentDialogueLines != null && _currentDialogueLines.Count > 0) 
        {
             _dialogueBoxText.text = _currentDialogueLines[0];
            _currentDialogueLines.RemoveAt(0);
        }
    }

    /// <summary>Displays a random congratulation line from <see cref="_sessionCompleteData"/>, attributed to Zeca Bigodes.</summary>
    private void ShowSessionCompletedLine()
    {
        _characterNameText.text = "Zeca Bigodes";

        if(_sessionCompleteData != null && _sessionCompleteData.Congrats != null)
        {
             _dialogueBoxText.text = _sessionCompleteData.Congrats[Utils.RandomValueInRange(0, _sessionCompleteData.Congrats.Count)];
        }
           
    }
}