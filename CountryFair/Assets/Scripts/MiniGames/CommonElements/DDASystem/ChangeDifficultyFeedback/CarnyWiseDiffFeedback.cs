using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Displays a Carny Wise feedback message and expression when the difficulty changes.
/// Loads feedback lines from <c>change_difficulty.json</c> via <see cref="UIDialog"/>.
/// Requests that arrive before the JSON loads are queued and replayed after loading completes.
/// </summary>
public class CarnyWiseDiffFeedback : UIDialog
{
    /// <summary>Duration in seconds the feedback panel stays visible before auto-hiding.</summary>
    [Header("Display Configuration")]
    [SerializeField]
    [FormerlySerializedAs("displayDuration")]
    private float _displayDuration = 6f;

    /// <summary>Carny Wise face shown when the difficulty goes up.</summary>
    [Header("Carny Wise Expressions")]
    [SerializeField]
    [FormerlySerializedAs("increaseDiffExpression")]
    private GameObject _increaseDiffExpression;
    /// <summary>Carny Wise face shown when the difficulty goes down.</summary>
    [SerializeField]
    [FormerlySerializedAs("decreaseDiffExpression")]
    private GameObject _decreaseDiffExpression;


    /// <summary>Root of the Carny Wise panel; hidden at start and after <see cref="_displayDuration"/>.</summary>
    [Header("CarnyWise UI")]
    [SerializeField]
    [FormerlySerializedAs("carnyUI")]
    private GameObject _carnyUI;

    /// <summary>Wired to the scene audio manager's <c>PlaySoundEffect</c> to play the increase/decrease jingle.</summary>
    [Header("Sound Feedback")]
    [SerializeField]
    [FormerlySerializedAs("soundFeedback")]
    private UnityEvent <AudioManager.GameSoundEffects> _soundFeedback;

    /// <summary>
    /// Typed copy of the base <see cref="UIDialog._data"/>, holding the increase/decrease lines. Null until the JSON has
    /// loaded; <see cref="ShowNewDiffFeedback"/> uses that to decide whether to queue a request.
    /// </summary>
    /// <remarks>If the JSON never loads (missing file or bad format), it stays null and queued requests are never shown.</remarks>
    private DiffcultyFeedBackData _feedbackData;

    /// <summary>Jingle sent through <see cref="_soundFeedback"/> when the difficulty goes up (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _increaseDiffSound = AudioManager.GameSoundEffects.CARNYWISE_INCREASE_DIFF;
    /// <summary>Jingle sent through <see cref="_soundFeedback"/> when the difficulty goes down (fixed; not editable in the Inspector).</summary>
    private readonly AudioManager.GameSoundEffects _decreaseDiffSound = AudioManager.GameSoundEffects.CARNYWISE_DECREASE_DIFF;

    /// <summary>Difficulty changes requested before the JSON finished loading (true = increase), replayed in <see cref="OnDataLoaded"/>.</summary>
    private Queue<bool> _pendingRequests = new ();

    /// <summary>Starts loading the JSON (base), validates references and hides the panel.</summary>
    protected override void Awake()
    {
        base.Awake();

        if (_increaseDiffExpression == null || _decreaseDiffExpression == null)
        {
            Debug.LogError("ERROR: Carny Wise expressions are not assigned.");
            return;
        }

        if (_carnyUI == null)
        {
            Debug.LogError("ERROR: Carny Wise UI GameObject is not assigned.");
            return;
        }

        _carnyUI.SetActive(false);
    }


    /// <summary>Stores the loaded feedback lines and replays any queued requests.</summary>
    protected override void OnDataLoaded()
    {
        if (_data is not DiffcultyFeedBackData)
        {
            Debug.LogError("ERROR: Cast to DiffcultyFeedBackData failed.");
            return;
        }

        _feedbackData = _data as DiffcultyFeedBackData;

        while (_pendingRequests.Count > 0)
        {
            bool wasIncrease = _pendingRequests.Dequeue();
            ShowNewDiffFeedback(wasIncrease); 
        }
    }

    /// <summary>Deserializes <c>change_difficulty.json</c> into <see cref="DiffcultyFeedBackData"/>.</summary>
    protected override System.Type GetJSONDataType()
    {
        return typeof(DiffcultyFeedBackData);
    }

    /// <summary>Loads <c>StreamingAssets/DialogFiles/change_difficulty.json</c>; the same file serves both mini-games.</summary>
    protected override void SetJSONFileName()
    {
        _jsonFileName = "change_difficulty.json";
    }


    /// <summary>
    /// Shows the Carny Wise feedback panel with a random line and the appropriate facial expression.
    /// If JSON data is not yet loaded, queues the request for replay once loading completes.
    /// </summary>
    /// <remarks>
    /// A new request while the panel is open replaces the line and restarts the <see cref="_displayDuration"/> countdown.
    /// Called by <see cref="CarnyWise"/> when the difficulty changes.
    /// </remarks>
    /// <param name="isToIncrease">True to show an encouraging increase-difficulty message; false for a supportive decrease message.</param>
    public void ShowNewDiffFeedback(bool isToIncrease)
    {
        if (_feedbackData == null)
        {
            Debug.LogWarning($"[WARNING] Request received before load finished. Storing in queue... (Increase: {isToIncrease})");
            _pendingRequests.Enqueue(isToIncrease);
            return; 
        }

        _carnyUI.SetActive(true);

        List<string> feedbackTexts;

        AudioManager.GameSoundEffects soundToPlay;

        _increaseDiffExpression.SetActive(isToIncrease);
        _decreaseDiffExpression.SetActive(!isToIncrease);

        if (isToIncrease)
        {
            feedbackTexts = _feedbackData.IncreaseDiff;
            soundToPlay = _increaseDiffSound;
        }
        else
        {
            feedbackTexts = _feedbackData.DecreaseDiff;
            soundToPlay = _decreaseDiffSound;
        }

        _soundFeedback.Invoke(soundToPlay);

        if (feedbackTexts != null && feedbackTexts.Count > 0)
        {
            _dialogueBoxText.text = feedbackTexts[Utils.RandomValueInRange(0, feedbackTexts.Count)];
        }
        else
        {
            _dialogueBoxText.text = "...";
            Debug.LogError("List of feedback texts is null or empty.");
        }

        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), _displayDuration);
    }

    /// <summary>Hides the panel; scheduled with <c>Invoke</c> by <see cref="ShowNewDiffFeedback"/>.</summary>
    private  void HideFeedback()
    {
        _carnyUI.SetActive(false);
    }
}