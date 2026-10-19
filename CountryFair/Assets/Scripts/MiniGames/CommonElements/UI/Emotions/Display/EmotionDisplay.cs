using UnityEngine;
using Unity.Netcode;
using System.Linq;

/// <summary>
/// Abstract base class for networked emotion display components.
/// Defines the <see cref="_messageSeparator"/> convention and the
/// <see cref="ProcessServerString"/> contract used by <c>ServerListener</c> to push server-sent emotion updates.
/// </summary>
public abstract class EmotionDisplay: NetworkBehaviour
{
    /// <summary>
    /// Re-entrancy guard: true while a change is being applied from a <c>NetworkVariable</c> callback, so the visual update
    /// does not echo the same value back to the network.
    /// </summary>
    protected bool _isUpdatingFromNetwork = false;

    /// <summary>Separator of the server message <c>EXPRESSION;CATEGORY</c> (e.g. <c>happy;positive</c>).</summary>
    protected char _messageSeparator = ';';

    /// <summary>Subscribes to the subclass's <c>NetworkVariable</c> and applies its current value.</summary>
    public abstract override void OnNetworkSpawn();

    /// <summary>Unsubscribes from the subclass's <c>NetworkVariable</c>.</summary>
    public abstract override void OnNetworkDespawn();

    /// <summary>
    /// Entry point for messages from <see cref="ServerListener"/>. The base implementation trims the message and logs a
    /// warning unless it has exactly one separator.
    /// </summary>
    /// <remarks>
    /// The early returns only leave this base method: overrides that call it still run their own parsing afterwards,
    /// so they must not rely on it to reject malformed messages.
    /// </remarks>
    /// <param name="message">Raw server line, <c>EXPRESSION;CATEGORY</c>.</param>
    public virtual void ProcessServerString(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        } 

        message = message.Trim();

        int seperatorCount = message.Count(c => c == ';');

        if (seperatorCount != 1)
        {
            Debug.LogWarning("Server message format invalid: " + message + "must have only one ';' as a seprator.");

            return;
        }
    }
}