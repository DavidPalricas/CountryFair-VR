using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial: UIDialog
{  
    [Header("Game Elements")]
    [SerializeField]
    private GameObject tutorialElements;

    [SerializeField]
    private GameObject postTutorialElements;

    private int _numberOfTasks;

    private List<string> _rules;

    private int _currentTasksCompleted = 0;

    private bool _tutorialCompleted = false;

    protected override void Awake()
    {  
        SetJSONFileName();
        
        base.Awake();

        if (data is TutorialData tutorialData)
        {
            _rules = tutorialData.Rules;
        }
        else
        {
            Debug.LogError("Tutorial data is not properly loaded or of incorrect type.");
        }

        if (tutorialElements  == null || postTutorialElements == null)
        {
            Debug.LogError("Tutorial or Post elements are not assigned in the inspector.");

            return;
        }

        tutorialElements.SetActive(true);
        postTutorialElements.SetActive(false);

 
        _numberOfTasks =  Utils.GetChildren(tutorialElements.transform).Length;

        ShowGameRule();
    }


    protected override void SetJSONFileName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _jsonFileName = "frisbee_tutorial.json";

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
            
            tutorialElements.SetActive(false);
            postTutorialElements.SetActive(true);
            Destroy(gameObject);
            return;
        }

        ShowGameRule();
    }

    private void ShowGameRule()
    {
        if (_rules.Count == 0){
            return;
        }

        dialogueBoxText.text = _rules[0];
        _rules.RemoveAt(0);
    }

    public void TaskCompleted()
    {
        _currentTasksCompleted++;

        if (_currentTasksCompleted >= _numberOfTasks)
        {
            TutorialCompleted();
            return;
        }
    }

    private void TutorialCompleted()
    {
        dialogueBoxText.text = "Parabéns! Você está pronto para começar o jogo!\n\tQuando estiver pronto carregue no botão e boa sorte!";
    }
}