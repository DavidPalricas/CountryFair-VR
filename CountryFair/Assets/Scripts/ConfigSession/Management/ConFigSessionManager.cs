using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ConfigSessionManager : MonoBehaviour
{  
    [Header("Stroke Survivors")]
    /// <summary>Survivor configured with the <see cref="SelectionMode.RayGrab"/> selection mode.</summary>
    [SerializeField]
    private StrokeSurvivorProfile _rayGrabSurvivor;

    /// <summary>Survivor configured with the <see cref="SelectionMode.TentMenu"/> selection mode.</summary>
    [SerializeField]
    private StrokeSurvivorProfile _tentMenuSurvivor;

    /// <summary>Survivor configured with the <see cref="SelectionMode.GiantButton"/> selection mode.</summary>
    [SerializeField]
    private StrokeSurvivorProfile _giantButtonSurvivor;

    [Header("Events")]
    [SerializeField]
    private UnityEvent<bool> _activateTentMenu;
    
    [SerializeField]
    private UnityEvent<bool> _activateGiantButton;
  
    [SerializeField]
    private UnityEvent<bool> _activateTentsHandGrab;

    [SerializeField]
    private UnityEvent<bool> _activateTentsRayInteractable;

    [SerializeField]
    private UnityEvent<bool> _activateConfigButton;
    
    [SerializeField]
    private UnityEvent<bool> _activateTents;

    [SerializeField]
    private UnityEvent<Material> _changedSurvivorFile;


    /// <summary>
    /// Current configuration of each tent, filled by <see cref="OnTentConfigChanged"/>. Initialized here (not in
    /// <see cref="Awake"/>) because the tents may report their configuration before this manager's Awake runs.
    /// </summary>
    private readonly Dictionary<GameManager.MiniGames, TentConfigData> _currentTentsConfig = new();

    /// <summary>Survivor being configured, the one associated with <see cref="_currentSelectionMode"/>.</summary>
    private StrokeSurvivorProfile _currentSurvivor;

    /// <summary>True while the tents match the current survivor's file, so <see cref="OnSessionConfigured"/> runs once per match.</summary>
    private bool _isConfigured = false;

    private SelectionMode _currentSelectionMode = SelectionMode.RayGrab;

    private List<SelectionMode> _currentSequence;

    private Tuple<HandGrabInteractable, RayInteractable>[] _tentsInteractables;

    /// <summary>Records the user test done in the headset (config time per mode, grab times per tent) in <c>userTestData.json</c>.</summary>
    private readonly ConfigTestLogger _testLogger = new("userTestData.json");

    /// <summary>Records what is configured on the web app in <c>webAppTestData.json</c>, one key per device.</summary>
    private readonly ConfigTestLogger _webTestLogger = new("webAppTestData.json");

    private enum SelectionMode
    {
        RayGrab,
        TentMenu,
        GiantButton
    }

    private void Awake()
    {
        if (!Utils.RequireAssigned(_rayGrabSurvivor, nameof(_rayGrabSurvivor), this) ||
            !Utils.RequireAssigned(_tentMenuSurvivor, nameof(_tentMenuSurvivor), this) ||
            !Utils.RequireAssigned(_giantButtonSurvivor, nameof(_giantButtonSurvivor), this))
        {
            return;
        }

        SelectionMode[][] _availableSequences = new SelectionMode[][]
        {
           new SelectionMode[] { SelectionMode.RayGrab} //, SelectionMode.TentMenu, SelectionMode.GiantButton },
        //   new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.TentMenu, SelectionMode.RayGrab },
         //  new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.RayGrab, SelectionMode.TentMenu},
        };

        _currentSequence = _availableSequences[Utils.RandomValueInRange(0, _availableSequences.Length)].ToList();

        _currentSelectionMode = _currentSequence[0];

        _activateTents.Invoke(true);

        Debug.Log($"Current sequence: {string.Join(", ", _currentSequence)}");

        _testLogger.StartSession(_currentSequence.Select(mode => mode.ToString()));

        // No planned sequence: the devices are stored in the order they were used.
        _webTestLogger.StartSession();

        OrderableTentElement.SelectionChanged += OnTentSelectionChanged;

        UpdateSelectionMode();

        ApplyCurrentSurvivor();
    }

    /// <summary>
    /// Registers with <see cref="ConnectToWebApp"/> at runtime (it is <c>DontDestroyOnLoad</c>, this manager is not, so
    /// an Inspector reference would go stale on a scene reload): the web app's "advance" button and every (re)join of
    /// the room, after which the state is reported again because anything sent before the join was dropped.
    /// </summary>
    private void Start()
    {
        ConnectToWebApp webApp = ConnectToWebApp.Instance;

        if (webApp == null)
        {
            return;
        }

        webApp.nextConfiguration.AddListener(OnWebNextConfiguration);
        webApp.joinedRoom.AddListener(ReportConfigState);
        webApp.webTentGrab.AddListener(OnWebTentGrab);

        ReportConfigState();
    }

    /// <summary>Stops listening to <see cref="ConnectToWebApp"/>.</summary>
    private void OnDestroy()
    {
        OrderableTentElement.SelectionChanged -= OnTentSelectionChanged;

        ConnectToWebApp webApp = ConnectToWebApp.Instance;

        if (webApp == null)
        {
            return;
        }

        webApp.nextConfiguration.RemoveListener(OnWebNextConfiguration);
        webApp.joinedRoom.RemoveListener(ReportConfigState);
        webApp.webTentGrab.RemoveListener(OnWebTentGrab);
    }

    /// <summary>
    /// Tells the web app which survivor's file is up and whether the tents already match it, so it shows the file or,
    /// once completed, the "advance" button.
    /// </summary>
    private void ReportConfigState()
    {
        if (ConnectToWebApp.Instance == null || _currentSurvivor == null)
        {
            return;
        }

        // SetNextSelectionMode removes the current mode from the sequence, so the last survivor is the only one left in it.
        bool isLast = _currentSequence != null && _currentSequence.Count <= 1;

        ConnectToWebApp.Instance.UpdateConfigState(_currentSurvivor.survivorName, _isConfigured, isLast);
    }

    /// <summary>
    /// The therapist pressed "advance to the next configuration" on the web app: does what the headset's config button does.
    /// Ignored unless the configuration is completed (the server already filters this, but the states can race).
    /// </summary>
    private void OnWebNextConfiguration()
    {
        if (!_isConfigured)
        {
            return;
        }

        SetNextSelectionMode();
    }

    private void UpdateSelectionMode()
    {
        _activateConfigButton.Invoke(false);

        bool tentMenuActive = false;
        bool giantButtonActive = false;
        bool handGrabActive = false;
        bool rayInteractableActive = false;

        switch (_currentSelectionMode)
        {
            case SelectionMode.RayGrab:
                rayInteractableActive = true;
                break;

            case SelectionMode.TentMenu:
                tentMenuActive = true;
                break;

            case SelectionMode.GiantButton:
                giantButtonActive = true;
                handGrabActive = true;
                break;

            default:
                Debug.LogError($"Unknown selection mode: {_currentSelectionMode}");
                return;
        }

        HandleSelection(tentMenuActive, giantButtonActive, handGrabActive, rayInteractableActive);
    }


    private void HandleSelection(bool tentMenuActive, bool giantButtonActive, bool handGrabActive, bool rayInteractableActive)
    {
        _activateTentMenu.Invoke(tentMenuActive);
        _activateGiantButton.Invoke(giantButtonActive);
        _activateTentsHandGrab.Invoke(handGrabActive);
        _activateTentsRayInteractable.Invoke(rayInteractableActive);
    }

    public void SetNextSelectionMode()
    {  
        _currentSequence.Remove(_currentSelectionMode);
        _activateTents.Invoke(true);        
        if (_currentSequence.Count == 0)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            
            return;
        }

        _currentSelectionMode = _currentSequence[0];

        UpdateSelectionMode();

        ApplyCurrentSurvivor();
    }

    /// <summary>Returns the survivor associated with a selection mode.</summary>
    private StrokeSurvivorProfile GetSurvivor(SelectionMode selectionMode)
    {
        switch (selectionMode)
        {
            case SelectionMode.RayGrab:
                return _rayGrabSurvivor;

            case SelectionMode.TentMenu:
                return _tentMenuSurvivor;

            case SelectionMode.GiantButton:
                return _giantButtonSurvivor;

            default:
                Debug.LogError($"No survivor for selection mode: {selectionMode}", this);
                return null;
        }
    }

    /// <summary>Shows the current survivor's file on the clipboard and checks the tents against it.</summary>
    private void ApplyCurrentSurvivor()
    {
        _currentSurvivor = GetSurvivor(_currentSelectionMode);

        if (_currentSurvivor == null)
        {
            return;
        }

        Debug.Log($"Current survivor: {_currentSurvivor.survivorName} ({_currentSelectionMode})");

        _testLogger.BeginMode(_currentSelectionMode.ToString(), _currentSurvivor);
        _webTestLogger.BeginMode(_currentSelectionMode.ToString(), _currentSurvivor);

        _changedSurvivorFile.Invoke(_currentSurvivor.fileMaterial);

        _isConfigured = false;

        // A new survivor starts not configured; ValidateConfiguration reports again if the tents already match it.
        ReportConfigState();

        ValidateConfiguration();
    }

    /// <summary>
    /// Stores the new configuration of a tent and checks the whole fair against the current survivor's file.
    /// </summary>
    /// <param name="tentConfig">Current configuration of the tent that changed.</param>
    /// <remarks>Invoked via Inspector by the <c>_onConfigChanged</c> event of each <see cref="ConfigTent"/>.</remarks>
    public void OnTentConfigChanged(TentConfigData tentConfig)
    {
        _currentTentsConfig[tentConfig.MiniGame] = tentConfig;

        _testLogger.OnTentConfigChanged(tentConfig);
        _webTestLogger.OnTentConfigChanged(tentConfig);

        ValidateConfiguration();
    }

    /// <summary>A tent or wrist-menu panel was grabbed or released (<see cref="OrderableTentElement.SelectionChanged"/>).</summary>
    private void OnTentSelectionChanged(OrderableTentElement element, bool isSelected)
    {
        _testLogger.OnTentSelectionChanged(element.MiniGame, isSelected);
    }

    /// <summary>
    /// A tent was grabbed or dropped on the web app (<see cref="ConnectToWebApp.webTentGrab"/>). The drop arrives after
    /// the new order, so the tents have already moved when it is logged, like a release in the headset.
    /// </summary>
    private void OnWebTentGrab(WebTentGrabMessage message)
    {
        if (!Enum.TryParse(message.miniGame, true, out GameManager.MiniGames game) || string.IsNullOrEmpty(message.device))
        {
            Debug.LogWarning($"[ConfigSession] Ignoring invalid web tent grab: {message.miniGame} / {message.device}");
            return;
        }

        // "tablet" -> "Tablet", the key of the device in webAppTestData.json.
        string device = char.ToUpperInvariant(message.device[0]) + message.device.Substring(1).ToLowerInvariant();

        double? grabDuration = message.isSelected ? (double?)null : message.grabDurationMs / 1000.0;

        _webTestLogger.OnTentSelectionChanged(game, message.isSelected, device, grabDuration);
    }

    /// <summary>
    /// Compares the tents with the current survivor's file and calls <see cref="OnSessionConfigured"/> when they
    /// start matching. Does nothing until every tent has reported its configuration.
    /// </summary>
    private void ValidateConfiguration()
    {
        if (_currentSurvivor == null)
        {
            return;
        }

        StrokeSurvivorProfile survivor = _currentSurvivor;

        if (_currentTentsConfig.Count < survivor.tents.Length)
        {
            return;
        }

        if (!survivor.Matches(_currentTentsConfig, out string mismatch))
        {
            bool wasConfigured = _isConfigured;

            _isConfigured = false;

            if (wasConfigured)
            {
                ReportConfigState();
            }

            Debug.Log($"[ConfigSession] {survivor.survivorName} mismatch: {mismatch}");

            return;
        }

        if (_isConfigured)
        {
            return;
        }

        _isConfigured = true;

        OnSessionConfigured();
    }

    /// <summary>Called once the tents match the current survivor's file.</summary>
    private void OnSessionConfigured()
    {
        _testLogger.CompleteMode();
        _webTestLogger.CompleteMode();

        _activateConfigButton.Invoke(true);

        _activateTents.Invoke(false);

        ReportConfigState();
    }
}
