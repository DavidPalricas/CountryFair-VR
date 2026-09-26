using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Events;
using System;

public class CheatCodes : MonoBehaviour
{   

    /// <summary>
    /// Fired by the "skip" cheat. Wired in the Inspector (CountryFair.unity) to
    /// <see cref="CountryFairDialogue.NextStep"/>, advancing the intro by a single step.
    /// </summary>
    [SerializeField]
    private UnityEvent _skipDialogue;


    protected string _playerInput = "";

    /// <summary>
    /// Maximum length of any cheat code, used to limit input buffer size.
    /// Updated automatically when RegisterCheat() is called.
    /// </summary>
    protected int _maxCheatLength;

    /// <summary>
    /// Dictionary mapping cheat code strings to their corresponding commands (Actions).
    /// Subclasses register their own cheats via RegisterCheat().
    /// </summary>
    protected Dictionary<string, Action> _cheatCommands = new();

    /// <summary>
    /// Registered "prefix + optional number" cheats (e.g. "goal100"), keyed by prefix ("goal").
    /// Registered via <see cref="RegisterNumericCheat"/>.
    /// </summary>
    private readonly Dictionary<string, (int defaultValue, Action<int> command)> _numericCheatCommands = new();

    /// <summary>
    /// Prefix (e.g. "goal") of the numeric cheat currently being typed, or null if none is pending.
    /// A numeric cheat has no fixed length (the digit suffix is variable), so unlike the
    /// fixed-length cheats in <see cref="_cheatCommands"/> it can't be recognised as "complete" the
    /// instant it appears in the buffer - it's only committed once the player presses Enter
    /// (see <see cref="Update"/>), confirming they're done typing the number.
    /// </summary>
    private string _pendingNumericCheatPrefix = null;

