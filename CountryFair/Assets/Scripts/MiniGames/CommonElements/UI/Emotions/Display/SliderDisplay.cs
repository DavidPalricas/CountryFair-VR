using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// Networked slider UI that tracks overall player mood as a POSITIVE / NEUTRAL / NEGATIVE category.
/// Each update nudges the slider value by <see cref="_sliderChangeAmountThreshold"/> and recolors the
/// fill and handle sprite accordingly. State is synchronized across the network via a server-authoritative
/// <see cref="NetworkVariable{T}"/>.
/// </summary>
/// <remarks>
/// Note that only the latest category is synchronized, not the slider value: a late joiner applies one nudge for the
/// current category instead of reproducing the accumulated value.
/// </remarks>
public class SliderDisplay: EmotionDisplay
{
    /// <summary>Slider component that visually represents cumulative mood (0 = most negative, 1 = most positive).</summary>
    [Header("Slider Configuration")]
    [SerializeField]
    private Slider _slider = null;

    /// <summary>Fill image of the slider; color reflects the current mood category.</summary>
    [SerializeField]
    private Image _fillImage = null;

    /// <summary>Handle image; sprite swaps between neutral/happy/angry emoji sprites based on mood.</summary>
    [SerializeField]
    private Image _handleImage = null;

    /// <summary>Fill and handle color when mood is in the positive range.</summary>
    [Header("Slider Colors")]
    [SerializeField]
    private Color _positiveColor = Color.green;

    /// <summary>Fill and handle color when mood is in the neutral range.</summary>
    [SerializeField]
    private Color _neutralColor = Color.yellow;

    /// <summary>Fill and handle color when mood is in the negative range.</summary>
    [SerializeField]
    private Color _negativeColor = Color.red;

    /// <summary>Sprite shown on the handle when mood is neutral.</summary>
    [Header("Emoji Sprites")]
    [SerializeField]
    private Sprite _neutralEmoji = null;

    /// <summary>Sprite shown on the handle when mood is positive.</summary>
    [SerializeField]
    private Sprite _happyEmoji = null;

    /// <summary>Sprite shown on the handle when mood is negative.</summary>
    [SerializeField]
    private Sprite _angryEmoji = null;

    /// <summary>Slider value at or above which the mood is classified as positive.</summary>
    [Header("Emojis State Thresholds")]
    [SerializeField]
    [Range(0, 1)]
    private float _positiveThreshold = 0.6f;

    /// <summary>Slider value at or below which the mood is classified as negative.</summary>
    [SerializeField]
    [Range(0, 1)]
    private float _negativeThreshold = 0.4f;

    /// <summary>Step amount added to or subtracted from the slider value on each emotion update.</summary>
    [SerializeField]
    [Range(0, 1)]
    private float _sliderChangeAmountThreshold = 0.05f;

    /// <summary>Last mood category; written only by the server (clients go through <see cref="RequestDisplayChangeServerRpc"/>).</summary>
     private readonly NetworkVariable<EMOJI_CATEGORY> _netSliderState = new(
        EMOJI_CATEGORY.Neutral,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>Mood category sent by the server after the separator (<c>EXPRESSION;CATEGORY</c>).</summary>
    public enum EMOJI_CATEGORY
    {
        Neutral,
        Positive,
        Negative,
    }

    /// <summary>Validates the references and resets the slider to a hidden neutral state.</summary>
    private void Awake()
    {  
        if (_slider == null ||  _fillImage  == null)
        {
            Debug.LogError("Slider and/or its fill image is not reference is not assigned in EmojiSlider.");
            return;
        }

        if (_handleImage == null)
        {
            Debug.LogError("Handle Image reference is not assigned in EmojiSlider.");
            return;
        }

        if (_neutralEmoji == null || _happyEmoji == null || _angryEmoji == null)
        {
            Debug.LogError("One or more emoji GameObjects are not assigned in EmojiSlider.");
            return;
        }

        StartAsNeutral();
    }

    
    /// <summary>Subscribes to the category variable and applies its current value without sending it back.</summary>
    public override void OnNetworkSpawn()
    {
        _netSliderState.OnValueChanged += OnCategoryChanged;
        
        // Force initial visual update without triggering send logic
        _isUpdatingFromNetwork = true;
        UpdateSlider(_netSliderState.Value);
        _isUpdatingFromNetwork = false;
    }

      /// <summary>
    /// Called when the object is despawned from the network.
    /// Unsubscribes from state changes.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        _netSliderState.OnValueChanged -= OnCategoryChanged;
    }

    
    /// <summary>Applies a category received from the network, guarded so it is not re-sent.</summary>
    private void OnCategoryChanged(EMOJI_CATEGORY previous, EMOJI_CATEGORY current)
    {
        _isUpdatingFromNetwork = true;
       UpdateSlider(current);
        _isUpdatingFromNetwork = false;
    }


