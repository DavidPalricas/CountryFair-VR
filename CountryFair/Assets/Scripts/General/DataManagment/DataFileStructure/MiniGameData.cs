using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Persisted data for a single mini-game: session records keyed by ISO timestamp and
/// a flexible dictionary of adaptive parameters (e.g., difficulty level, dog distance).
/// </summary>
[System.Serializable]
public class MiniGameData
{
    /// <summary>Session records keyed by ISO timestamp strings (e.g., "2024-01-01_10-00-00").</summary>
    public Dictionary<string, SessionData> SessionsData { get; set; } = new();

    /// <summary>Adaptive algorithm parameters stored as name-value string pairs for JSON flexibility.</summary>
    public Dictionary<string, string> AdaptiveParameters { get; set; } = new();

    /// <summary>Read-only alias for the misspelled key written by older builds, so existing save files still load.</summary>
    /// <remarks>Has no getter, so it is never written back; the next save stores the values under <see cref="AdaptiveParameters"/>.</remarks>
    [JsonProperty("AdadaptiveParameters")]
    private Dictionary<string, string> LegacyAdaptiveParameters { set => AdaptiveParameters = value; }

    public MiniGameData()
    {
        SessionsData = new Dictionary<string, SessionData>();
        AdaptiveParameters = new Dictionary<string, string>();
    }
}