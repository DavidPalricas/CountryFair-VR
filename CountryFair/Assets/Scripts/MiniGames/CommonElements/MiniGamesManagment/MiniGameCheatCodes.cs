using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;


/// <summary>
/// Cheat codes shared by every mini-game scene: scene/tutorial control, difficulty overrides, session goal and the
/// emotion-display test codes. Subclasses (<see cref="ArcheryCheatCodes"/>, <see cref="FrisbeeCheatCodes"/>) supply
/// the game-specific <c>miss</c>/<c>score</c> behaviour.
/// </summary>
/// <remarks>
/// Until the tutorial is completed, only the codes in <see cref="_cheatCodesOnTutorial"/> are accepted, plus those in
/// <see cref="_cheatCodesOnPractice"/> while the tutorial's practice phase is running.
/// </remarks>
[RequireComponent(typeof(MiniGameManager))]
public class MiniGameCheatCodes : CheatCodes
{   
    /// <summary>Goal used by the <c>goal</c> cheat when no number follows it.</summary>
    [SerializeField]
    private int _defaultUpdateGoalValue = 10;

    /// <summary>
    /// Raised by the <c>increase</c> (true) and <c>decrease</c> (false) cheats; wired to <see cref="CarnyWise.ChangeDifficulty"/>.
    /// </summary>
    [Header("CarnyWise Events")]
    [SerializeField]
    private UnityEvent <bool> _changeDifficulty;

    /// <summary>Raised by the <c>reset</c> cheat; wired to <see cref="CarnyWise.ResetDifficulty"/>.</summary>
    [SerializeField]
    private  UnityEvent _resetDifficulty;

    /// <summary>Raised by the <c>complete</c> cheat; wired to <see cref="CarnyWise.SessionGoalReached"/>.</summary>
    [SerializeField]
    private UnityEvent _goalReached;

    /// <summary>Selects the active emotion display mode; wired to <see cref="ServerListener.UpdateDisplayMode"/>.</summary>
    [Header("Emotion Display Events")]

    [SerializeField]
    private UnityEvent <ServerListener.DisplayMode> _changeEmotionDisplay;

    /// <summary>Shows an expression on the face display (<c>f</c> + expression cheats).</summary>
    [SerializeField]
    private UnityEvent <ExpressionDisplay.ExpressionType> _updateFaceDisplay;

    /// <summary>Shows an expression on the emoji display (<c>e</c> + expression cheats).</summary>
   [SerializeField]
    private UnityEvent <ExpressionDisplay.ExpressionType> _updateEmojiDisplay;

    /// <summary>Nudges the mood slider (<c>positive</c>/<c>negative</c> cheats).</summary>
    [SerializeField]
    private UnityEvent <SliderDisplay.EmojiCategory> _updateSliderDisplay;


    /// <summary>Used by the <c>return</c> cheat.</summary>
    [Header("Other Common Mini-Game Events")]
    [SerializeField]
    private UnityEvent _returnToFair;

    /// <summary>Tutorial skipped by the <c>tutorial</c> cheat.</summary>
    [SerializeField]
    private UnityEvent _skipTutorial;

    /// <summary>Raised by the <c>goal</c> cheat with the new session goal (wired to the score system).</summary>
    [SerializeField]
    private UnityEvent <int> _newGoal;


    /// <summary>Whether this mini-game's tutorial is completed; gates most cheats.</summary>
    /// <remarks>
    /// Set to true only via Inspector by <c>Tutorial._tutorialCompleted</c>, which fires when the player finishes (or
    /// skips) the tutorial and also in <c>Tutorial.Start</c> when it was already completed earlier in the session.
    /// </remarks>
    public bool TutorialCompleted { get; set; } = false;

    /// <summary>Codes that work before the tutorial is completed.</summary>
    private readonly string[] _cheatCodesOnTutorial = new string[] {"return", "skip", "tutorial"};

    /// <summary>Extra codes that also work during the tutorial's practice phase (see <see cref="IsPracticing"/>).</summary>
    private readonly string[] _cheatCodesOnPractice = new string[] {"miss", "score"};

    /// <summary>
    /// True while the tutorial's practice phase is running; enables the codes in <see cref="_cheatCodesOnPractice"/>
    /// before the tutorial is completed, and lets subclasses target the practice objects instead of the game ones.
    /// </summary>
    /// <remarks>Set via Inspector by <c>Tutorial._practiceStateChanged</c> (true when practice starts, false when it ends).</remarks>
    public bool IsPracticing { get; set; } = false;

    /// <summary>Registers the shared cheats; subclasses then add their own and read the tutorial flag.</summary>
    protected virtual void Awake()
    {
        RegisterBaseCheats();
    }