    /// <summary>
    /// Applies a mood category to the slider: nudges value up for POSITIVE, down for NEGATIVE, unchanged for NEUTRAL.
    /// Updates visuals and, unless called from a network callback, syncs the category to the server.
    /// </summary>
    /// <param name="newCategory">The mood category to apply.</param>
    public void UpdateSlider(EMOJI_CATEGORY newCategory)
    {
       _slider.gameObject.SetActive(true);

        Debug.Log($"Updating slider with category: {newCategory}");

        if (newCategory == EMOJI_CATEGORY.Positive)
        {
             _slider.value = Mathf.Min(_slider.value + _sliderChangeAmountThreshold, 1);

        }else if(newCategory == EMOJI_CATEGORY.Negative){
             _slider.value = Mathf.Max(_slider.value - _sliderChangeAmountThreshold, 0);
        }

        UpdateEmojiVisual();


         // 2. NETWORK LOGIC (Only executes if NOT updating via network callback)
        if (!_isUpdatingFromNetwork)
        {
            SyncToNetwork(newCategory);
        }
    }
   
    /// <summary>Centers the slider (0.5) with the neutral sprite and color, and hides it until the first update.</summary>
    private void StartAsNeutral()
    {
        _handleImage.sprite = _neutralEmoji;
        _slider.value = 0.5f;
        _fillImage.color = _neutralColor;

        _slider.gameObject.SetActive(false);

    }

    /// <summary>Picks handle sprite and fill color from the value: positive at or above <see cref="_positiveThreshold"/>, negative at or below <see cref="_negativeThreshold"/>, neutral in between.</summary>
    private void UpdateEmojiVisual()
    {  
        if (_slider.value >= _positiveThreshold)
        {
            _handleImage.sprite = _happyEmoji;
         
            _fillImage.color = _positiveColor;
         

            return;
        }

        if (_slider.value <= _negativeThreshold)
        {
            _handleImage.sprite = _angryEmoji;
            _fillImage.color = _negativeColor;

            return;
        }

        _handleImage.sprite = _neutralEmoji;
        _fillImage.color = _neutralColor;
    }


    /// <summary>
    /// Parses a server message (format: <c>type;CATEGORY</c>) and calls <see cref="UpdateSlider"/> with the parsed category.
    /// </summary>
    /// <param name="message">Raw server string containing the emoji category after the separator.</param>
    /// <remarks>Does not call the base validation; a message without a separator throws an <see cref="IndexOutOfRangeException"/>.</remarks>
    public override void ProcessServerString(string message)
    {
        string emotionCategory = message.Split(_messageSeparator)[1];
        
        if (Enum.TryParse(emotionCategory, true, out EMOJI_CATEGORY result))
        {
           UpdateSlider(result); 
        }
    }


    /// <summary>
    /// Pushes a locally applied category to the network: written directly on the server, requested by RPC from the owning
    /// client, ignored when Netcode is not running (offline testing).
    /// </summary>
    private void SyncToNetwork(EMOJI_CATEGORY category)
    {
        // Checks if NetworkManager exists and is running
        if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
        {
            return; // We are offline (test mode), do nothing on network
        }

        // Avoid redundant sending if the value is already the same
        if (_netSliderState.Value == category) return;

        if (IsServer)
        {
            _netSliderState.Value = category;

            return;
        }

        if (IsOwner) // If it's a local cheat by the owner
        {
            RequestDisplayChangeServerRpc(category);
        }
    }

    /// <summary>Server RPC through which the owning client asks the server to write <see cref="_netSliderState"/>.</summary>
   [ServerRpc]
   private void RequestDisplayChangeServerRpc(EMOJI_CATEGORY category)
    {
        _netSliderState.Value = category;
    }
}