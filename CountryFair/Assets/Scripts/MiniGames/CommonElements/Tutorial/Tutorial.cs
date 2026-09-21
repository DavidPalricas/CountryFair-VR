using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Drives the mini-game tutorial flow: presents rule slides from JSON, launches a practice phase,
/// and fires <c>tutorialCompleted</c> when the player is ready to play for real.
/// Skips itself immediately if <see cref="GameManager"/> reports the tutorial was already completed.
/// </summary>
public class Tutorial : UIDialog
{
    /// <summary>Fired when the tutorial ends — wired to <see cref="MiniGameManager.TutorialCompleted"/>.</summary>
    [SerializeField]
    private UnityEvent _tutorialCompleted;

    [Header("Practise Elements")]
    /// <summary>Video screen container shown during the practice phase.</summary>
    [SerializeField]
    private GameObject _videoScreen;

    /// <summary>Video player object shown on the screen during practice.</summary>
    [SerializeField]
    private GameObject  _miniGameVideo;

    /// <summary>Mini-game prop (bow or frisbee) revealed at the start of the practice phase.</summary>
    [SerializeField]
    private GameObject _miniGameProp;

    /// <summary>Root container that holds all interactive practice score areas.</summary>
    [SerializeField]
    private GameObject _practiceElements;

    [Header("Game Elements")]
    /// <summary>Button the player presses to advance through rule slides and to confirm practice completion.</summary>
    [SerializeField]
    private GameObject _tutorialButton;

    /// <summary>UI elements activated after the tutorial completes (e.g. score display).</summary>
    [SerializeField]
    private GameObject _postTutorialElements;

    /// <summary>Number of practice score areas found under <see cref="_practiceElements"/>; the practice phase ends once this many tasks are completed.</summary>
    private int _numberOfTasks;

    /// <summary>Parsed tutorial content (rules, practice guide, end message), set once <see cref="OnDataLoaded"/> deserializes the mini-game's tutorial JSON.</summary>
    private TutorialData _tutorialData;

    /// <summary>Number of practice tasks completed so far in the current session.</summary>
    private int _currentTasksCompleted = 0;

    /// <summary>True once all practice tasks are done and the "ready to play" confirmation is showing.</summary>
    private bool _finishedPracticing = false;

    /// <summary>True when the active scene is the Frisbee mini-game, false for Archery; set by <see cref="CheckCurrenMiniGame"/>.</summary>
    private bool _isFromFrisbeGame = false;

    /// <summary>Validates the practice/UI references, counts practice tasks from <see cref="_practiceElements"/>, and hides all practice/post-tutorial elements until they are needed.</summary>
    protected override void Awake()
    {
        base.Awake();

        if (_practiceElements == null || _postTutorialElements == null)
        {
            Debug.LogError("Practice or Post elements are not assigned in the inspector.");
            return;
        }

        if (_tutorialButton == null)
        {
            Debug.LogError("Tutorial button is not assigned in the inspector.");
            return;
        }

        if (_miniGameProp == null)
        {
            Debug.LogError("Mini game prop is not assigned in the inspector.");
            return;
        }

        if (_videoScreen == null || _miniGameVideo == null)
        {
            Debug.LogError("Video screen or mini game video is not assigned in the inspector.");
            return;
        }

        _numberOfTasks = Utils.GetChildren(_practiceElements.transform).Length;

        _practiceElements.SetActive(false);
        _miniGameProp.SetActive(false);
        _postTutorialElements.SetActive(false);
        _videoScreen.SetActive(false);
    }

    /// <summary>Wires <see cref="_tutorialCompleted"/> to <see cref="ConnectToWebApp"/>, then skips the tutorial immediately if <see cref="TutorialWasCompleted"/> reports it was already done.</summary>
    private void Start()
    {
        _tutorialCompleted.AddListener(ConnectToWebApp.Instance.PlayerFinishedTutorial);

         if (TutorialWasCompleted())
        {
            ActivatePostTutorialElements();
            _tutorialCompleted.Invoke();
            Destroy(gameObject);
        }
    }

    /// <summary>Casts the loaded JSON to <see cref="TutorialData"/> and shows the first rule slide.</summary>
    protected override void OnDataLoaded()
    {
        if (_data is not TutorialData tutorialData)
        {
            Debug.LogError("Error Converting data to TutorialData.");
            return;
        }

        _tutorialData = tutorialData;

        ShowGameRule();
    }

