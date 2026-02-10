using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ActivateEmoji : NetworkBehaviour
{ 
    private readonly Dictionary<string, GameObject> _emojis = new();
    private GameObject currentEmojiActive = null;

    // Apenas o SERVIDOR pode escrever aqui.
    private readonly NetworkVariable<EmojiType> _netEmojiState = new(
        EmojiType.NEUTRAL, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public enum EmojiType { SAD, HAPPY, ANGRY, DISGUST, SURPRISE, FEAR, NEUTRAL }

    private void Awake()
    {
        SetEmojis();
    }

    public override void OnNetworkSpawn()
    {
        _netEmojiState.OnValueChanged += OnEmojiStateChanged;
        
        // Garante que quem entra tarde vê o emoji correto
        UpdateVisuals(_netEmojiState.Value);
    }

    public override void OnNetworkDespawn()
    {
        _netEmojiState.OnValueChanged -= OnEmojiStateChanged;
    }

    // --- LÓGICA DE REDE ---

    public void ProcessServerString(string rawMessage)
    {
        if (string.IsNullOrEmpty(rawMessage)) return;

        // Parsing da string
        rawMessage = rawMessage.Trim();
        if (System.Enum.TryParse<EmojiType>(rawMessage, true, out EmojiType result))
        {
            // 1. ATUALIZAÇÃO VISUAL IMEDIATA (O que tu pediste)
            // Não espera por ninguém. O jogador vê logo a mudança.
            UpdateVisuals(result); 
        }
        else
        {
            Debug.LogError($"Emoji desconhecido: {rawMessage}");
        }
    }

    private void OnEmojiStateChanged(EmojiType previous, EmojiType current)
    {
        // Se a mudança veio da rede e eu já atualizei visualmente antes (via ProcessServerString),
        // o UpdateVisuals lida com isso sem problemas (é idempotente).
        UpdateVisuals(current);
    }

    // --- LÓGICA VISUAL ---

    public void UpdateVisuals(EmojiType type)
    { 
        string emojiName = type.ToString().ToLower();

        if (_emojis.TryGetValue(emojiName, out GameObject targetEmoji))
        {   
            // Otimização: Se já estiver ativo, não faz nada (evita flickering)
            if (currentEmojiActive == targetEmoji) return;

            if (currentEmojiActive != null) currentEmojiActive.SetActive(false);
            
            targetEmoji.SetActive(true);
            currentEmojiActive = targetEmoji;
        }
        else
        {
            Debug.LogError("Invalid emoji type: " + type);
        }
    }

    private void SetEmojis()
    {
        foreach (GameObject emoji in Utils.GetChildren(transform))
        {   
            string emojiName = emoji.name.ToLower();
            _emojis[emojiName] = emoji;
            emoji.SetActive(false); 
        }

        if (_emojis.ContainsKey("neutral"))
            currentEmojiActive = _emojis["neutral"];
    }
}