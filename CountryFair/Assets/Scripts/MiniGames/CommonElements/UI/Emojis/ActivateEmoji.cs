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
        
        // Ensures late joiners see the correct emoji
        UpdateVisuals(_netEmojiState.Value);
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
    /// Processes a string message from the server (e.g., from TCP listener) to update the emoji state.
    /// </summary>
    /// <param name="message">The string representation of the emoji type.</param>
    public void ProcessServerString(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("Received Empty Message from the server");

            return;
        }

        message = message.Trim();

        if (System.Enum.TryParse(message, true, out EmojiType result))
        {
            UpdateVisuals(result); 

            return;
        }
       
        Debug.LogError($"Unknown emoji: {message}");
    }

    /// <summary>
    /// Callback when the network variable changes.
    /// </summary>
    /// <param name="previous">Previous state.</param>
    /// <param name="current">New state.</param>
    private void OnEmojiStateChanged(EmojiType previous, EmojiType current)
    {
        UpdateVisuals(current);
    }


    /// <summary>
    /// Updates the visual representation of the active emoji.
    /// </summary>
    /// <param name="type">The emoji type to display.</param>
    public void UpdateVisuals(EmojiType type)
    { 
        string emojiName = type.ToString().ToLower();

        if (_emojis.TryGetValue(emojiName, out GameObject targetEmoji))
        {   
            if (_currentEmojiActive == targetEmoji)
            {
                return;
            } 

            if (_currentEmojiActive != null)
            {
                _currentEmojiActive.SetActive(false);
            } 
            
            targetEmoji.SetActive(true);
            _currentEmojiActive = targetEmoji;

            return;
        }
      
        Debug.LogError("Invalid emoji type: " + type);
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

        if (_emojis.ContainsKey("neutral"))
        {
            _currentEmojiActive = _emojis["neutral"];
        }   
    }
}