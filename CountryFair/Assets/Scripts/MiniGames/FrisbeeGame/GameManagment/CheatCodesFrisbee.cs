using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodesFrisbee : MonoBehaviour
{   
    private string _playerInput = "";

    private readonly string[] _cheatCodes = new string[] { "throw"};

    private int _maxCheatLength;

    private OnPlayerFront _onPlayerFront;

    private void Awake()
    {   
        _maxCheatLength = _cheatCodes.Max(c => c.Length);

        _onPlayerFront = GameObject.FindGameObjectWithTag("Frisbee").GetComponent<OnPlayerFront>();

        if (_onPlayerFront == null)
        {
            Debug.LogError("OnPlayerFront component not found or Frisbee GameObject not found.");
        }
    }


    private void OnEnable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
        }
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    private void OnTextInput(char c)
    {
        if (char.IsLetterOrDigit(c))
        {
            _playerInput += c.ToString().ToLower();

            if (_playerInput.Length > _maxCheatLength)
            {
                _playerInput = _playerInput[^_maxCheatLength..];
            }

            CheckCheatCode();
        }
    }



    private void CheckCheatCode()
    {
       foreach (string code in _cheatCodes)
       {
           if (_playerInput.Contains(code))
           {   
               ActivateCheat(code);
           
               return;
           }
       }
    }

    private void ActivateCheat(string cheatCode){
        _playerInput = string.Empty;

        switch (cheatCode)
        {
            case "throw":
                _onPlayerFront.ThrowFrisbee();
                break;
            default:
                Debug.LogError("Invalid cheat code: " + cheatCode);
                break;
        }
    }

}
