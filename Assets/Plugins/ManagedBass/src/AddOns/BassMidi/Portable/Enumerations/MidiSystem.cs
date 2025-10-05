namespace ManagedBass.Midi
{
    /// <summary>
    /// System mode parameter, to be used with <see cref="MidiEventType.System" />, <see cref="MidiEventType.SystemEx" />.
    /// </summary>
    /// <remarks>
    /// <see cref="Default"/> is identical to <see cref="GS"/>, except that channel 10 is melodic if there are not 16 channels.
    /// </remarks>
    public enum MidiSystem
    {
        /// <summary>
        /// System default. 
        /// </summary>
        Default,

        /// <summary>
        /// General MIDI Level 1.
        /// </summary>
        GM1,

        /// <summary>
        /// General MIDI Level 2.
        /// </summary>
        GM2,

        /// <summary>
        /// XG-Format (Yamaha).
        /// </summary>
        XG,

        /// <summary>
        /// GS-Format (Roland).
        /// </summary>
        GS
    }
}