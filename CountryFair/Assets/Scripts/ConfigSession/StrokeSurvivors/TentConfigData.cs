using System;

/// <summary>
/// Configuration of one tent in the session configuration scene: its mini-game, its position in the fair order and
/// the session parameters chosen in its <see cref="SessionParamsMenu"/>.
///
/// Used both for the expected values (stored in a <see cref="StrokeSurvivorProfile"/>) and for the current values
/// (sent by a <see cref="ConfigTent"/> to <see cref="ConFigSessionManager"/> every time they change).
/// </summary>
[Serializable]
public struct TentConfigData
{
    /// <summary>Mini-game of the tent.</summary>
    public GameManager.MiniGames MiniGame;

    /// <summary>Position of the tent in the fair order (the <see cref="TentPlaceHolder.Number"/> of its slot, 1 to 4).</summary>
    public int Order;

    /// <summary>Number of repetitions.</summary>
    public int Repetitions;

    /// <summary>Maximum session time, in minutes.</summary>
    public int SessionTimeMinutes;

    /// <summary>How the game difficulty should change.</summary>
    public SessionParamsMenu.DIFFICULTY_CHANGE DifficultyChange;

    /// <summary>
    /// True if the affected arm is the left one. Only meaningful in the values sent by a tent: in a
    /// <see cref="StrokeSurvivorProfile"/> the affected arm is stored once for the survivor.
    /// </summary>
    public bool IsLeftArmAffected;
}
