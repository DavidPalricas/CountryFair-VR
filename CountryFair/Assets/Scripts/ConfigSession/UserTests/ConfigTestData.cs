using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>One grab-and-release of a tent during the configuration user test.</summary>
public class TentAttemptData
{
    /// <summary>Seconds between grabbing the tent and releasing it on a slot.</summary>
    [JsonProperty("grabDuration")]
    public double GrabDuration;

    /// <summary>Order (slot number) where the tent was left.</summary>
    [JsonProperty("placedOrder")]
    public int PlacedOrder;

    /// <summary>True if <see cref="PlacedOrder"/> is the order the survivor's file asks for.</summary>
    [JsonProperty("correct")]
    public bool Correct;
}

/// <summary>What happened to one tent while a survivor's file was being configured.</summary>
public class TentTestData
{
    /// <summary>Order the survivor's file asks for.</summary>
    [JsonProperty("expectedOrder")]
    public int ExpectedOrder;

    /// <summary>
    /// Seconds from the first grab of this tent until a grab left it in the correct slot. Null while it did not
    /// happen, so a tent that only became correct because another one was dropped on it has no time.
    /// </summary>
    [JsonProperty("totalTime", NullValueHandling = NullValueHandling.Ignore)]
    public double? TotalTime;

    /// <summary>True if the tent reached its correct slot because another tent was dropped on it (swap), not by grabbing it.</summary>
    [JsonProperty("correctedBySwap")]
    public bool CorrectedBySwap;

    /// <summary>Every grab of this tent, in order.</summary>
    [JsonProperty("attempts")]
    public List<TentAttemptData> Attempts = new();
}

/// <summary>Results of one selection mode (ray grab, tent menu or giant button) with its survivor.</summary>
public class SelectionModeTestData
{
    /// <summary>Name of the survivor whose file was configured.</summary>
    [JsonProperty("survivor")]
    public string Survivor;

    /// <summary>Seconds from showing the survivor's file until the whole configuration matched it. Null until then.</summary>
    [JsonProperty("configTime", NullValueHandling = NullValueHandling.Ignore)]
    public double? ConfigTime;

    /// <summary>Data of every tent that was grabbed (or corrected by a swap), by mini-game name.</summary>
    [JsonProperty("tents")]
    public Dictionary<string, TentTestData> Tents = new();
}
