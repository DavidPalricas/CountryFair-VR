using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


public class Tutorial: UIDialog
{   
    [SerializeField]
    private UnityEvent tutorialCompleted;

    [Header("Game Elements")]
    [SerializeField]
    private GameObject practiceElements;


    [SerializeField]
    private GameObject tutorialButton;

    [SerializeField]
    private GameObject miniGameProp;

    [SerializeField]
    private GameObject postTutorialElements;

    private int _numberOfTasks;

    private List<string> _rules;

    private int _currentTasksCompleted = 0;

    private bool _tutorialCompleted = false;

    protected override void Awake()
    {  
        base.Awake();

        if (_data is not TutorialData tutorialData)
        {
            Debug.LogError("Error Converting data to TutorialData.");

            return;
        }

        _rules = tutorialData.Rules;

        if (practiceElements  == null || postTutorialElements == null)
        {
            Debug.LogError("Practice or Post elements are not assigned in the inspector.");

            return;
        }

        if (tutorialButton == null)
        {
            Debug.LogError("Tutorial button is not assigned in the inspector.");

            return;
        }

        if (miniGameProp == null)
        {
            Debug.LogError("Mini game prop is not assigned in the inspector.");

            return;
        }

        _numberOfTasks =  Utils.GetChildren(practiceElements.transform).Length;

        practiceElements.SetActive(false);     
        miniGameProp.SetActive(false);
        postTutorialElements.SetActive(false);

        ShowGameRule();

        PositionInFrontOfPlayer();
    }


    protected override System.Type GetJSONDataType()
    {
        return typeof(TutorialData);
    }

    protected override void SetJSONFileName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _jsonFileName = "frisbbee_tutorial.json";

            return;
        }

        if (sceneName.Contains("archery"))
        {
            _jsonFileName = "archery_tutorial.json";

            return;
        }
      
        Debug.LogError("No tutorial JSON file found for the current scene.");

        return;
    }


    public void NextStep()
    {    
        if (_tutorialCompleted)
        {   
            postTutorialElements.SetActive(true);

            tutorialCompleted.Invoke();

            Destroy(transform.parent.gameObject);

             return;
        }

        ShowGameRule();
    }

    private void ShowGameRule()
    {
        if (_rules.Count == 0){
            StartPractice();
            return;
        }

        dialogueBoxText.text = _rules[0];
        _rules.RemoveAt(0);
    }


    private void StartPractice(){
        tutorialButton.SetActive(false);
        practiceElements.SetActive(true);
        miniGameProp.SetActive(true);
    }

    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {
             dialogueBoxText.text = "Parabéns! Você está pronto para começar o jogo!\n\tQuando estiver pronto carregue no botão e boa sorte!";

             tutorialButton.SetActive(true);
             miniGameProp.SetActive(false);
            return;
        }
    }
}