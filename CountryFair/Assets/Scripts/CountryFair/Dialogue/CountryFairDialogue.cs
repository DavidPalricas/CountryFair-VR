
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class CountryFairDialogue : UIDialog
{   [Header("Country Fair Settings")]
    [SerializeField]
    private GameObject gameTents;
    
    [SerializeField]
    // To turn on the balloon spawn
    private UnityEvent _dialogueCompleted;
    
    [Header("Characters")]
    [SerializeField]
    private GameObject zeca;

    [SerializeField]
    private GameObject carnyWise;

    [SerializeField]
    private TextMeshProUGUI characterNameText;

    private IntroData _introData;

    private SessionCompletedData _sessionCompleteData;

    private List<string> _currentDialogueLines = null;

   private enum DialogueState
    {
        BEGIN_INTRO,
        ZECA_INTRO_PART1,
        ZECA_INTRO_PART2,
        CARNY_WISE_INTRO,
        INTRO_COMPLETED,
    }

    private DialogueState _currentDialogueState = DialogueState.BEGIN_INTRO;


    protected override void Awake()
    {   
        GameManager gameManager = GameManager.GetInstance();

        if (GameManager.GetInstance().IntroCompleted)
        {   
            _currentDialogueState = DialogueState.INTRO_COMPLETED;

            // Its not to show nothing
            if (!gameManager.FrisbeeSessionCompleted && !gameManager.ArcherySessionCompleted)
            {   
                _dialogueCompleted.Invoke();
                Destroy(transform.parent.gameObject);

                return;
            }

        }

        base.Awake();

        if (zeca == null || carnyWise == null || characterNameText == null )
        {
            Debug.LogError("One or more character references are missing.");

            return;
        }

        carnyWise.SetActive(false);


        if (gameTents == null)
        {
            Debug.LogError("Game Tents reference is missing.");

            return;
        }

        gameTents.SetActive(false);

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

            return;
        }
       
        if (_data is not SessionCompletedData sessionCompleteData)
        {
            Debug.LogError("Error Converting data to SessionCompletedData.");

            return;
        }

        _sessionCompleteData = sessionCompleteData;

        ShowSessionCompletedLine();
    }


   private void SetIntroCurrentState()
    {  
        switch (_currentDialogueState)
        {
            case DialogueState.BEGIN_INTRO:
                _currentDialogueLines = _introData.ZecaPart1;
                characterNameText.text = "Zeca Bigodes";

                _currentDialogueState = DialogueState.ZECA_INTRO_PART1;

                zeca.SetActive(true);
                carnyWise.SetActive(false);
                return;

            case DialogueState.ZECA_INTRO_PART1:
                _currentDialogueLines = _introData.CarnyWise;
                characterNameText.text = "Carny Wise";

                _currentDialogueState = DialogueState.CARNY_WISE_INTRO;

                zeca.SetActive(false);
                carnyWise.SetActive(true);

                return;

            case DialogueState.CARNY_WISE_INTRO:
                _currentDialogueLines = _introData.ZecaPart2;
                characterNameText.text = "Zeca Bigodes";

                _currentDialogueState = DialogueState.ZECA_INTRO_PART2;

                zeca.SetActive(true);
                carnyWise.SetActive(false);

                return;
            case DialogueState.ZECA_INTRO_PART2:
                _currentDialogueState = DialogueState.INTRO_COMPLETED;

                IntroComplete();
                return;
      
            default:
                Debug.LogError("Invalid dialogue state to show text.");

                return;
        }
        
    }


    private void IntroComplete()
    {   
        _dialogueCompleted.Invoke();

        gameTents.SetActive(true);
        GameManager.GetInstance().IntroCompleted = true;

        Destroy(transform.parent.gameObject);
    }

    protected override System.Type GetJSONDataType()
    {   
        if (_currentDialogueState != DialogueState.INTRO_COMPLETED)
        {
            return typeof(IntroData);
        }
       
        return typeof(SessionCompletedData);
    }

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


    private void ShowIntroLines()
    {   if ( _currentDialogueState != DialogueState.INTRO_COMPLETED) 
        {
             dialogueBoxText.text = _currentDialogueLines[0];

            _currentDialogueLines.RemoveAt(0);
        }
    }


    private void ShowSessionCompletedLine()
    {   
        characterNameText.text = "Zeca Bigodes";
        dialogueBoxText.text = _sessionCompleteData.Congrats[Utils.RandomValueInRange(0, _sessionCompleteData.Congrats.Count)];
    }

    public override void NextStep()
    {   
        if (_currentDialogueState == DialogueState.INTRO_COMPLETED)
        {   
            _dialogueCompleted.Invoke();
            gameTents.SetActive(true);
            Destroy(transform.parent.gameObject);

            return;
        }
       
        if (_currentDialogueLines.Count == 0)
        {
            SetIntroCurrentState();
        }

        ShowIntroLines();
    }
}