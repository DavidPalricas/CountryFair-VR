using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

/// <summary>
/// Dialogue advanced by the player through a "next" button (tutorials, plaza dialogue). The button and its
/// "press to continue" prompt are hidden while a line is typed and shown shortly after it finishes.
/// Dialogues without player input (e.g. <c>CarnyWiseDiffFeedback</c>) derive from <see cref="UIDialog"/> directly.
/// </summary>
public abstract class InteractableDialogue : UIDialog
{
    [Header("Next Button")]
    /// <summary>
    /// Button that advances the dialogue (calls <see cref="UIDialog.NextStep"/>). Hidden while a line is being typed and only
    /// shown <see cref="_nextButtonDelaySeconds"/> after it finishes, to prevent accidental skips.
    /// </summary>
    [SerializeField]
    [FormerlySerializedAs("_tutorialButton")]
    protected GameObject _nextButton;

    /// <summary>Delay (seconds) between a line finishing and <see cref="_nextButton"/> appearing, so a hand already resting on its spot doesn't press it straight away.</summary>
    [SerializeField]
    [Min(0f)]
    private float _nextButtonDelaySeconds = 0.3f;

    /// <summary>
    /// "Press the button to continue" text game object, kept outside the dialogue lines so it is never narrated. Shown and hidden
    /// together with <see cref="_nextButton"/>.
    /// </summary>
    [SerializeField]
    private GameObject _continueDialogue;

    /// <summary>Pending delayed call that shows <see cref="_nextButton"/>, killed when a new line starts.</summary>
    private Tween _showNextButtonTween;

    /// <summary>Validates the button references, hides them, then runs the base setup.</summary>
    protected override void Awake()
    {
        if (!Utils.RequireAssigned(_nextButton, nameof(_nextButton), this) ||
            !Utils.RequireAssigned(_continueDialogue, nameof(_continueDialogue), this))
        {
            return;
        }

        _nextButton.SetActive(false);
        _continueDialogue.SetActive(false);

        base.Awake();
    }

    /// <summary>Hides the button while the new line is typed.</summary>
    protected override void OnLineStarted() => SetNextButtonVisible(false);

    /// <summary>Shows the button (after the delay) once the line is complete, if <see cref="ShouldShowNextButton"/> allows it.</summary>
    protected override void OnLineCompleted() => ScheduleNextButton();

    /// <summary>
    /// Whether <see cref="_nextButton"/> should appear when the current line finishes. Subclasses override it for
    /// phases where the dialogue must not be advanced by the button (e.g. the tutorial practice phase).
    /// </summary>
    protected virtual bool ShouldShowNextButton() => true;

    /// <summary>
    /// Whether the <see cref="_continueDialogue"/> prompt should appear together with <see cref="_nextButton"/>.
    /// Subclasses override it for lines whose text (and narration) already tells the player to press the button,
    /// so the prompt is not shown twice.
    /// </summary>
    protected virtual bool ShouldShowContinuePrompt() => true;

    /// <summary>Shows or hides <see cref="_nextButton"/> right away, cancelling any pending delayed show.</summary>
    protected void SetNextButtonVisible(bool visible)
    {
        _showNextButtonTween?.Kill();
        _showNextButtonTween = null;

        _nextButton.SetActive(visible);
        _continueDialogue.SetActive(visible && ShouldShowContinuePrompt());
    }

    /// <summary>Shows <see cref="_nextButton"/> after <see cref="_nextButtonDelaySeconds"/>, if <see cref="ShouldShowNextButton"/> allows it.</summary>
    private void ScheduleNextButton()
    {
        if (!ShouldShowNextButton())
        {
            return;
        }

        _showNextButtonTween?.Kill();
        _showNextButtonTween = DOVirtual.DelayedCall(_nextButtonDelaySeconds, () =>
        {
            _showNextButtonTween = null;
            _nextButton.SetActive(true);
            _continueDialogue.SetActive(ShouldShowContinuePrompt());
        }).SetLink(gameObject);
    }
}
