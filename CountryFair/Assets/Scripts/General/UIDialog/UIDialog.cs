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

    protected override void Awake()
    {   
       base.Awake();
       SetJSONFileName();

       if (dialogueBoxGameObject == null || dialogueBoxText == null)
       {
          Debug.LogError("Dialogue box or text is not assigned.");
          return;
       }

       // Inicia o carregamento. O resto acontece no Coroutine.
       StartCoroutine(LoadJSONDataRoutine());
    }

    // --- NOVO MÉTODO ---
    // Este método é chamado automaticamente quando o download termina.
    // As classes filhas (CountryFairDialogue) devem subscrever este método para iniciar a lógica.
    protected virtual void OnDataLoaded()
    {
        // Por defeito não faz nada, a classe filha é que decide o que fazer.
        Debug.Log("Dados carregados na base. À espera da lógica da classe filha.");
    }

    protected virtual System.Type GetJSONDataType()
    {
        Debug.LogError("GetJSONDataType must be overridden.");
        return null;
    }

    private IEnumerator LoadJSONDataRoutine()
    {   
        string jsonContent = "";
        
        // Caminho relativo simples
        string relativePath = $"DialogFiles/{_jsonFileName}";

        // LÓGICA ANDROID (Meta Quest 3)
        if (Application.platform == RuntimePlatform.Android)
        {
            // Força barras normais (/) para não quebrar o URI jar:file://
            string uriPath = Path.Combine(Application.streamingAssetsPath, relativePath).Replace("\\", "/");

            using (UnityWebRequest request = UnityWebRequest.Get(uriPath))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Erro ao ler JSON: {request.error} | URL: {uriPath}");
                    yield break;
                }
                jsonContent = request.downloadHandler.text;
            }
        }
        // LÓGICA PC
        else 
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "DialogFiles", _jsonFileName);
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

        // PARSE
        try
        {
            if (!string.IsNullOrWhiteSpace(jsonContent))
            {
                _data = (JSONData)JsonConvert.DeserializeObject(jsonContent, GetJSONDataType());
                Debug.Log("JSON loaded successfully!");
                
                // --- GATILHO AUTOMÁTICO ---
                // Chama a função que avisa a classe filha que os dados estão prontos
                OnDataLoaded(); 
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in JSON format: " + e.Message);
        }
    }

    protected virtual void HideFeedback() {}
    protected virtual void SetJSONFileName() {}
    public virtual void NextStep() {}
}