using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

/// <summary>
/// Manages emoji activation and synchronization over the network.
/// </summary>
public class ActivateEmoji : NetworkBehaviour
{ 
    /// <summary>
    /// Dictionary to look up emoji GameObjects by name.
    /// </summary>
    private readonly Dictionary<string, GameObject> _emojis = new();

    /// <summary>
    /// The currently active emoji GameObject.
    /// </summary>
    private GameObject _currentEmojiActive = null;

    /// <summary>
    /// Control variable to prevent infinite loops during network updates.
    /// </summary>
    private bool _isUpdatingFromNetwork = false;

    /// <summary>
    /// Network variable to maintain emoji state across clients.
    /// Only the server has permission to write to this variable.
    /// </summary>
    private readonly NetworkVariable<EmojiType> _netEmojiState = new(
        EmojiType.NEUTRAL, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Enumeration of supported emoji types.
    /// </summary>
    public enum EmojiType { 
        /// <summary>Represents a sad expression.</summary>
        SAD, 
        /// <summary>Represents a happy expression.</summary>
        HAPPY, 
        /// <summary>Represents an angry expression.</summary>
        ANGRY, 
        /// <summary>Represents a disgusted expression.</summary>
        DISGUST, 
        /// <summary>Represents a surprised expression.</summary>
        SURPRISE, 
        /// <summary>Represents a fearful expression.</summary>
        FEAR, 
        /// <summary>Represents a neutral expression.</summary>
        NEUTRAL 
    }

    /// <summary>
    /// Unity Awake method. Initializes internal state.
    /// </summary>
    private void Awake()
    {
        SetEmojis();
    }

    /// <summary>
    /// Called when the object is spawned on the network.
    /// Subscribes to state changes and updates visuals.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        _netEmojiState.OnValueChanged += OnEmojiStateChanged;
        
        // Force initial visual update without triggering send logic
        _isUpdatingFromNetwork = true;
        UpdateVisuals(_netEmojiState.Value);
        _isUpdatingFromNetwork = false;
    }

    /// <summary>
    /// Called when the object is despawned from the network.
    /// Unsubscribes from state changes.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _netEmojiState.OnValueChanged -= OnEmojiStateChanged;
    }

    /// <summary>
    /// Callback when the network variable changes.
    /// </summary>
    /// <param name="previous">Previous state.</param>
    /// <param name="current">New state.</param>
    private void OnEmojiStateChanged(EmojiType previous, EmojiType current)
    {
        // Protection: Notify UpdateVisuals that this came from the network
        _isUpdatingFromNetwork = true;
        UpdateVisuals(current);
        _isUpdatingFromNetwork = false;
    }

    /// <summary>
    /// Updates the visual representation of the active emoji.
    /// Does both local visual updates and network synchronization.
    /// </summary>
    /// <param name="type">The emoji type to display.</param>
    public void UpdateVisuals(EmojiType type)
    { 
        // 1. VISUAL UPDATE (Local and Immediate)
        string emojiName = type.ToString().ToLower();

        if (_emojis.TryGetValue(emojiName, out GameObject targetEmoji))
        {   
            if (_currentEmojiActive != targetEmoji)
            {
                if (_currentEmojiActive != null) _currentEmojiActive.SetActive(false);
                targetEmoji.SetActive(true);
                _currentEmojiActive = targetEmoji;
            }
        }
        else
        {
            Debug.LogError("Invalid emoji type: " + type);
            return; // If it fails visually, do not propagate to the network
        }

        // 2. NETWORK LOGIC (Only executes if NOT updating via network callback)
        if (!_isUpdatingFromNetwork)
        {
            SyncToNetwork(type);
        }
    }

    /// <summary>
    /// Isolated logic for sending state to the network.
    /// </summary>
    /// <param name="type">The emoji type to synchronize.</param>
    private void SyncToNetwork(EmojiType type)
    {
        // Checks if NetworkManager exists and is running
        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
        {
            return; // We are offline (test mode), do nothing on network
        }

        // Avoid redundant sending if the value is already the same
        if (_netEmojiState.Value == type) return;

        if (IsServer)
        {
            _netEmojiState.Value = type;
        }
        else if (IsOwner) // If it's a local cheat by the owner
        {
            RequestEmojiChangeServerRpc(type);
        }
    }

    /// <summary>
    /// Server RPC to request an emoji change from a client.
    /// </summary>
    /// <param name="newType">The new emoji type requested.</param>
    [ServerRpc]
    private void RequestEmojiChangeServerRpc(EmojiType newType)
    {
        _netEmojiState.Value = newType;
    }

    // --- UTILS E PARSING ---

    /// <summary>
    /// Processes a string message (e.g., from server) and updates the emoji.
    /// </summary>
    /// <param name="message">String representation of the EmojiType.</param>
    public void ProcessServerString(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        message = message.Trim();
        if (System.Enum.TryParse(message, true, out EmojiType result))
        {
            UpdateVisuals(result); 
        }
    }

    /// <summary>
    /// Initializes the emojis dictionary from child objects.
    /// </summary>
    private void SetEmojis()
    {
        foreach (GameObject emoji in Utils.GetChildren(transform))
        {   
            string emojiName = emoji.name.ToLower();
            _emojis[emojiName] = emoji;
            emoji.SetActive(false); 
        }

        if (_emojis.ContainsKey("neutral")) _currentEmojiActive = _emojis["neutral"];
    }
}