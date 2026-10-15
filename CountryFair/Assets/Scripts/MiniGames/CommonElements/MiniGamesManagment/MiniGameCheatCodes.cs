using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;


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

    [Header("Mini Game Dependencies")]

    /// <summary>Used by the <c>return</c> cheat.</summary>
    [SerializeField]
    [FormerlySerializedAs("returnToFair")]
    private ReturnToFair _returnToFair;

    /// <summary>DDA system driven by the <c>increase</c>, <c>decrease</c>, <c>reset</c> and <c>complete</c> cheats.</summary>
    [SerializeField]
    [FormerlySerializedAs("carnyWise")]
    private CarnyWise _carnyWise;

    /// <summary>Tutorial skipped by the <c>tutorial</c> cheat.</summary>
    [SerializeField]
    [FormerlySerializedAs("tutorial")]
    private Tutorial _tutorial;

    /// <summary>Emoji display (<see cref="ServerListener.DISPLAYMODE.Emoji"/>) set by the <c>e*</c> cheats (e.g. <c>ehappy</c>).</summary>
    [Header("Emotion Server Dependencies")]
    [SerializeField]
    [FormerlySerializedAs("emojiDisplay")]
    private  ExpressionDisplay _emojiDisplay;
   
    /// <summary>Face display (<see cref="ServerListener.DISPLAYMODE.Face"/>) set by the <c>f*</c> cheats (e.g. <c>fhappy</c>).</summary>
   [SerializeField]
    [FormerlySerializedAs("faceDisplay")]
    private ExpressionDisplay _faceDisplay;
    
    /// <summary>Mood bar (<see cref="ServerListener.DISPLAYMODE.Slider"/>) nudged by the <c>positive</c>/<c>negative</c> cheats.</summary>
    [SerializeField]
    [FormerlySerializedAs("sliderDisplay")]
    private SliderDisplay _sliderDisplay;
   

    /// <summary>Wired to <see cref="ServerListener.UpdateDisplayMode"/> so a display cheat also switches to that display mode.</summary>
   [Header("Cheat Code Events")]
   [SerializeField]
    [FormerlySerializedAs("changeEmotionDisplay")]
    private UnityEvent <ServerListener.DISPLAYMODE> _changeEmotionDisplay;


    /// <summary>Raised by the <c>goal</c> cheat with the new session goal (wired to the score system).</summary>
    [SerializeField]
    private UnityEvent <int> _newGoal;

    /// <summary>
    /// The scene's Archery/Frisbee manager on the same GameObject (guaranteed by <c>[RequireComponent]</c>), cached in
    /// <see cref="Awake"/>. Read for <see cref="MiniGameManager.DifficultyLevel"/> and called directly by the
    /// <c>increase</c>, <c>decrease</c> and <c>reset</c> cheats.
    /// </summary>
    private MiniGameManager _miniGameManager;

    /// <summary>Set by subclasses from <see cref="GameManager"/> and by <see cref="SkipTutorial"/>; gates most cheats.</summary>
    protected bool _tutorialCompleted = false;

    /// <summary>Codes that work before the tutorial is completed.</summary>
    private readonly string[] _cheatCodesOnTutorial = new string[] {"return", "skip", "tutorial"};

    /// <summary>Validates the dependencies and registers the shared cheats.</summary>
    protected virtual void Awake()
    {
        if (_carnyWise == null)
        {
            Debug.LogError("CarnyWise component reference is not assigned in the inspector.");

            return;
        }

        if (_returnToFair == null)
        {
            Debug.LogError("ReturnToFair component reference is not assigned in the inspector.");

            return;
        }

        if (_tutorial == null)
        {
            Debug.LogError("Tutorial component reference is not assigned in the inspector.");

            return;
        }

        if (_emojiDisplay == null || _faceDisplay == null || _sliderDisplay == null)
        {
            Debug.LogError("One or more emotions display is missing");
            return;
        }


        _miniGameManager = GetComponent<MiniGameManager>();

        RegisterBaseCheats();
    }

    /// <summary>
    /// Registers all shared cheat codes available across every mini-game.
    /// Called from <see cref="Awake"/>.
    /// </summary>
    protected override void RegisterBaseCheats()
    {   
        base.RegisterBaseCheats();
        
        RegisterCheat("return", () => _returnToFair.Return());

        RegisterCheat("tutorial", () => SkipTutorial());

        RegisterCheat("reset",  () => ResetDifficulty());
        
        RegisterCheat("miss",   OnMissCheat);
        RegisterCheat("score",  OnScoreCheat);

        RegisterCheat("complete", ()=> _carnyWise.SessionGoalReached());

        RegisterCheat("increase", () => IncreaseDifficulty());
        RegisterCheat("decrease", () => DecreaseDifficulty());

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

                // Every cheat code except the  skp tutorial one ("tutorial") requires the tutorial to be completed, to avoid errors this statment is added"
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

    /// <summary>Back to level 0, with the decrease feedback; no-op if already at 0.</summary>
    private void ResetDifficulty()
    {   
        if (_miniGameManager.DifficultyLevel > 0)
        {
            _carnyWise.DecreaseDifficulty();
            _miniGameManager.ResetDifficulty();
        }
   
     }

    private  void IncreaseDifficulty()
    {      
        _carnyWise.IncreaseDifficulty();

        _miniGameManager.ChangeDifficulty(true);
    }

    private void DecreaseDifficulty()
    {   
        if (_miniGameManager.DifficultyLevel > 0)
        {
            _carnyWise.DecreaseDifficulty();
             
            _miniGameManager.ChangeDifficulty(false);     
        }
    }

    /// <summary>Finishes the tutorial immediately via <see cref="Tutorial.ReadyToPlay"/>.</summary>
    private void SkipTutorial()
    {   
        if (_tutorialCompleted)
        {
            Debug.LogWarning("Tutorial already completed. Cheat ignored.");
            return;
        }

        _tutorial.ReadyToPlay();

        _tutorialCompleted = true;
    }

    /// <summary>Switches to the emoji display and shows <paramref name="type"/> (also synced over the network).</summary>
    private void DisplayEmoji(ExpressionDisplay.EXPRESSION_TYPE type)
    {      
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Emoji);
    
        _emojiDisplay.UpdateVisuals(type);
    }

    /// <summary>Switches to the face display and shows <paramref name="type"/>.</summary>
    private void DisplayFace(ExpressionDisplay.EXPRESSION_TYPE type)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Face);
    
        _faceDisplay.UpdateVisuals(type);
    }


    /// <summary>Switches to the slider display and nudges it towards <paramref name="category"/>.</summary>
    private void DisplaySlider(SliderDisplay.EMOJI_CATEGORY category)
    {
        _changeEmotionDisplay.Invoke(ServerListener.DISPLAYMODE.Slider);

        _sliderDisplay.UpdateSlider(category);
    }

}