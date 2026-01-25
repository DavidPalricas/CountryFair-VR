using UnityEngine;
using System.Collections.Generic;


public class CarnyWiseDiffFeedback : UIDialog
{
    [Header("Display Configuration")]
    [SerializeField]
    private float displayDuration = 6f;

    [SerializeField]
    private float transitionDuration = 0.4f;

    [Header("Carny Wise Expressions")]
    [SerializeField]
    private GameObject increaseDiffExpression;
    [SerializeField]
    private GameObject decreaseDiffExpression;

    private DiffcultyFeedBackData _feedbackData;


    protected override void Awake()
    {
        base.Awake();

        if (increaseDiffExpression == null || decreaseDiffExpression == null)
        {
            Debug.LogError("One carny wise expression or more are not assigned in the inspector.");
            return;
        }

        dialogueBoxGameObject.SetActive(false);
        increaseDiffExpression.SetActive(false);
        decreaseDiffExpression.SetActive(false);
    }

    protected override void OnDataLoaded()
    {
        if (_data is not DiffcultyFeedBackData)
        {
            Debug.LogError("Could not cast data to DiffcultyFeedBackData.");
            return;
        }

        _feedbackData = _data as DiffcultyFeedBackData;
        Debug.Log("Feedback data loaded correctly.");
    }

    protected override System.Type GetJSONDataType()
    {
        return typeof(DiffcultyFeedBackData);
    }

    protected override void SetJSONFileName()
    {
        _jsonFileName = "change_difficulty.json";
    }

  
    public void ShowNewDiffFeedback(bool isToIncrease)
    {
        if (_feedbackData == null)
        {
            Debug.LogWarning("Tentativa de mostrar feedback antes do JSON carregar.");
            return;
        }

        PositionInFrontOfPlayer();

        List<string> feedbackTexts;

        if (isToIncrease)
        {
            increaseDiffExpression.SetActive(true);
            feedbackTexts = _feedbackData.IncreaseDiff;
        }
        else
        {
            decreaseDiffExpression.SetActive(true);
            feedbackTexts = _feedbackData.DecreaseDiff;
            Debug.Log("Showing decrease difficulty feedback.");
        }

        dialogueBoxGameObject.SetActive(true);

        if (feedbackTexts != null && feedbackTexts.Count > 0)
        {
            dialogueBoxText.text = feedbackTexts[Utils.RandomValueInRange(0, feedbackTexts.Count)];
        }

        Invoke(nameof(HideFeedback), displayDuration);
    }

    private  void HideFeedback()
    {
        dialogueBoxGameObject.SetActive(false);
        increaseDiffExpression.SetActive(false);
        decreaseDiffExpression.SetActive(false);
    }
}