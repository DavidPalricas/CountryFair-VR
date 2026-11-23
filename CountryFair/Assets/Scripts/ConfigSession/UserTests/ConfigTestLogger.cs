using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Records the configuration user test in a JSON file: for every survivor configured, the time to configure the fair
/// (<see cref="SelectionModeTestData.ConfigTime"/>) and, per tent, how long each grab took and whether it left the tent
/// in the slot the survivor's file asks for. The file is a JSON array with one object per run (<c>id</c>,
/// <c>sequence</c>, <c>totalConfigTime</c> and one key per mode), and every run is appended.
///
/// <see cref="ConfigSessionManager"/> keeps two of these: one for the headset (<c>userTestData.json</c>, one key per
/// selection mode) and one for the web app (<c>webAppTestData.json</c>, one key per device). Each one only writes what
/// was configured through it: a mode is added on the first grab it receives, and a run is added to the file only then,
/// so a run configured entirely on the web leaves nothing in the headset's file, and vice versa.
///
/// Only a tent that was grabbed gets a time. A tent that reaches its correct slot because another one was dropped on
/// it is just flagged (<see cref="TentTestData.CorrectedBySwap"/>).
///
/// It is a plain class driven by <see cref="ConfigSessionManager"/>. It does not read the file in its constructor
/// (Unity forbids <see cref="Application.persistentDataPath"/> there): the first grab does.
/// </summary>
public class ConfigTestLogger
{
    /// <summary>Decimal places kept in the stored times.</summary>
    private const int TimeDecimals = 2;

    /// <summary>Name of the file, in <see cref="Application.persistentDataPath"/> on Android and in the project root in the Editor.</summary>
    private readonly string _fileName;

    /// <summary>Order each tent is in right now. Filled even before <see cref="StartSession"/>, as tents report in their Awake.</summary>
    private readonly Dictionary<GameManager.MiniGames, int> _currentOrders = new();

    /// <summary>Order the current survivor's file asks for, by mini-game.</summary>
    private readonly Dictionary<GameManager.MiniGames, int> _expectedOrders = new();

    /// <summary>Time of the first grab of each tent in the current mode.</summary>
    private readonly Dictionary<GameManager.MiniGames, float> _firstGrabTimes = new();

    /// <summary>Results of every mode used in this run, by key (selection mode or device).</summary>
    private readonly Dictionary<string, SelectionModeTestData> _modes = new();

    /// <summary>Keys of <see cref="_modes"/> in the order they were first used, for a run without a planned sequence.</summary>
    private readonly List<string> _usedKeys = new();

    private string _filePath;

    private JArray _root;

    /// <summary>The object of this run inside <see cref="_root"/>; null until something is grabbed.</summary>
    private JObject _session;

    private string _sessionId;

    /// <summary>Planned order of the modes, or null to write the keys in the order they were used.</summary>
    private List<string> _plannedSequence;

    private string _modeName;

    /// <summary>Survivor of the current mode, kept until the first grab creates <see cref="_mode"/>.</summary>
    private string _survivorName;

    /// <summary>Data of the current mode; null until a tent is grabbed in it.</summary>
    private SelectionModeTestData _mode;

    private bool _modeCompleted;

    private float _modeStartTime;

    /// <summary>Tent being held, if any.</summary>
    private GameManager.MiniGames? _grabbedGame;

    private float _grabStartTime;

    /// <param name="fileName">Name of the JSON file this logger appends its runs to.</param>
    public ConfigTestLogger(string fileName)
    {
        _fileName = fileName;
    }

    /// <summary>Starts a new run. Nothing is written until a tent is grabbed.</summary>
    /// <param name="sequence">
    /// Planned modes of this run, in the order they will be tested, or null to store the keys in the order they are used.
    /// </param>
    public void StartSession(IEnumerable<string> sequence = null)
    {
        _sessionId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _plannedSequence = sequence?.ToList();
    }

    /// <summary>Starts timing a survivor's file. The mode only enters the file once a tent is grabbed in it.</summary>
    /// <param name="modeName">Name of the selection mode, the key in the file unless the grabs give another one.</param>
    /// <param name="survivor">Survivor whose file is being configured.</param>
    public void BeginMode(string modeName, StrokeSurvivorProfile survivor)
    {
        if (_sessionId == null)
        {
            return;
        }

        _modeName = modeName;
        _survivorName = survivor.survivorName;
        _mode = null;
        _modeCompleted = false;

        _modeStartTime = Time.realtimeSinceStartup;
        _grabbedGame = null;

        _expectedOrders.Clear();
        _firstGrabTimes.Clear();

        foreach (TentConfigData expected in survivor.tents)
        {
            _expectedOrders[expected.MiniGame] = expected.Order;
        }
    }

    /// <summary>
    /// Reports the current configuration of a tent. A tent that is not the one being held and just reached its
    /// correct order while a tent of this logger was being dropped was moved there by a swap.
    /// </summary>
    public void OnTentConfigChanged(TentConfigData config)
    {
        bool orderChanged = _currentOrders.TryGetValue(config.MiniGame, out int previousOrder) && previousOrder != config.Order;

        _currentOrders[config.MiniGame] = config.Order;

        // With no tent of this logger held, the change came from the other surface (headset or web), which logs it.
        if (_mode == null || !orderChanged || _grabbedGame == null || config.MiniGame == _grabbedGame)
        {
            return;
        }

        if (_expectedOrders.TryGetValue(config.MiniGame, out int expectedOrder) && config.Order == expectedOrder)
        {
            GetTent(config.MiniGame).CorrectedBySwap = true;

            Save();
        }
    }

