using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using TMPro;

/// <summary>
/// Abstract base class for all in-game dialogue UIs (JSON loading, typewriter, narration).
/// Dialogues advanced by the player through a button derive from <see cref="InteractableDialogue"/> instead.
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

    [Header("Narration")]
    /// <summary>
    /// FMOD event with a Programmer Instrument that plays the narration clips from the audio table.
    /// </summary>
    [SerializeField]
    private FMODUnity.EventReference _narrationEvent;

    /// <summary>Narration helper bound to <see cref="_narrationEvent"/>; created in <see cref="Awake"/>.</summary>
    private DialogueNarration _narration;

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
           !Utils.RequireAssigned(_dialogueBoxText, nameof(_dialogueBoxText), this) ||
           !Utils.RequireAssigned(_narrationEvent, nameof(_narrationEvent), this))
       {
          return;
       }

       _narration = new DialogueNarration(_narrationEvent);

       StartCoroutine(LoadJSONDataRoutine());
    }

    /// <summary>Stops any narration still playing when the dialogue is destroyed (e.g. on scene change or when it ends).</summary>
    protected virtual void OnDestroy()
    {
        StopNarration();
    }

    /// <summary>
    /// Builds the narration key of a line: <c>{json file name without extension}_{section}</c>, plus <c>_{index}</c>
    /// for lines that come from a list. E.g. <c>intro_ZecaPart1_0</c>, <c>frisbee_tutorial_Guide</c>.
    /// </summary>
    /// <param name="section">JSON property the line comes from.</param>
    /// <param name="index">Position of the line in that property's list, or -1 for single-string properties.</param>
    protected string GetNarrationKey(string section, int index = -1)
    {
        string key = $"{Path.GetFileNameWithoutExtension(_jsonFileName)}_{section}";

        return index >= 0 ? $"{key}_{index}" : key;
    }

    /// <summary>
    /// Shows <paramref name="content"/> narrated by the audio-table clip <paramref name="narrationKey"/>: the clip is
    /// loaded (synchronously) and the typewriter is stretched/compressed to its length, so text and voice end together.
    /// Any line still being narrated is stopped first.
    /// When there is no clip for that key, a warning is logged and the line is typed at normal speed with no audio.
    /// </summary>
    /// <param name="content">Line to show.</param>
    /// <param name="narrationKey">Audio table key, usually from <see cref="GetNarrationKey"/>.</param>
    /// <param name="onCompleted">Invoked once the whole line is visible.</param>
    /// <param name="onNarrationDuration">Receives the clip length in seconds, or -1 when the line has no narration.</param>
    protected void ShowNarratedLine(string content, string narrationKey, System.Action onCompleted = null, System.Action<float> onNarrationDuration = null)
    {
        float duration = _narration.Prepare(narrationKey);

        // Started in the same frame as the typewriter so the voice and the text begin together.
        _narration.Play();
        onNarrationDuration?.Invoke(duration);
        ShowTextWithTypewriter(content, duration, onCompleted);
    }

    /// <summary>Stops the line being narrated (with fade-out).</summary>
    protected void StopNarration()
    {
        _narration?.Stop();
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

    /// <summary>
    /// Shows <paramref name="content"/> in <see cref="_dialogueBoxText"/> with a typewriter effect, calling
    /// <see cref="OnLineStarted"/> before typing and <see cref="OnLineCompleted"/> once the line is fully visible.
    /// </summary>
    /// <param name="content">Line to show.</param>
    /// <param name="narrationDuration">
    /// Optional length (seconds) of the narration audio for this line; when &gt; 0 the reveal is stretched/compressed
    /// to end together with the audio. Leave at -1 while there is no narration.
    /// </param>
    /// <param name="onCompleted">Invoked once the whole line is visible.</param>
    protected void ShowTextWithTypewriter(string content, float narrationDuration = -1f, System.Action onCompleted = null)
    {
        _typewriter ??= new TypewriterEffect(gameObject, _dialogueBoxText, _typewriterSettings);

        OnLineStarted();

        _typewriter.Play(content, narrationDuration, () =>
        {
            OnLineCompleted();
            onCompleted?.Invoke();
        });
    }

    /// <summary>True while a line is still being typed; <see cref="NextStep"/> implementations ignore presses meanwhile.</summary>
    protected bool IsTyping => _typewriter != null && _typewriter.IsTyping;

    /// <summary>
    /// If a line is still being typed, reveals it entirely (which fires <see cref="OnLineCompleted"/>, so the next button appears)
    /// and returns true; otherwise returns false. <see cref="NextStep"/> implementations call it first so that the "skip" cheat
    /// completes the line on the first use and only advances on the next one.
    /// </summary>
    protected bool SkipTyping() => _typewriter != null && _typewriter.Skip();

    /// <summary>Hook called right before a line starts being typed; base does nothing.</summary>
    protected virtual void OnLineStarted() { }

    /// <summary>Hook called when a line has been fully typed; base does nothing.</summary>
    protected virtual void OnLineCompleted() { }

    /// <summary>Sets <see cref="_jsonFileName"/> to the dialogue file this instance should load.</summary>
    protected abstract void SetJSONFileName();

    /// <summary>Advances the dialogue by one step (line, section, or phase).</summary>
    public abstract void NextStep();
}