    /// <summary>Sets <see cref="_isFromFrisbeGame"/> by inspecting the active scene's name.</summary>
    private void CheckCurrenMiniGame()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _isFromFrisbeGame = true;
            return;
        }

        if (sceneName.Contains("archery"))
        {
            _isFromFrisbeGame = false;
            return;
        }

        Debug.LogError("Invalid scene for tutorial detection.");
    }

    /// <summary>
    /// Determines <see cref="_isFromFrisbeGame"/> and returns whether the matching mini-game's tutorial was already completed this session.
    /// </summary>
    private bool TutorialWasCompleted()
    {
        CheckCurrenMiniGame();

        GameManager gameManager = GameManager.GetInstance();

        // If in the development the developer starts testing on a mini game scene without going through the fair intro, to make sure when return to te fair does not have the intro dialogue this flag is set to true.
        gameManager.IntroCompleted = true;

        if (_isFromFrisbeGame && gameManager.FrisbeeTutorialCompleted)
        {
            return true;
        }

        if (!_isFromFrisbeGame && gameManager.ArcheryTutorialCompleted)
        {
            return true;
        }

        return false;
    }

    /// <summary>Always deserializes the tutorial JSON into <see cref="TutorialData"/>.</summary>
    protected override System.Type GetJSONDataType()
    {
        return typeof(TutorialData);
    }

    /// <summary>Picks the Frisbee or Archery tutorial JSON based on <see cref="_isFromFrisbeGame"/>.</summary>
    protected override void SetJSONFileName()
    {
        // _isFromFrisbeGame is determined by CheckCurrenMiniGame(), called from TutorialWasCompleted() in Start().
        _jsonFileName = _isFromFrisbeGame ? "frisbee_tutorial.json" : "archery_tutorial.json";
    }

    /// <summary>
    /// Advances to the next rule slide; transitions to the "ready to play" confirmation once practice ends.
    /// </summary>
    /// <remarks>Invoked via Inspector by the advance button on the tutorial screen.</remarks>
    public override void NextStep()
    {
        if (_tutorialData == null){
             return;
        }

        if (_finishedPracticing)
        {
            ReadyToPlay();
            return;
        }

        ShowGameRule();
    }

    /// <summary>
    /// Activates the mini-game prop and post-tutorial UI, fires <c>tutorialCompleted</c>, marks the tutorial
    /// complete in <see cref="GameManager"/>, and destroys this tutorial object.
    /// </summary>
    /// <remarks>Invoked via Inspector by the "Estou Pronto" ("I'm ready") button at the end of the tutorial.</remarks>
    public void ReadyToPlay()
    {
        ActivatePostTutorialElements();
        _tutorialCompleted.Invoke();

        if (_isFromFrisbeGame)
        {
            GameManager.GetInstance().FrisbeeTutorialCompleted = true;
        }
        else
        {
            GameManager.GetInstance().ArcheryTutorialCompleted = true;
        }

        Destroy(gameObject);
    }

    /// <summary>Activates the mini-game prop and post-tutorial UI (e.g. score display) before <see cref="_tutorialCompleted"/> fires, so listeners that depend on them (e.g. <see cref="MiniGameManager.TutorialCompleted"/> finding tag-based references) run after they exist in the scene.</summary>
    private void ActivatePostTutorialElements()
    {
        _miniGameProp.SetActive(true);
        _postTutorialElements.SetActive(true);
    }

    /// <summary>Displays and consumes the next rule slide, or starts the practice phase once all rules are shown.</summary>
    private void ShowGameRule()
    {
        List<string> rules = _tutorialData.Rules;

        if (rules.Count == 0)
        {
            StartPractice();
            return;
        }

        _dialogueBoxText.text = rules[0];
        _tutorialData.Rules.RemoveAt(0);
    }

    /// <summary>Hides the rule dialogue button and reveals the practice score areas, prop, and instructional video.</summary>
    private void StartPractice()
    {
        _tutorialButton.SetActive(false);
        _practiceElements.SetActive(true);
        _miniGameProp.SetActive(true);
        _videoScreen.SetActive(true);
        _miniGameVideo.SetActive(true);

        _dialogueBoxText.text = _tutorialData.Guide;
    }

    /// <summary>Shows the tutorial's end message and switches back to the dialogue button, hiding the practice prop and video.</summary>
    private void PractiseCompleted()
    {
        _dialogueBoxText.text = _tutorialData.End;

        _tutorialButton.SetActive(true);
        _miniGameProp.SetActive(false);
        _videoScreen.SetActive(false);
        _finishedPracticing = true;
    }

    /// <summary>
    /// Records one completed practice task; starts the end-of-practice dialogue when all tasks are done.
    /// </summary>
    /// <remarks>Invoked via Inspector by the taskCompleted events of the practice score areas.</remarks>
    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {
            PractiseCompleted();
        }
    }
}