using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using TMPro;


public class UIDialog : DisplayInPlayerFront
{
   
   [Header("Dialogue Box")]
   [SerializeField]
   protected GameObject dialogueBoxGameObject;

    [SerializeField]
   protected TextMeshProUGUI dialogueBoxText;

   protected JSONData _data;

   protected string _jsonFileName;

   protected virtual void Awake()
   {  
      SetJSONFileName();

      if (dialogueBoxGameObject == null || dialogueBoxText == null)
      {
         Debug.LogError("Dialogue box or text is not assigned in the inspector.");

         return;
      }

      StartCoroutine(LoadJSONDataRoutine());
   }

   protected virtual System.Type GetJSONDataType()
   {
       Debug.LogError("GetJSONDataType method must be overridden in derived classes.");

       return null;
   }

    private IEnumerator LoadJSONDataRoutine()
    {   
        string filePath = Path.Combine(Application.streamingAssetsPath, "DialogFiles", _jsonFileName);
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
                _data = (JSONData)JsonConvert.DeserializeObject(jsonContent, GetJSONDataType());
                Debug.Log("JSON loaded successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in JSON format: " + e.Message);
        }
    }

    protected virtual void HideFeedback()
   {
      Debug.LogError("HideFeedback method must be overridden in derived classes.");
   }

   protected virtual void SetJSONFileName()
   {
       Debug.LogError("SetJSONFileName method must be overridden in derived classes.");
   }


}