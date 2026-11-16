using System;
using UnityEngine.Scripting;

/// <summary>
/// Payload of the <c>"updateTentParams"</c> room message: the session parameters of one tent, sent by
/// <see cref="SessionParamsMenu"/> and by the web app's panel above each tent. The field names are the JSON/MsgPack keys
/// the server (<c>sanitizeTentParams</c>) and the web client (<c>TentParamsMessage</c>) use, so they must not be renamed.
/// </summary>
/// <remarks><c>[Preserve]</c> because the Colyseus SDK creates it through reflection when deserializing.</remarks>
[Serializable, Preserve]
public class TentParamsMessage
{
    /// <summary>Tent id: <see cref="GameManager.MiniGames"/> name (the server lower-cases it).</summary>
    public string miniGame;

    /// <summary>Number of repetitions (1 to 5).</summary>
    public int repetitions;

    /// <summary>Session time, in minutes (1 to 15).</summary>
    public int sessionTimeMinutes;

    /// <summary><see cref="SessionParamsMenu.DIFFICULTY_CHANGE"/> name (the server lower-cases it).</summary>
    public string difficultyChange;

    [Preserve]
    public TentParamsMessage() { }
}

/// <summary>Payload of the <c>"updateAffectedArm"</c> room message: the survivor's affected arm, shared by every tent.</summary>
[Serializable, Preserve]
public class AffectedArmMessage
{
    /// <summary>True if the affected arm is the left one.</summary>
    public bool isLeftArmAffected;

    [Preserve]
    public AffectedArmMessage() { }
}
