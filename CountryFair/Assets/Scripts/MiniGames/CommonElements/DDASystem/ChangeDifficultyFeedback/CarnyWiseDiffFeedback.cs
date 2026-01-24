using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

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
    private Dictionary<string, Vector3> _feedbackElementsScales;

    protected override void Awake()
    {
        // 1. Inicia o download
        base.Awake();

        // 2. Referências e Inicializações Físicas (Seguro)
        if (increaseDiffExpression == null || decreaseDiffExpression == null)
        {
            Debug.LogError("One carny wise expression or more are not assigned in the inspector.");
            return;
        }

        _feedbackElementsScales = new Dictionary<string, Vector3>(){
            { "IncreaseDiffExpr", increaseDiffExpression.transform.localScale },
            { "DecreaseDiffExpr", decreaseDiffExpression.transform.localScale },
            { "DialogueBox", dialogueBoxGameObject.transform.localScale }
        };

        dialogueBoxGameObject.SetActive(false);
        increaseDiffExpression.SetActive(false);
        decreaseDiffExpression.SetActive(false);

        // O cast do _data foi REMOVIDO daqui.
    }

    // --- NOVO: Chamado quando o JSON chega ---
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

    // Este método é chamado por eventos externos
    public void ShowNewDiffFeedback(bool isToIncrease)
    {
        // Proteção: Se tentarem chamar isto antes do download acabar
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

    protected override void HideFeedback()
    {
        Sequence sequence = DOTween.Sequence();
        const Ease TRANSITION_EASE = Ease.InBack;

        if (increaseDiffExpression.activeSelf)
        {
            sequence.Append(increaseDiffExpression.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE));
        }
        else if (decreaseDiffExpression.activeSelf)
        {
            sequence.Append(decreaseDiffExpression.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE));
        }

        sequence.Join(dialogueBoxGameObject.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE).SetDelay(0.15f));

        sequence.OnComplete(() =>
        {
            increaseDiffExpression.SetActive(false);
            decreaseDiffExpression.SetActive(false);
            dialogueBoxGameObject.SetActive(false);

            increaseDiffExpression.transform.localScale = _feedbackElementsScales["IncreaseDiffExpr"];
            decreaseDiffExpression.transform.localScale = _feedbackElementsScales["DecreaseDiffExpr"];
            dialogueBoxGameObject.transform.localScale = _feedbackElementsScales["DialogueBox"];
        });
    }
}