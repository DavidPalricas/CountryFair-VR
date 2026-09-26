using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Cheat code handler for the CountryFair hub scene.
/// Provides developer shortcuts to skip/complete the intro dialogue, jump directly into any mini-game
/// and toggle giant mode, gating every non-intro cheat until the intro dialogue is completed.
/// </summary>
public class CountryFairCheatCodes : CheatCodes
{
    /// <summary>
    /// Fired by the "intro" cheat. Wired in the Inspector (CountryFair.unity) to
    /// <see cref="CountryFairDialogue.IntroComplete"/>, jumping straight to the end of the intro.
    /// </summary>
    [SerializeField]
    private UnityEvent _completeIntro;
    
    /// <summary>
    /// Fired by the "giant" cheat. Wired in the Inspector (CountryFair.unity) to
    /// <see cref="PlayerScale.ToggleScale"/>.
    /// </summary>
    [SerializeField]
    private UnityEvent _togglePlayerScale;

    /// <summary>
    /// Cheat codes that remain usable before the intro dialogue is completed
    /// (the two ways of progressing/finishing that dialogue itself).
    /// </summary>
    private readonly string[] _cheatCodesOnIntro = new string[] {"intro", "skip"};

    private void Awake()
    {
        RegisterBaseCheats();
    }

    /// <summary>
    /// Registers every cheat available in the CountryFair hub: completing/skipping the intro,
    /// jumping into a mini-game scene and toggling giant mode.
    /// </summary>
    protected override void RegisterBaseCheats()
    {   
        base.RegisterBaseCheats();
        
        RegisterCheat("intro", () => _completeIntro.Invoke());
   
        RegisterCheat("frisbee", () => GameManager.GetInstance().GoToMiniGame(OrderableTentElement.MINI_GAMES.FRISBEE));
        RegisterCheat("archery", () => GameManager.GetInstance().GoToMiniGame(OrderableTentElement.MINI_GAMES.ARCHERY));
        RegisterCheat("duck", () => GameManager.GetInstance().GoToMiniGame(OrderableTentElement.MINI_GAMES.DUCKGAME));
        RegisterCheat("fishing", () => GameManager.GetInstance().GoToMiniGame(OrderableTentElement.MINI_GAMES.FISHING));
        RegisterCheat("giant", () => _togglePlayerScale.Invoke());
    }

    /// <summary>
    /// Checks the input buffer against every registered cheat, gating anything outside
    /// <see cref="_cheatCodesOnIntro"/> until <see cref="GameManager.IntroCompleted"/> is true
    /// (and blocking "intro" once it already is, since completing it twice has no effect).
    /// Reads <see cref="GameManager.IntroCompleted"/> directly rather than caching it locally, so this
    /// stays correct even if the intro is completed through a path other than the "intro" cheat.
    /// </summary>
    protected override void CheckCheatCode()
    {
        foreach (var (code, command) in _cheatCommands)
        {
            if (_playerInput.Contains(code))
            {
                _playerInput = string.Empty;

                bool introCompleted = GameManager.GetInstance().IntroCompleted;

                if (introCompleted && code == "intro")
                {
                    Debug.LogWarning($"Cheat code '{code}' entered but intro already completed. Cheat ignored.");

                    return;
                }

                if (!introCompleted && !_cheatCodesOnIntro.Contains(code))
                {
                    Debug.LogWarning($"Cheat code '{code}' entered but intro not completed. Cheat ignored.");

                    return;
                }

                command.Invoke();

                return;
            }
        }
    }
}
   