    /// <summary>
    /// Registers all shared cheat codes available across every mini-game.
    /// Called from <see cref="Awake"/>.
    /// </summary>
    protected override void RegisterBaseCheats()
    {   
        base.RegisterBaseCheats();
        
        RegisterCheat("return", () => _returnToFair.Invoke());

        RegisterCheat("tutorial", () => SkipTutorial());

        RegisterCheat("reset",  () => _resetDifficulty.Invoke());
        
        RegisterCheat("miss",   OnMissCheat);
        RegisterCheat("score",  OnScoreCheat);

        RegisterCheat("complete", ()=> _goalReached.Invoke());

        RegisterCheat("increase", () => _changeDifficulty.Invoke(true));
        RegisterCheat("decrease", () => _changeDifficulty.Invoke(false));

        RegisterCheat("ehappy",    () => DisplayEmoji(ExpressionDisplay.ExpressionType.Happy));
        RegisterCheat("eneutral",  () => DisplayEmoji(ExpressionDisplay.ExpressionType.Neutral));
        RegisterCheat("esad",      () => DisplayEmoji(ExpressionDisplay.ExpressionType.Sad));
        RegisterCheat("eangry",    () => DisplayEmoji(ExpressionDisplay.ExpressionType.Angry));
        RegisterCheat("edisgust",  () => DisplayEmoji(ExpressionDisplay.ExpressionType.Disgust));
        RegisterCheat("esurprise", () => DisplayEmoji(ExpressionDisplay.ExpressionType.Surprise));
        RegisterCheat("efear",     () => DisplayEmoji(ExpressionDisplay.ExpressionType.Fear));

        RegisterCheat("fhappy",    () => DisplayFace(ExpressionDisplay.ExpressionType.Happy));
        RegisterCheat("fneutral",  () => DisplayFace(ExpressionDisplay.ExpressionType.Neutral));
        RegisterCheat("fsad",      () => DisplayFace(ExpressionDisplay.ExpressionType.Sad));
        RegisterCheat("fangry",    () => DisplayFace(ExpressionDisplay.ExpressionType.Angry));
        RegisterCheat("fdisgust",  () => DisplayFace(ExpressionDisplay.ExpressionType.Disgust));
        RegisterCheat("fsurprise", () => DisplayFace(ExpressionDisplay.ExpressionType.Surprise));
        RegisterCheat("ffear",     () => DisplayFace(ExpressionDisplay.ExpressionType.Fear));

        RegisterCheat("positive", () => DisplaySlider(SliderDisplay.EmojiCategory.Positive));
        RegisterCheat("negative", () => DisplaySlider(SliderDisplay.EmojiCategory.Negative));

        RegisterNumericCheat("goal", _defaultUpdateGoalValue, newGoal => _newGoal.Invoke(newGoal));
    }

 

    /// <summary>
    /// Called when the "miss" cheat is triggered. Override in subclasses for specific behaviour.
    /// </summary>
    protected virtual void OnMissCheat()
    {
        Debug.LogError("OnMissCheat should be overridden in derived classes.");
    }

    /// <summary>
    /// Called when the "score" cheat is triggered. Override in subclasses for specific behaviour.
    /// </summary>
    protected virtual void OnScoreCheat()
    {
        Debug.LogError("OnScoreCheat should be overridden in derived classes.");
    }

    /// <summary>
    /// Checks if the current input buffer contains any valid cheat code and executes it.
    /// </summary>
    protected override void CheckCheatCode()
    {
        if (TryMatchNumericCheat())
        {
            return;
        }

        foreach (var (code, command) in _cheatCommands)
        {
            if (_playerInput.Contains(code))
            {
                _playerInput = string.Empty;

                // Most cheats depend on objects that only exist after the tutorial (targets, dog, DDA), so only the
                // codes in _cheatCodesOnTutorial are allowed before it is completed (plus _cheatCodesOnPractice during
                // the practice phase, which has its own targets).
                if (!TutorialCompleted && !_cheatCodesOnTutorial.Contains(code) &&
                    !(IsPracticing && _cheatCodesOnPractice.Contains(code)))
                {
                    Debug.LogWarning($"Cheat code '{code}' entered but tutorial not completed. Cheat ignored.");

                    return;      
                }

                command.Invoke();

                return;
            }
        }
    }


    /// <summary>Finishes the tutorial immediately via <see cref="Tutorial.ReadyToPlay"/>.</summary>
    private void SkipTutorial()
    {    
        if (TutorialCompleted)
        {
            Debug.LogWarning("Cheat code 'tutorial' entered but tutorial already completed. Cheat ignored.");

            return;
        }

        // Tutorial.ReadyToPlay fires _tutorialCompleted, which sets TutorialCompleted.
        _skipTutorial.Invoke();
    }

    /// <summary>Switches to the emoji display and shows <paramref name="type"/> (also synced over the network).</summary>
    private void DisplayEmoji(ExpressionDisplay.ExpressionType type)
    {      
        _changeEmotionDisplay.Invoke(ServerListener.DisplayMode.Emoji);
    
        _updateEmojiDisplay.Invoke(type);
    }

    /// <summary>Switches to the face display and shows <paramref name="type"/>.</summary>
    private void DisplayFace(ExpressionDisplay.ExpressionType type)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DisplayMode.Face);
    
        _updateFaceDisplay.Invoke(type);
    }


    /// <summary>Switches to the slider display and nudges it towards <paramref name="category"/>.</summary>
    private void DisplaySlider(SliderDisplay.EmojiCategory category)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DisplayMode.Slider);

        _updateSliderDisplay.Invoke(category);
    }
}