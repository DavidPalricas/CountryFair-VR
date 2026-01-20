using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json;
using DG.Tweening;



public class CarnyWiseDiffFeedback : MonoBehaviour
{  
    [Header("Display Configuration")]
   [SerializeField]
   private float displayDuration = 6f;

   [SerializeField]
   private float transitionDuration = 0.4f;

   [Header("View Positioning")]
   [SerializeField]
   private Transform centerEyeTransform;

   [SerializeField]
   private float distanceFromPlayer = 12f;

   [SerializeField]
   private float heightOffset = 2.5f;

   [SerializeField]
   private float horizontalOffset =7f;

   [Header("Carny Wise Expressions")]
   [SerializeField]
   private GameObject increaseDiffExpression;
   [SerializeField]
   private GameObject decreaseDiffExpression;

   [Header("Dialogue Box")]
   [SerializeField]
   private GameObject dialogueBoxGameObject;

    [SerializeField]
   private TextMeshProUGUI dialogueBoxText;

   private const string _FEEDBACK_JSON_FILE_NAME = "change_difficulty.json";

   private DiffcultyFeedBackData _feedbackData;

   private Dictionary<string, Vector3> _feedbackElementsScales;


    private void Awake()
    {   
        if (increaseDiffExpression == null || decreaseDiffExpression == null)
        {
            Debug.LogError("One carny wise expression or more are not assigned in the inspector.");

            return;
        }

        if (dialogueBoxGameObject == null || dialogueBoxText == null)
        {
            Debug.LogError("Dialogue box or text is not assigned in the inspector.");

            return;
        }

        if (centerEyeTransform == null){
            Debug.LogError("Center Eye Transform is not assigned in the inspector.");

            return ;
        }

        _feedbackElementsScales = new Dictionary<string, Vector3>(){
            { "IncreaseDiffExpr", increaseDiffExpression.transform.localScale },
            { "DecreaseDiffExpr", decreaseDiffExpression.transform.localScale },
            { "DialogueBox", dialogueBoxGameObject.transform.localScale }
        };
        
 

        increaseDiffExpression.SetActive(false);
        decreaseDiffExpression.SetActive(false);
        dialogueBoxGameObject.SetActive(false);
        
        StartCoroutine(LoadFeedbackDataRoutine());
    }

    private IEnumerator LoadFeedbackDataRoutine()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, _FEEDBACK_JSON_FILE_NAME);
        string jsonContent = "";

        // READ FOR ANDROID (Meta Quest 3)
        if (Application.platform == RuntimePlatform.Android)
        {
            using UnityWebRequest request = UnityWebRequest.Get(filePath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error reading JSON on Quest: " + request.error);
                yield break; // Exit coroutine if failed
            }

            jsonContent = request.downloadHandler.text;
        }
 
        // LOGIC FOR PC / EDITOR
        else 
        {
            if (File.Exists(filePath))
            {
                jsonContent = File.ReadAllText(filePath);
                
            }
            else
            {
                Debug.LogError("File not found on PC: " + filePath);
                yield break;
            }
        }

        // PARSE (Igual para ambos)
        try
        {
            if (!string.IsNullOrEmpty(jsonContent))
            {
               _feedbackData = JsonConvert.DeserializeObject<DiffcultyFeedBackData>(jsonContent);
                Debug.Log("JSON loaded successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in JSON format: " + e.Message);
        }
    }

    public void ShowNewDiffFeedback(bool isToIncrease)
    {   
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

        dialogueBoxText.text = feedbackTexts[Utils.RandomValueInRange(0, feedbackTexts.Count)];

        Invoke(nameof( HideFeedback), displayDuration);
    }


    private void PositionInFrontOfPlayer()
    {   
        // Since the center eye position is in negative Z axis, we subtract to move forward
        Vector3 targetPosition = centerEyeTransform.position - centerEyeTransform.forward.normalized * distanceFromPlayer ; 
        targetPosition.y = centerEyeTransform.position.y - heightOffset; // Mantém a altura relativa ou fixa
        targetPosition.x = centerEyeTransform.position.x + horizontalOffset;
       
        transform.position = targetPosition;
    }

    private void HideFeedback()
    {
        Sequence sequence = DOTween.Sequence();

        const Ease TRANSITION_EASE = Ease.InBack;

        if (increaseDiffExpression.activeSelf)
        {
            sequence.Append(increaseDiffExpression.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE));
        }else if(decreaseDiffExpression.activeSelf)
        {
            sequence.Append(decreaseDiffExpression.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE));
        }
           
        sequence.Join(dialogueBoxGameObject.transform.DOScale(0f, transitionDuration).SetEase(TRANSITION_EASE).SetDelay(0.15f));

        sequence.OnComplete(() => {
            increaseDiffExpression.SetActive(false);
            decreaseDiffExpression.SetActive(false);
            dialogueBoxGameObject.SetActive(false);

            increaseDiffExpression.transform.localScale = _feedbackElementsScales["IncreaseDiffExpr"];
            decreaseDiffExpression.transform.localScale = _feedbackElementsScales["DecreaseDiffExpr"];
            dialogueBoxGameObject.transform.localScale = _feedbackElementsScales["DialogueBox"];
        });
    }
}
