using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ConFigSessionManager : MonoBehaviour
{  
    [Header("Selection Mode References")]
    [SerializeField]
    private GameObject _tentMenu;

    [SerializeField]
    private GameObject _giantButton;
    
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
        if (!Utils.RequireAssigned(_tentMenu, nameof(_tentMenu), this) || !Utils.RequireAssigned(_giantButton, nameof(_giantButton), this))
        {
            return;
        }

        SelectionMode[][] _availableSequences = new SelectionMode[][]
        {
            new SelectionMode[] { SelectionMode.RayGrab, SelectionMode.TentMenu, SelectionMode.GiantButton },
            new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.TentMenu, SelectionMode.RayGrab },
            new SelectionMode[] {SelectionMode.GiantButton, SelectionMode.RayGrab, SelectionMode.TentMenu},
        };

        SetTentsInteractables();

        _currentSequence = _availableSequences[Utils.RandomValueInRange(0, _availableSequences.Length)].ToList();

        _currentSelectionMode = _currentSequence[0];

        Debug.Log($"Current sequence: {string.Join(", ", _currentSequence)}");

        UpdateSelectionMode();
    }


    private void SetTentsInteractables()
    {    
        const string ConfigTentsInteractablesTag = "ConfigTentInteraction";
         
        GameObject[] tents = GameObject.FindGameObjectsWithTag(ConfigTentsInteractablesTag);

        _tentsInteractables = new Tuple<HandGrabInteractable, RayInteractable>[tents.Length];
       
        for (int i = 0; i < tents.Length; i++)
        {
            Transform tent = tents[i].transform;

            HandGrabInteractable grabInteractable = tent.GetComponentInChildren<HandGrabInteractable>();
            RayInteractable rayInteractable = tent.GetComponentInChildren<RayInteractable>();

            if (!Utils.RequireAssigned(grabInteractable, $"GrabInteractable of {tent.name}", this) ||
                !Utils.RequireAssigned(rayInteractable, $"RayInteractable of {tent.name}", this))
            {
                Debug.LogError($"Failed to initialize tent {tent.name}");
                return;
            }
    
            _tentsInteractables[i] = new Tuple<HandGrabInteractable, RayInteractable>(grabInteractable, rayInteractable);
        }
    }

    private void UpdateSelectionMode()
    {
        switch (_currentSelectionMode)
        {
            case SelectionMode.RayGrab:
                _tentMenu.SetActive(false);
                _giantButton.SetActive(false);

                foreach (var (grabInteractable, rayInteractable) in _tentsInteractables)
                {
                    grabInteractable.enabled = false;
                    rayInteractable.enabled = true;
                }

                return;

            case SelectionMode.TentMenu:
                _tentMenu.SetActive(true);
                _giantButton.SetActive(false);

                foreach (var (grabInteractable, rayInteractable) in _tentsInteractables)
                {
                    grabInteractable.enabled = false;
                    rayInteractable.enabled = true;
                }

                return;

            case SelectionMode.GiantButton:
                _tentMenu.SetActive(false);
                _giantButton.SetActive(true);

                foreach (var (grabInteractable, rayInteractable) in _tentsInteractables)
                {
                    grabInteractable.enabled = false;
                    rayInteractable.enabled = false;
                }

                return;

            default:
                Debug.LogError($"Unknown selection mode: {_currentSelectionMode}");
                return;
        }
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
    }
}