    /// <summary>A tent was grabbed (<paramref name="isSelected"/> true) or released and snapped (false).</summary>
    /// <param name="game">Tent grabbed or released.</param>
    /// <param name="isSelected">True on grab, false on release.</param>
    /// <param name="modeKey">Key of the mode in the file (the web app's device); null to use the selection mode's name.</param>
    /// <param name="grabDuration">Seconds the tent was held, measured where it was dragged (the web app); null to measure it here.</param>
    public void OnTentSelectionChanged(GameManager.MiniGames game, bool isSelected, string modeKey = null, double? grabDuration = null)
    {
        // Not gated by _modeCompleted: the last drop completes the mode before its release arrives, and must still count.
        if (_modeName == null)
        {
            return;
        }

        if (isSelected)
        {
            EnsureMode(modeKey ?? _modeName);

            _grabbedGame = game;
            _grabStartTime = Time.realtimeSinceStartup;

            _firstGrabTimes.TryAdd(game, _grabStartTime);

            return;
        }

        if (_mode == null || _grabbedGame != game)
        {
            return;
        }

        _grabbedGame = null;

        // The managers already swapped the tents, so the order is the final one.
        int placedOrder = _currentOrders.GetValueOrDefault(game);
        float now = Time.realtimeSinceStartup;

        TentTestData tent = GetTent(game);

        bool correct = placedOrder == tent.ExpectedOrder;

        tent.Attempts.Add(new TentAttemptData
        {
            GrabDuration = Round(grabDuration ?? now - _grabStartTime),
            PlacedOrder = placedOrder,
            Correct = correct
        });

        if (correct && tent.TotalTime == null)
        {
            tent.TotalTime = Round(now - _firstGrabTimes[game]);
        }

        Save();
    }

    /// <summary>
    /// The tents now match the survivor's file: stores the time of the mode and the sum of all modes. Nothing is
    /// stored when no tent was grabbed through this logger in the mode.
    /// </summary>
    public void CompleteMode()
    {
        if (_modeCompleted)
        {
            return;
        }

        _modeCompleted = true;

        if (_mode == null)
        {
            return;
        }

        _mode.ConfigTime = Round(Time.realtimeSinceStartup - _modeStartTime);

        _session["totalConfigTime"] = Round(_modes.Values.Sum(mode => mode.ConfigTime ?? 0));

        Save();
    }

    /// <summary>Creates the data of the current mode on its first grab, and the run in the file on the first grab of all.</summary>
    /// <param name="key">Key of the mode in the file. A key already used in this run gets a suffix (<c>Tablet_2</c>).</param>
    private void EnsureMode(string key)
    {
        if (_mode != null)
        {
            return;
        }

        EnsureSession();

        string uniqueKey = key;

        for (int i = 2; _modes.ContainsKey(uniqueKey); i++)
        {
            uniqueKey = $"{key}_{i}";
        }

        _mode = new SelectionModeTestData { Survivor = _survivorName };
        _modes[uniqueKey] = _mode;
        _usedKeys.Add(uniqueKey);

        if (_plannedSequence == null)
        {
            _session["sequence"] = new JArray(_usedKeys);
        }
    }

    /// <summary>Loads the file and appends this run to it, once.</summary>
    private void EnsureSession()
    {
        if (_session != null)
        {
            return;
        }

        _filePath = GetFilePath();

        _root = LoadRoot();

        _session = new JObject
        {
            ["id"] = _sessionId,
            ["sequence"] = new JArray(_plannedSequence ?? _usedKeys),
            ["totalConfigTime"] = 0.0
        };

        _root.Add(_session);
    }

    /// <summary>Returns the data of a tent in the current mode, creating it (with the expected order) on first use.</summary>
    private TentTestData GetTent(GameManager.MiniGames game)
    {
        string key = game.ToString();

        if (!_mode.Tents.TryGetValue(key, out TentTestData tent))
        {
            tent = new TentTestData { ExpectedOrder = _expectedOrders.GetValueOrDefault(game) };

            _mode.Tents[key] = tent;
        }

        return tent;
    }

    private static double Round(double seconds) => Math.Round(seconds, TimeDecimals);

    private string GetFilePath()
    {
        #if UNITY_EDITOR
            return Path.Combine(Directory.GetParent(Application.dataPath).ToString(), _fileName);
        #else
            return Path.Combine(Application.persistentDataPath, _fileName);
        #endif
    }

    /// <summary>Reads the runs already in the file; an unreadable file is kept as <c>.corrupt</c> instead of being overwritten.</summary>
    private JArray LoadRoot()
    {
        if (!File.Exists(_filePath))
        {
            return new JArray();
        }

        try
        {
            return JArray.Parse(File.ReadAllText(_filePath));
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigTestLogger] Could not read {_filePath}, keeping it as .corrupt: {e.Message}");

            File.Move(_filePath, _filePath + ".corrupt");

            return new JArray();
        }
    }

    /// <summary>Writes the whole file; the data is small, so every change is saved and a crash loses nothing.</summary>
    private void Save()
    {
        if (_session == null)
        {
            return;
        }

        // Replaces each mode's key with its current data, flat next to "id" and "sequence".
        foreach (KeyValuePair<string, SelectionModeTestData> mode in _modes)
        {
            _session[mode.Key] = JToken.FromObject(mode.Value);
        }

        try
        {
            File.WriteAllText(_filePath, _root.ToString(Formatting.Indented));
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigTestLogger] Error saving {_filePath}: {e.Message}");
        }
    }
}
