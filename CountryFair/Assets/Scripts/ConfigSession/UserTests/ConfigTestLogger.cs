using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Records the configuration user test in <c>userTestData.json</c>: for every selection mode, the time to configure
/// the fair (<see cref="SelectionModeTestData.ConfigTime"/>) and, per tent, how long each grab took and whether it
/// left the tent in the slot the survivor's file asks for. The file is a JSON array with one object per run
/// (<c>id</c>, <c>sequence</c>, <c>totalConfigTime</c> and one key per selection mode), and every run is appended.
///
/// Only a tent that was grabbed gets a time. A tent that reaches its correct slot because another one was dropped on
/// it is just flagged (<see cref="TentTestData.CorrectedBySwap"/>).
///
/// It is a plain class driven by <see cref="ConfigSessionManager"/>. It does not read the file in its constructor
/// (Unity forbids <see cref="Application.persistentDataPath"/> there): <see cref="StartSession"/> does.
/// </summary>
public class ConfigTestLogger
{
    /// <summary>Name of the file, in <see cref="Application.persistentDataPath"/> on Android and in the project root in the Editor.</summary>
    private const string FileName = "userTestData.json";

    /// <summary>Decimal places kept in the stored times.</summary>
    private const int TimeDecimals = 2;

    /// <summary>Order each tent is in right now. Filled even before <see cref="StartSession"/>, as tents report in their Awake.</summary>
    private readonly Dictionary<GameManager.MiniGames, int> _currentOrders = new();

    /// <summary>Order the current survivor's file asks for, by mini-game.</summary>
    private readonly Dictionary<GameManager.MiniGames, int> _expectedOrders = new();

    /// <summary>Time of the first grab of each tent in the current mode.</summary>
    private readonly Dictionary<GameManager.MiniGames, float> _firstGrabTimes = new();

    /// <summary>Results of every mode started in this run, by mode name.</summary>
    private readonly Dictionary<string, SelectionModeTestData> _modes = new();

    private string _filePath;

    private JArray _root;

    /// <summary>The object of this run inside <see cref="_root"/>.</summary>
    private JObject _session;

    private string _modeName;

    private SelectionModeTestData _mode;

    private float _modeStartTime;

    /// <summary>Tent being held, if any.</summary>
    private GameManager.MiniGames? _grabbedGame;

    private float _grabStartTime;

    /// <summary>Loads the file and appends a new run to it.</summary>
    /// <param name="sequence">Selection modes of this run, in the order they will be tested.</param>
    public void StartSession(IEnumerable<string> sequence)
    {
        _filePath = GetFilePath();

        _root = LoadRoot();

        _session = new JObject
        {
            ["id"] = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            ["sequence"] = new JArray(sequence),
            ["totalConfigTime"] = 0.0
        };

        _root.Add(_session);

        Save();
    }

    /// <summary>Starts timing a selection mode with its survivor.</summary>
    /// <param name="modeName">Name of the selection mode, used as the key in the file.</param>
    /// <param name="survivor">Survivor whose file is being configured.</param>
    public void BeginMode(string modeName, StrokeSurvivorProfile survivor)
    {
        if (_session == null)
        {
            return;
        }

        _modeName = modeName;
        _mode = new SelectionModeTestData { Survivor = survivor.survivorName };
        _modes[modeName] = _mode;

        _modeStartTime = Time.realtimeSinceStartup;
        _grabbedGame = null;

        _expectedOrders.Clear();
        _firstGrabTimes.Clear();

        foreach (TentConfigData expected in survivor.tents)
        {
            _expectedOrders[expected.MiniGame] = expected.Order;
        }

        Save();
    }

    /// <summary>
    /// Reports the current configuration of a tent. A tent that is not the one being held and just reached its
    /// correct order was moved there by a swap.
    /// </summary>
    public void OnTentConfigChanged(TentConfigData config)
    {
        bool orderChanged = _currentOrders.TryGetValue(config.MiniGame, out int previousOrder) && previousOrder != config.Order;

        _currentOrders[config.MiniGame] = config.Order;

        if (_mode == null || !orderChanged || config.MiniGame == _grabbedGame)
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
    public void OnTentSelectionChanged(GameManager.MiniGames game, bool isSelected)
    {
        if (_mode == null)
        {
            return;
        }

        if (isSelected)
        {
            _grabbedGame = game;
            _grabStartTime = Time.realtimeSinceStartup;

            _firstGrabTimes.TryAdd(game, _grabStartTime);

            return;
        }

        if (_grabbedGame != game)
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
            GrabDuration = Round(now - _grabStartTime),
            PlacedOrder = placedOrder,
            Correct = correct
        });

        if (correct && tent.TotalTime == null)
        {
            tent.TotalTime = Round(now - _firstGrabTimes[game]);
        }

        Save();
    }

    /// <summary>The tents now match the survivor's file: stores the time of the mode and the sum of all modes.</summary>
    public void CompleteMode()
    {
        if (_mode == null || _mode.ConfigTime != null)
        {
            return;
        }

        _mode.ConfigTime = Round(Time.realtimeSinceStartup - _modeStartTime);

        _session["totalConfigTime"] = Round(_modes.Values.Sum(mode => mode.ConfigTime ?? 0));

        Save();
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

    private static string GetFilePath()
    {
        #if UNITY_EDITOR
            return Path.Combine(Directory.GetParent(Application.dataPath).ToString(), FileName);
        #else
            return Path.Combine(Application.persistentDataPath, FileName);
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
