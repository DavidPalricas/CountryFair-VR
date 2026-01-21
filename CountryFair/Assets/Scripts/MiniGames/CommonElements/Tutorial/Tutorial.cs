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

   private TutorialData _tutorialData;

    private int _currentTasksCompleted = 0;

    private bool _finishedPracticing = false;

    protected override void Awake()
    {  
        base.Awake();

        if (_data is not TutorialData tutorialData)
        {
            Debug.LogError("Error Converting data to TutorialData.");

            return;
        }

        _tutorialData = tutorialData;

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

        Debug.Log("Number of tasks in tutorial: " + _numberOfTasks);

        practiceElements.SetActive(false);     
        miniGameProp.SetActive(false);
        postTutorialElements.SetActive(false);

        ShowGameRule();
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
        if (_finishedPracticing)
        {   
            ReadyToPlay();
            return;
        }

        ShowGameRule();
    }


    private void ReadyToPlay()
    {
        miniGameProp.SetActive(true);

        postTutorialElements.SetActive(true);

        tutorialCompleted.Invoke();

        Destroy(transform.parent.gameObject);
    }

    private void ShowGameRule()
    {   
        List<string> rules = _tutorialData.Rules;

        if (rules.Count == 0){
            StartPractice();
            return;
        }

        dialogueBoxText.text = rules[0];

        _tutorialData.Rules.RemoveAt(0);
    }


    private void StartPractice(){
        tutorialButton.SetActive(false);
        practiceElements.SetActive(true);
        miniGameProp.SetActive(true);

        dialogueBoxText.text = _tutorialData.Guide;
    }

    private void Completed()
    {
        dialogueBoxText.text = _tutorialData.End;

        tutorialButton.SetActive(true);
         miniGameProp.SetActive(false);
        _finishedPracticing =true;
    }

    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {    
            Completed();
        }
    }
}