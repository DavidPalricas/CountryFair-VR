
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Intro : UIDialog
{   
    [SerializeField]
    private GameObject gameTents;
    
    [Header("Characters")]
    [SerializeField]
    private GameObject zeca;

    [SerializeField]
    private GameObject carnyWise;

    [SerializeField]
    private TextMeshProUGUI characterNameText;

    private IntroData _introData;

    private List<string> _currentDialogueLines = null;

   private enum IntroState
    {
        BEGIN,
        ZECA_PART1,
        ZECA_PART2,
        CARNY_WISE,

        COMPLETED
    }

    private IntroState _currentDialogueState = IntroState.BEGIN;

    protected override void Awake()
    {  
        if (GameManager.GetInstance().IntroCompleted)
        { 
            enabled = false;

            return;
        }

        base.Awake();

        if (zeca == null || carnyWise == null || characterNameText == null )
        {
            Debug.LogError("One or more character references are missing.");

            return;
        }


        if (gameTents == null)
        {
            Debug.LogError("Game Tents reference is missing.");

            return;
        }

        if (_data is not IntroData introData)
        {
            Debug.LogError("Error Converting data to IntroData.");

            return;
        }

        _introData = introData;


        gameTents.SetActive(false);

        SetCurrentDialogueLines();

        ShowDialogueLines();
    }


   private void SetCurrentDialogueLines()
    {
        switch (_currentDialogueState)
        {
            case IntroState.BEGIN:
                _currentDialogueLines = _introData.ZecaPart1;
                characterNameText.text = "Zeca";

                zeca.SetActive(true);
                carnyWise.SetActive(false);
                return;

            case IntroState.ZECA_PART1:
                _currentDialogueLines = _introData.CarnyWise;
                characterNameText.text = "Carny Wise";

                zeca.SetActive(false);
                carnyWise.SetActive(true);

                return;

            case IntroState.CARNY_WISE:
                _currentDialogueLines = _introData.ZecaPart2;
                characterNameText.text = "Zeca";

                zeca.SetActive(true);
                carnyWise.SetActive(false);

                return;
            case IntroState.ZECA_PART2:
                 _currentDialogueState = IntroState.COMPLETED;
              
                return;
            case IntroState.COMPLETED:
                 gameTents.SetActive(true);
                 GameManager.GetInstance().IntroCompleted = true;

                 Destroy(gameObject);
                 
                 return;
            default:
                Debug.LogError("Invalid dialogue state.");

                return;
        }
        
    }

    protected override System.Type GetJSONDataType()
    {
        return typeof(IntroData);
    }

    protected override void SetJSONFileName()
    {  
        _jsonFileName = "intro.json";
    }


    private void ShowDialogueLines()
    {
        dialogueBoxText.text = _currentDialogueLines[0];

        _currentDialogueLines.RemoveAt(0);
    }

    public override void NextStep()
    {   
        if (_currentDialogueLines.Count == 0)
        {
            SetCurrentDialogueLines();
        }

        ShowDialogueLines();
    }
}