    /// <summary>
    /// Subscribes to keyboard text input and device change events when enabled.
    /// </summary>
    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        SubscribeKeyboard();
    }

    /// <summary>
    /// Unsubscribes from all input events when disabled.
    /// </summary>
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        if (Keyboard.current != null)
        {
             Keyboard.current.onTextInput -= OnTextInput;
        }

        ConnectToWebApp.Instance.receiveCheatCode.RemoveListener(OnWebCheatCode);
    }

    /// <summary>
    /// Subscribes to <see cref="ConnectToWebApp.receiveCheatCode"/> so cheat codes typed on the
    /// web app are processed the same way as keyboard input. Done in Start() (rather than
    /// OnEnable(), like the keyboard) because <see cref="ConnectToWebApp.Instance"/> is only
    /// guaranteed to exist once every object's Awake() has run, matching the pattern already
    /// used for this event in <c>TentPlaceHolderManager.Start()</c>.
    /// </summary>
    private void Start()
    {
        ConnectToWebApp.Instance.receiveCheatCode.AddListener(OnWebCheatCode);
    }

    /// <summary>
    /// Subscribes to the current keyboard if one is available.
    /// </summary>
    private void SubscribeKeyboard()
    {
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput; // avoid double subscription
            Keyboard.current.onTextInput += OnTextInput;

            return;
        }
    }

    /// <summary>
    /// Listens for new devices being connected (e.g. Bluetooth keyboard on Quest)
    /// and subscribes to text input when a keyboard is added.
    /// </summary>
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard keyboard)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                keyboard.onTextInput -= OnTextInput; // avoid double subscription
                keyboard.onTextInput += OnTextInput;

                return;
            }

            if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                keyboard.onTextInput -= OnTextInput;
            }
        }
    }

    /// <summary>
    /// Confirms a pending numeric cheat (see <see cref="_pendingNumericCheatPrefix"/>) the moment
    /// the player presses Enter. Polling the key here, rather than checking for '\r'/'\n' in
    /// <see cref="OnTextInput"/>, is what the Input System actually guarantees: whether Enter is
    /// delivered as a text character depends on platform/IME and isn't reliable on every keyboard
    /// (notably Bluetooth keyboards on Quest), but the key's pressed state always is.
    /// </summary>
    private void Update()
    {
        if (_pendingNumericCheatPrefix == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            CommitPendingNumericCheat();
        }
    }

    /// <summary>
    /// Handles keyboard character input and checks for cheat code patterns.
    /// </summary>
    private void OnTextInput(char c)
    {
        if (!char.IsLetterOrDigit(c)){
            return;
        }

        _playerInput += c.ToString().ToLower();

        if (_playerInput.Length > _maxCheatLength)
        {
            _playerInput = _playerInput[^_maxCheatLength..];
        }
        
        CheckCheatCode();
    }

    /// <summary>
    /// Handles a cheat code received from the web app (<see cref="ConnectToWebApp.receiveCheatCode"/>).
    /// Rebuilds <see cref="_playerInput"/> from <paramref name="rawCode"/> using the exact same
    /// character filtering/lowercasing as <see cref="OnTextInput"/>, then runs it through
    /// <see cref="CheckCheatCode"/> so web-entered codes go through the same verification and
    /// gating logic (tutorial/intro checks, etc.) as keyboard-entered ones. The web panel only ever
    /// sends a complete code (on form submit, not per keystroke - see <c>CheatCodePanel.tsx</c>), so
    /// a numeric cheat match is committed immediately here instead of waiting for an Enter press.
    /// </summary>
    private void OnWebCheatCode(string rawCode)
    {
        _playerInput = "";

        foreach (char c in rawCode)
        {
            if (char.IsLetterOrDigit(c))
            {
                _playerInput += c.ToString().ToLower();
            }
        }

        CheckCheatCode();

        if (_pendingNumericCheatPrefix != null)
        {
            CommitPendingNumericCheat();
        }
    }

    protected virtual void CheckCheatCode()
    {
        Debug.LogError("CheckCheatCode should be overridden in derived classes.");
    }

    protected virtual void RegisterBaseCheats()
    {
        RegisterCheat("skip", () => _skipDialogue.Invoke());
    }

    /// <summary>
    /// Registers a cheat code with its associated command.
    /// Automatically updates _maxCheatLength so the input buffer is always large enough.
    /// </summary>
    /// <param name="code">The cheat code string.</param>
    /// <param name="command">The action to execute when the code is entered.</param>
    protected void RegisterCheat(string code, Action command)
    {
        _cheatCommands[code] = command;
        _maxCheatLength = Mathf.Max(_maxCheatLength, code.Length);
    }

    /// <summary>
    /// Registers a "prefix + optional number" cheat (e.g. "goal" for "goal100"). Unlike
    /// <see cref="RegisterCheat"/>, the code's length isn't fixed, so it can't be matched with a
    /// plain <c>Contains()</c> check - subclasses should call <see cref="TryMatchNumericCheat"/>
    /// from their <c>CheckCheatCode()</c> override to check for it.
    /// </summary>
    /// <param name="prefix">The literal text before the number, e.g. "goal".</param>
    /// <param name="defaultValue">Value used when the prefix is typed with no digits after it.</param>
    /// <param name="command">Invoked with the parsed number (or <paramref name="defaultValue"/>).</param>
    protected void RegisterNumericCheat(string prefix, int defaultValue, Action<int> command)
    {
        _numericCheatCommands[prefix] = (defaultValue, command);

        // +10 digits: enough buffer for any positive int, since the suffix length isn't fixed.
        _maxCheatLength = Mathf.Max(_maxCheatLength, prefix.Length + 10);
    }

    /// <summary>
    /// Checks <see cref="_playerInput"/> against every cheat registered via
    /// <see cref="RegisterNumericCheat"/>. Because the number suffix is variable-length, a match
    /// doesn't invoke the command immediately (that would fire on the prefix alone, with zero
    /// digits, before the player finishes typing the number) - it just records the match as
    /// pending, and <see cref="Update"/> commits it once the player presses Enter. Returns true as
    /// soon as a match is found, so callers can early-out the same way they would for a normal
    /// cheat match (and skip the "goal" text being checked against unrelated exact-match cheats).
    /// </summary>
    protected bool TryMatchNumericCheat()
    {
        string input = _playerInput.ToLower();

        foreach (string prefix in _numericCheatCommands.Keys)
        {
            if (Regex.IsMatch(input, $"^{Regex.Escape(prefix)}\\d*$"))
            {
                _pendingNumericCheatPrefix = prefix;

                return true;
            }
        }

        _pendingNumericCheatPrefix = null;

        return false;
    }

    /// <summary>
    /// Applies the pending numeric cheat (Enter was pressed, or the web app sent a complete code),
    /// then clears the input buffer (mirroring how every other cheat resets it after firing).
    /// </summary>
    private void CommitPendingNumericCheat()
    {
        if (_pendingNumericCheatPrefix == null || !_numericCheatCommands.TryGetValue(_pendingNumericCheatPrefix, out var entry))
        {
            return;
        }

        string digits = _playerInput.ToLower()[_pendingNumericCheatPrefix.Length..];

        int value = digits.Length > 0 && int.TryParse(digits, out int parsed) && parsed > 0
            ? parsed
            : entry.defaultValue;

        entry.command.Invoke(value);

        _playerInput = string.Empty;
        _pendingNumericCheatPrefix = null;
    }
}