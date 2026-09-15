using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Events;

/// <summary>
/// Orchestrates placeholder visibility and element positioning during drag-and-drop rearrangement.
/// Maintains a map of each <see cref="OrderableTentElement"/> to the <see cref="TentPlaceHolder"/> it currently occupies,
/// and swaps entries when the player drops an element into a slot already taken by another element.
/// One instance manages the world tents and a twin instance manages the wrist-menu panels; they mirror
/// each other's order via <see cref="_updateOtherManagers"/> / <see cref="OnOtherManagerUpdate"/>.
/// </summary>
public class TentPlaceHolderManager : MonoBehaviour
{
    /// <summary>Broadcasts this manager's current element order so the twin manager on the other surface can mirror it.</summary>
    [SerializeField]
    private UnityEvent<Dictionary<string, string>> _updateOtherManagers;

    /// <summary>Parent transform whose children are scanned for <see cref="OrderableTentElement"/> components to register.</summary>
    [SerializeField]
    private Transform _elementsTransform = null;


    /// <summary>Tracks which placeholder each element currently occupies.</summary>
    private readonly Dictionary<OrderableTentElement, TentPlaceHolder> _elementsMap = new ();

     /// <summary>
     /// Discovers all scene tents/panels by tag, seeds <see cref="_elementsMap"/> with their starting slots,
     /// and hides all placeholders until a grab begins.
     /// </summary>
     private void Awake()
     {
        if (_elementsTransform == null)
        {
            Debug.LogError("Elements Transform reference is null in PlaceHolderManager");

            return;
        }

        OrderableTentElement[] elements = GetOrderableElements();

        if (elements.Length == 0){
            Debug.LogError("No OrderableTentElement components found in children of miniGameTents.");

            return;
        }

        foreach (OrderableTentElement element in elements)
        {
            _elementsMap.Add(element, element.GetCurrentPlaceHolder());
        }

        TogglePlaceHolders(false);
     }

    /// <summary>Broadcasts the starting element order to the twin manager so both surfaces begin in sync.</summary>
    private void Start()
    {
        Dictionary<string, string> fairState = GetFairState();

        _updateOtherManagers.Invoke(fairState);
    }


    /// <summary>Collects every <see cref="OrderableTentElement"/> registered under <see cref="_elementsTransform"/>.</summary>
    private OrderableTentElement[] GetOrderableElements()
    {
        return _elementsTransform.GetComponentsInChildren<OrderableTentElement>();
    }

    /// <summary>Shows or hides all registered placeholder GameObjects.</summary>
    private void TogglePlaceHolders(bool isActive)
    {
        TentPlaceHolder[] placeHolders = _elementsMap.Values.ToArray();

        foreach (TentPlaceHolder placeHolder in placeHolders)
        {
            placeHolder.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Reacts to a tent being grabbed: hides every other tent and reveals all placeholders
    /// except the one already occupied by <paramref name="selectedElement"/>.
    /// </summary>
    /// <param name="selectedElement">The tent the player just picked up.</param>
    public void ElementSelected(OrderableTentElement selectedElement)
    {
        OrderableTentElement[] elements = _elementsMap.Keys.ToArray();

        int tentIndex = Array.IndexOf(elements, selectedElement);

        if (tentIndex == -1){
            Debug.LogError($"Selected tent '{selectedElement.gameObject.name}' is not registered.");
            return;
        }

        for (int i = 0; i < elements.Length; i++)
        {
            if (i != tentIndex){
                elements[i].gameObject.SetActive(false);
            }
        }

        TogglePlaceHolders(true);

        _elementsMap[selectedElement].gameObject.SetActive(false);
    }

    /// <summary>
    /// Reacts to a tent being released: updates positions (swapping tents if needed),
    /// re-enables all tents, and hides all placeholders.
    /// </summary>
    /// <param name="unselectedElement">The tent the player just released.</param>
    private void ElementUnselected(OrderableTentElement unselectedElement)
    {
        OrderableTentElement[] elements = _elementsMap.Keys.ToArray();

        int elementIndex = Array.IndexOf(elements, unselectedElement);

        if (elementIndex == -1){
            Debug.LogError($"Selected tent '{unselectedElement.gameObject.name}' is not registered.");
            return;
        }

        UpdateElementPosition(unselectedElement);

        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].gameObject.SetActive(true);
        }

        TogglePlaceHolders(false);

        Dictionary<string, string> fairState = GetFairState();

        _updateOtherManagers.Invoke(fairState);
    }

