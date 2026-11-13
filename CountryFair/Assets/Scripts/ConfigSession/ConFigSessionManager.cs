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
            new SelectionMode[] { SelectionMode.RayGrab, SelectionMode.TentMenu, SelectionMode.GiantButton },
            new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.TentMenu, SelectionMode.RayGrab },
            new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.RayGrab, SelectionMode.TentMenu},
        };

        _currentSequence = _availableSequences[Utils.RandomValueInRange(0, _availableSequences.Length)].ToList();

        _currentSelectionMode = _currentSequence[0];

        Debug.Log($"Current sequence: {string.Join(", ", _currentSequence)}");

        UpdateSelectionMode();

        ApplyCurrentSurvivor();
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
        
        if (_currentSequence.Count == 0)
        {
            Application.Quit();

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

        _changedSurvivorFile.Invoke(_currentSurvivor.fileMaterial);

        _isConfigured = false;

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

        ValidateConfiguration();
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
            _isConfigured = false;

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
        Debug.Log($"[ConfigSession] {_currentSurvivor.survivorName} configured successfully");

        _activateConfigButton.Invoke(true);
    }
}
