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
   [SerializeField]
   private float displayDuration = 6f;

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

   private DiffcultyFeedBackData _feedBackData;
   private Vector3 _originalIncreaseScale;
   private Vector3 _originalDecreaseScale;
   private Vector3 _originalDialogueBoxScale;

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
        
        _originalIncreaseScale = increaseDiffExpression.transform.localScale;
        _originalDecreaseScale = decreaseDiffExpression.transform.localScale;
        _originalDialogueBoxScale = dialogueBoxGameObject.transform.localScale;

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
               _feedBackData = JsonConvert.DeserializeObject<DiffcultyFeedBackData>(jsonContent);
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

            Debug.Log("Showing increase difficulty feedback.");

            feedbackTexts = _feedBackData.IncreaseDiff;
        }
        else
        {
            decreaseDiffExpression.SetActive(true);
            feedbackTexts = _feedBackData.DecreaseDiff;
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

        if (increaseDiffExpression.activeSelf)
        {
            sequence.Append(increaseDiffExpression.transform.DOScale(0f, 0.4f).SetEase(Ease.InBack));
        }else if(decreaseDiffExpression.activeSelf)
        {
            sequence.Append(decreaseDiffExpression.transform.DOScale(0f, 0.4f).SetEase(Ease.InBack));
        }
           
        sequence.Join(dialogueBoxGameObject.transform.DOScale(0f, 0.4f).SetEase(Ease.InBack).SetDelay(0.15f));

        sequence.OnComplete(() => {
            increaseDiffExpression.SetActive(false);
            decreaseDiffExpression.SetActive(false);
            dialogueBoxGameObject.SetActive(false);

            // Restore original scales so they appear correctly next time
            increaseDiffExpression.transform.localScale = _originalIncreaseScale;
            decreaseDiffExpression.transform.localScale = _originalDecreaseScale;
            dialogueBoxGameObject.transform.localScale = _originalDialogueBoxScale;
        });
    }
}