    /// <summary>Builds a mini-game-name-to-slot-number map describing the current fair layout, for the webapp.</summary>
    private Dictionary<string, string> GetFairState()
    {
        Dictionary<string, string> fairState = new();

        foreach (var kvp in _elementsMap)
        {
            OrderableTentElement element = kvp.Key;
            TentPlaceHolder placeHolder = kvp.Value;

            fairState[element.miniGame.ToString()] = placeHolder.number.ToString();
        }

        return fairState;
    }

    /// <summary>
    /// If <paramref name="unselectedElement"/> was dropped onto a placeholder occupied by another tent,
    /// swaps their entries in <see cref="_elementsMap"/> and teleports the displaced tent to the vacated slot.
    /// </summary>
    private void UpdateElementPosition(OrderableTentElement unselectedElement)
    {
       TentPlaceHolder previousUnselectedTentPlaceHolder = _elementsMap[unselectedElement];

       TentPlaceHolder currentUnselectedTentPlaceHolder = unselectedElement.GetCurrentPlaceHolder();

       foreach (OrderableTentElement tent in _elementsMap.Keys)
       {
            if (_elementsMap[tent] == currentUnselectedTentPlaceHolder)
            {
                _elementsMap[tent] = previousUnselectedTentPlaceHolder;
                _elementsMap[unselectedElement] = currentUnselectedTentPlaceHolder;

                tent.UpdateTentPlaceHolder(previousUnselectedTentPlaceHolder);


                tent.SnapToCurrentPlaceHolder();
                return;
            }
       }
    }

    /// <summary>
    /// Entry point called by <see cref="OrderableTentElement.OnElementSelectionChanged"/>.
    /// Routes to <see cref="ElementSelected"/> or <see cref="ElementUnselected"/> based on <paramref name="isTentSelected"/>.
    /// </summary>
    /// <param name="isTentSelected">True when the tent was grabbed; false when released.</param>
    /// <param name="element">The tent that changed state.</param>
    /// <remarks>Invoked via the Inspector in the <c>OnTentSelectionChanged</c> UnityEvent of each <see cref="OrderableTentElement"/>.</remarks>
    public void HandleTentSelection(bool isTentSelected, OrderableTentElement element)
    {
        if (isTentSelected)
        {
            ElementSelected(element);

            return;
        }

        ElementUnselected(element);
    }

    /// <summary>
    /// Mirrors the twin manager's element order onto this one: for each incoming element, finds the
    /// matching local element by <see cref="OrderableTentElement.miniGame"/>, moves it to the placeholder
    /// with the same slot <see cref="TentPlaceHolder.number"/>, and snaps it there.
    /// </summary>
    /// <param name="fairState">Mini-game-name-to-slot-number map describing the twin manager's current layout.</param>
    /// <remarks>Invoked via the Inspector in the <c>_updateOtherManagers</c> UnityEvent of the <see cref="TentPlaceHolderManager"/> on the other surface.</remarks>
    public void OnOtherManagerUpdate(Dictionary<string, string> fairState)
    {
        foreach (var miniGame in fairState.Keys)
        {
            OrderableTentElement element = _elementsMap.Keys.FirstOrDefault(e => e.miniGame.ToString().ToLower() == miniGame.ToLower());

            if (element == null)
            {
                Debug.LogError($"Element with miniGame '{miniGame}' not found in this manager.");

                return;
            }

            int placeHolderNumber = int.Parse(fairState[miniGame]);

            TentPlaceHolder updatedPlaceHolder = _elementsMap.Values.FirstOrDefault(ph => ph.number == placeHolderNumber);

            element.UpdateTentPlaceHolder(updatedPlaceHolder);

            UpdateElementPosition(element);

            element.SnapToCurrentPlaceHolder();
        }
    }
}
