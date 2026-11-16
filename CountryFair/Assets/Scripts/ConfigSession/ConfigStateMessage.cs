using System;
using UnityEngine.Scripting;

/// <summary>
/// Payload of the <c>"updateConfigState"</c> room message, sent by <see cref="ConfigSessionManager"/>: which survivor's
/// file is being configured and whether the tents already match it. The field names are the keys the server
/// (<c>sanitizeConfigState</c>) expects, so they must not be renamed.
/// </summary>
/// <remarks><c>[Preserve]</c> because the Colyseus SDK serializes it through reflection.</remarks>
[Serializable, Preserve]
public class ConfigStateMessage
{
    /// <summary>Name of the survivor (<see cref="StrokeSurvivorProfile.survivorName"/>, e.g. "Rute Pereira").</summary>
    public string survivor;

    /// <summary>True once the tents match the survivor's file.</summary>
    public bool configured;

    /// <summary>True if this is the last survivor of the test: the web app's button then ends the configuration.</summary>
    public bool isLast;

    [Preserve]
    public ConfigStateMessage() { }
}
