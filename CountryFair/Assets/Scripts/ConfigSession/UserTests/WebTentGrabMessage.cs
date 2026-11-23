using System;
using UnityEngine.Scripting;

/// <summary>
/// Payload of the <c>"webTentGrab"</c> room message: a tent was grabbed or released on the web app, during the
/// configuration user test. Relayed by the server to the game only, where <see cref="ConfigSessionManager"/> hands it to
/// the web app's <see cref="ConfigTestLogger"/>. The field names are the keys the server (<c>sanitizeWebTentGrab</c>)
/// sends, so they must not be renamed.
/// </summary>
/// <remarks><c>[Preserve]</c> because the Colyseus SDK creates it through reflection when deserializing.</remarks>
[Serializable, Preserve]
public class WebTentGrabMessage
{
    /// <summary>Tent id, lower-cased (<c>"frisbee"</c>...): a <see cref="GameManager.MiniGames"/> name, ignoring case.</summary>
    public string miniGame;

    /// <summary>True when the drag started, false when the tent was dropped (after its new order was sent).</summary>
    public bool isSelected;

    /// <summary>Device that dragged the tent: <c>"computer"</c>, <c>"tablet"</c> or <c>"phone"</c>.</summary>
    public string device;

    /// <summary>Milliseconds the tent was held, measured in the browser; 0 when the drag started.</summary>
    public int grabDurationMs;

    [Preserve]
    public WebTentGrabMessage() { }
}
