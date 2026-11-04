using FMOD;

/// <summary>
/// Clip prepared by <see cref="DialogueNarration"/>, handed to its FMOD callback through the event instance's user data
/// so the Programmer Instrument plays it.
/// </summary>
internal class ProgrammerSoundData
{
    /// <summary>Sound (the audio table's FSB) created from the narration key.</summary>
    public Sound sound;

    /// <summary>Index of the clip inside <see cref="sound"/>.</summary>
    public int subsoundIndex;
}
