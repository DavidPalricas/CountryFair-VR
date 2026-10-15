
/// <summary>
/// Root JSON structure of <c>survivorData.json</c>.
/// Contains persistent session history and adaptive parameters for each mini-game.
/// </summary>
/// <remarks>
/// Field names are written verbatim as JSON keys by Newtonsoft.Json, so renaming them changes the
/// save-file format.
/// </remarks>
[System.Serializable]
public class DataFileRoot
{
    /// <summary>Frisbee mini-game session history and adaptive parameters.</summary>
    public MiniGameData frisbeeGame = new ();
    /// <summary>Archery mini-game session history and adaptive parameters.</summary>
    public MiniGameData archeryGame = new ();
}