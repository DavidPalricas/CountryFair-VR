using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using TMPro;

/// <summary>
/// Abstract base class for all in-game dialogue UIs.
/// Loads a JSON file from <c>StreamingAssets/DialogFiles/</c> at runtime and drives a step-by-step dialogue flow.
/// Uses <see cref="UnityWebRequest"/> on Android (required for Quest StreamingAssets) and
/// <see cref="System.IO.File"/> on PC.
/// </summary>
/// <remarks>
/// Subclasses must override <see cref="SetJSONFileName"/>, <see cref="GetJSONDataType"/>,
/// <see cref="OnDataLoaded"/>, and <see cref="NextStep"/> to implement specific dialogue behaviour.
/// </remarks>
public abstract class UIDialog : MonoBehaviour
{
    [Header("Dialogue Box")]
    /// <summary>Root GameObject of the dialogue bubble; shown/hidden by subclass logic.</summary>
    [SerializeField]
    protected GameObject _dialogueBoxGameObject;

    /// <summary>Text component inside the dialogue bubble where lines are displayed.</summary>
    [SerializeField]
    protected TextMeshProUGUI _dialogueBoxText;

    [Header("Typewriter Effect")]
    /// <summary>Speed and punctuation-pause timings of lines shown through <see cref="ShowTextWithTypewriter"/>.</summary>
    [SerializeField]
    private TypewriterSettings _typewriterSettings = new();

    /// <summary>Typewriter helper bound to <see cref="_dialogueBoxText"/>; created lazily by <see cref="ShowTextWithTypewriter"/>.</summary>
    private TypewriterEffect _typewriter;

    /// <summary>Dialogue data deserialized from JSON by <see cref="LoadJSONDataRoutine"/>; concrete type is picked by the subclass via <see cref="GetJSONDataType"/>.</summary>
    protected JSONData _data;

    /// <summary>File name (under <c>StreamingAssets/DialogFiles/</c>) to load; set by the subclass via <see cref="SetJSONFileName"/> before <see cref="LoadJSONDataRoutine"/> runs.</summary>
    protected string _jsonFileName;

    /// <summary>Resolves which JSON file to load, validates the dialogue box references, and starts loading it.</summary>
    protected virtual void Awake()
    {
       SetJSONFileName();

       if (!Utils.RequireAssigned(_dialogueBoxGameObject, nameof(_dialogueBoxGameObject), this) ||
           !Utils.RequireAssigned(_dialogueBoxText, nameof(_dialogueBoxText), this))
       {
          return;
       }

       StartCoroutine(LoadJSONDataRoutine());
    }

    /// <summary>Called once <see cref="_data"/> has been populated; base implementation does nothing, subclasses cast <see cref="_data"/> to their concrete type and show the first line.</summary>
    protected virtual void OnDataLoaded()
    {
       // Debug.Log("Data loaded in base. Waiting for child class logic.");
    }

    /// <summary>Returns the concrete <see cref="JSONData"/> subtype to deserialize <see cref="_jsonFileName"/> into.</summary>
    protected abstract System.Type GetJSONDataType();

    /// <summary>
    /// Reads <see cref="_jsonFileName"/> from <c>StreamingAssets/DialogFiles/</c> (via <see cref="UnityWebRequest"/>
    /// on Android, since Quest builds cannot use <see cref="File"/> on StreamingAssets; via
    /// <see cref="File"/> elsewhere), deserializes it into the type from <see cref="GetJSONDataType"/>,
    /// stores it in <see cref="_data"/>, and calls <see cref="OnDataLoaded"/>.
    /// </summary>
    private IEnumerator LoadJSONDataRoutine()
    {
        string jsonContent = "";
   
        string relativePath = $"DialogFiles/{_jsonFileName}";

        if (Application.platform == RuntimePlatform.Android)
        {
            string uriPath = Path.Combine(Application.streamingAssetsPath, relativePath).Replace("\\", "/");

            using UnityWebRequest request = UnityWebRequest.Get(uriPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error reading JSON: {request.error} | URL: {uriPath}");
                yield break;
            }
            jsonContent = request.downloadHandler.text;
        }
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

                OnDataLoaded(); 
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in JSON format: " + e.Message);
        }
    }

    /// <summary>Shows <paramref name="content"/> in <see cref="_dialogueBoxText"/> with a typewriter effect.</summary>
    /// <param name="content">Line to show.</param>
    /// <param name="narrationDuration">
    /// Optional length (seconds) of the narration audio for this line; when &gt; 0 the reveal is stretched/compressed
    /// to end together with the audio. Leave at -1 while there is no narration.
    /// </param>
    /// <param name="onCompleted">Invoked once the whole line is visible (also when skipped).</param>
    protected void ShowTextWithTypewriter(string content, float narrationDuration = -1f, System.Action onCompleted = null)
    {
        _typewriter ??= new TypewriterEffect(gameObject, _dialogueBoxText, _typewriterSettings);
        _typewriter.Play(content, narrationDuration, onCompleted);
    }

    /// <summary>
    /// If a line is still being typed, reveals it entirely and returns true. Call at the start of
    /// <see cref="NextStep"/> so the first press completes the line and only the next one advances.
    /// </summary>
    protected bool SkipTypewriter()
    {
        return _typewriter != null && _typewriter.Skip();
    }

    /// <summary>Sets <see cref="_jsonFileName"/> to the dialogue file this instance should load.</summary>
    protected abstract void SetJSONFileName();

    /// <summary>Advances the dialogue by one step (line, section, or phase).</summary>
    public abstract void NextStep();
}