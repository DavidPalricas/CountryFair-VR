using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ActivateEmoji : NetworkBehaviour
{ 
    private readonly Dictionary<string, GameObject> _emojis = new();
    private GameObject _currentEmojiActive = null;

    // Variável de controlo para evitar loops infinitos
    private bool _isUpdatingFromNetwork = false;

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
        
        // Força atualização visual inicial sem disparar lógica de envio
        _isUpdatingFromNetwork = true;
        UpdateVisuals(_netEmojiState.Value);
        _isUpdatingFromNetwork = false;
    }

    public override void OnNetworkDespawn()
    {
        _netEmojiState.OnValueChanged -= OnEmojiStateChanged;
    }

    // Chamado quando a variável muda na rede
    private void OnEmojiStateChanged(EmojiType previous, EmojiType current)
    {
        // Proteção: Avisa o UpdateVisuals que isto veio da rede
        _isUpdatingFromNetwork = true;
        UpdateVisuals(current);
        _isUpdatingFromNetwork = false;
    }

    // --- O MÉTODO QUE PEDISTE (HÍBRIDO) ---
    public void UpdateVisuals(EmojiType type)
    { 
        // 1. ATUALIZAÇÃO VISUAL (Local e Imediata)
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
            return; // Se falhar visualmente, não propaga para a rede
        }

        // 2. LÓGICA DE REDE (Só executa se NÃO estivermos a atualizar via callback da rede)
        if (!_isUpdatingFromNetwork)
        {
            SyncToNetwork(type);
        }
    }

    // Lógica isolada de envio
    private void SyncToNetwork(EmojiType type)
    {
        // Verifica se o NetworkManager existe e está ligado
        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
        {
            return; // Estamos offline (modo teste), não faz nada na rede
        }

        // Evita envio redundante se o valor já for igual
        if (_netEmojiState.Value == type) return;

        if (IsServer)
        {
            _netEmojiState.Value = type;
        }
        else if (IsOwner) // Se for cheat local do dono
        {
            RequestEmojiChangeServerRpc(type);
        }
    }

    [ServerRpc]
    private void RequestEmojiChangeServerRpc(EmojiType newType)
    {
        _netEmojiState.Value = newType;
    }

    // --- UTILS E PARSING ---

    public void ProcessServerString(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        message = message.Trim();
        if (System.Enum.TryParse(message, true, out EmojiType result))
        {
            UpdateVisuals(result); 
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

        if (_emojis.ContainsKey("neutral")) _currentEmojiActive = _emojis["neutral"];
    }
}