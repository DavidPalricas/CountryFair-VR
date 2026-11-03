using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

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

    /// <summary>
    /// Button that advances the dialogue (calls <see cref="NextStep"/>). Hidden while a line is being typed and only
    /// shown <see cref="_nextButtonDelaySeconds"/> after it finishes, to prevent accidental skips. Optional: when
    /// unassigned the button is left as configured in the scene.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("_tutorialButton")]
    protected GameObject _nextButton;

    /// <summary>Delay (seconds) between a line finishing and <see cref="_nextButton"/> appearing, so a hand already resting on its spot doesn't press it straight away.</summary>
    [SerializeField]
    [Min(0f)]
    private float _nextButtonDelaySeconds = 0.3f;

    /// <summary>Pending delayed call that shows <see cref="_nextButton"/>, killed when a new line starts.</summary>
    private Tween _showNextButtonTween;

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

       if (_nextButton == null)
       {
          Debug.LogWarning($"{nameof(_nextButton)} is not assigned on {name}; it won't be hidden while text is being typed.", this);
       }
       else
       {
          _nextButton.SetActive(false);
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

    /// <summary>
    /// Shows <paramref name="content"/> in <see cref="_dialogueBoxText"/> with a typewriter effect. <see cref="_nextButton"/>
    /// is hidden while typing and shown after <see cref="_nextButtonDelaySeconds"/> once the line is complete
    /// (if <see cref="ShouldShowNextButton"/> allows it).
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

        SetNextButtonVisible(false);

        _typewriter.Play(content, narrationDuration, () =>
        {
            ScheduleNextButton();
            onCompleted?.Invoke();
        });
    }

    /// <summary>True while a line is still being typed; <see cref="NextStep"/> implementations ignore presses meanwhile.</summary>
    protected bool IsTyping => _typewriter != null && _typewriter.IsTyping;

    /// <summary>
    /// Whether <see cref="_nextButton"/> should appear when the current line finishes. Subclasses override it for
    /// phases where the dialogue must not be advanced by the button (e.g. the tutorial practice phase).
    /// </summary>
    protected virtual bool ShouldShowNextButton() => true;

    /// <summary>Shows or hides <see cref="_nextButton"/> right away, cancelling any pending delayed show.</summary>
    protected void SetNextButtonVisible(bool visible)
    {
        _showNextButtonTween?.Kill();
        _showNextButtonTween = null;

        if (_nextButton != null)
        {
            _nextButton.SetActive(visible);
        }
    }

    /// <summary>Shows <see cref="_nextButton"/> after <see cref="_nextButtonDelaySeconds"/>, if <see cref="ShouldShowNextButton"/> allows it.</summary>
    private void ScheduleNextButton()
    {
        if (_nextButton == null || !ShouldShowNextButton())
        {
            return;
        }

        _showNextButtonTween?.Kill();
        _showNextButtonTween = DOVirtual.DelayedCall(_nextButtonDelaySeconds, () =>
        {
            _showNextButtonTween = null;
            _nextButton.SetActive(true);
        }).SetLink(gameObject);
    }

    /// <summary>Sets <see cref="_jsonFileName"/> to the dialogue file this instance should load.</summary>
    protected abstract void SetJSONFileName();

    /// <summary>Advances the dialogue by one step (line, section, or phase).</summary>
    public abstract void NextStep();
}