using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;


/// <summary>
/// Cheat codes shared by every mini-game scene: scene/tutorial control, difficulty overrides, session goal and the
/// emotion-display test codes. Subclasses (<see cref="ArcheryCheatCodes"/>, <see cref="FrisbeeCheatCodes"/>) supply
/// the game-specific <c>miss</c>/<c>score</c> behaviour.
/// </summary>
/// <remarks>Until the tutorial is completed, only the codes in <see cref="_cheatCodesOnTutorial"/> are accepted.</remarks>
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
    private UnityEvent <ServerListener.DISPLAYMODE> _changeEmotionDisplay;

    /// <summary>Shows an expression on the face display (<c>f</c> + expression cheats).</summary>
    [SerializeField]
    private UnityEvent <ExpressionDisplay.EXPRESSION_TYPE> _updateFaceDisplay;

    /// <summary>Shows an expression on the emoji display (<c>e</c> + expression cheats).</summary>
   [SerializeField]
    private UnityEvent <ExpressionDisplay.EXPRESSION_TYPE> _updateEmojiDisplay;

    /// <summary>Nudges the mood slider (<c>positive</c>/<c>negative</c> cheats).</summary>
    [SerializeField]
    private UnityEvent <SliderDisplay.EMOJI_CATEGORY> _updateSliderDisplay;


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


    /// <summary>Set by subclasses from <see cref="GameManager"/> and by <see cref="SkipTutorial"/>; gates most cheats.</summary>
    protected bool _tutorialCompleted = false;

    /// <summary>Codes that work before the tutorial is completed.</summary>
    private readonly string[] _cheatCodesOnTutorial = new string[] {"return", "skip", "tutorial"};

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

        RegisterCheat("ehappy",    () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Happy));
        RegisterCheat("eneutral",  () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Neutral));
        RegisterCheat("esad",      () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Sad));
        RegisterCheat("eangry",    () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Angry));
        RegisterCheat("edisgust",  () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Disgust));
        RegisterCheat("esurprise", () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Surprise));
        RegisterCheat("efear",     () => DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE.Fear));

        RegisterCheat("fhappy",    () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Happy));
        RegisterCheat("fneutral",  () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Neutral));
        RegisterCheat("fsad",      () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Sad));
        RegisterCheat("fangry",    () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Angry));
        RegisterCheat("fdisgust",  () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Disgust));
        RegisterCheat("fsurprise", () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Surprise));
        RegisterCheat("ffear",     () => DisplayFace(ExpressionDisplay.EXPRESSION_TYPE.Fear));

        RegisterCheat("positive", () => DisplaySlider(SliderDisplay.EMOJI_CATEGORY.Positive));
        RegisterCheat("negative", () => DisplaySlider(SliderDisplay.EMOJI_CATEGORY.Negative));

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
                // codes in _cheatCodesOnTutorial are allowed before it is completed.
                if (!_tutorialCompleted && !_cheatCodesOnTutorial.Contains(code))
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
        if (_tutorialCompleted)
        {
            Debug.LogWarning("Cheat code 'tutorial' entered but tutorial already completed. Cheat ignored.");

            return;
        }

        _skipTutorial.Invoke();

        _tutorialCompleted = true;
    }

    /// <summary>Switches to the emoji display and shows <paramref name="type"/> (also synced over the network).</summary>
    private void DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE type)
    {      
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Emoji);
    
        _updateEmojiDisplay.Invoke(type);
    }

    /// <summary>Switches to the face display and shows <paramref name="type"/>.</summary>
    private void DisplayFace(ExpressionDisplay.EXPRESSION_TYPE type)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Face);
    
        _updateFaceDisplay.Invoke(type);
    }


    /// <summary>Switches to the slider display and nudges it towards <paramref name="category"/>.</summary>
    private void DisplaySlider(SliderDisplay.EMOJI_CATEGORY category)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Slider);

        _updateSliderDisplay.Invoke(category);
    }
}