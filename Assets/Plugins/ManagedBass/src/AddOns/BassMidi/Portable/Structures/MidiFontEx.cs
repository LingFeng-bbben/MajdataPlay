using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
	/// Used with <see cref="BassMidi.StreamSetFonts(int,MidiFontEx[],int)" /> and <see cref="BassMidi.StreamGetFonts(int,MidiFontEx[],int)" /> to set and retrieve soundfont configurations.
	/// </summary>
	/// <remarks>
    /// This is an extended version of the <see cref="MidiFont" /> structure that allows more flexible mapping of soundfont presets to MIDI programs, including access to the bank LSB (eg. MIDI controller 32).
	/// <para>
    /// When using an individual preset from a soundfont, BASSMIDI will first look for the exact <see cref="SoundFontPreset"/> and <see cref="SoundFontBank"/> match, but if that is not present, the first preset from the soundfont will be used.
    /// This is useful for single preset soundfonts.
    /// Individual presets can be assigned to program numbers beyond the standard 127 limit, up to 65535, which can then be accessed via <see cref="BassMidi.StreamEvent(int,int,MidiEventType,int)" />.
    /// </para>
	/// <para>
    /// When using all presets from all banks in a soundfont, the <see cref="DestinationBank"/> member is a base number that is added to the soundfont's banks.
    /// For example, if <see cref="DestinationBank"/> = 1 then the soundfont's bank 0 becomes bank 1, etc.
    /// Negative base numbers are allowed, to lower a soundfont's bank numbers.
    /// </para>
	/// <para>
    /// The bank LSB raises the maximum number of melodic banks from 128 to 16384 (128 x 128).
    /// But, the SF2 soundfont format only supports 128 banks.
    /// So a soundfont that is set to be used on all banks (<see cref="DestinationPreset"/> and <see cref="DestinationBank"/> are -1) will still only apply to the single bank LSB specified by <see cref="DestinationBankLSB"/>.
    /// </para>
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
    public struct MidiFontEx
    {
        /// <summary>
        /// Soundfont handle, previously inititialized with <see cref="BassMidi.FontInit(string,FontInitFlags)" />.
        /// </summary>
        public int Handle;

        /// <summary>
		/// Soundfont preset number... 0-127, -1 = use all presets.
		/// </summary>
		public int SoundFontPreset;

        /// <summary>
		/// Soundfont bank number... 0-128, -1 = use all banks.
		/// </summary>
		public int SoundFontBank;

        /// <summary>
		/// Destination preset/program number... 0-65535, -1 = all presets.
		/// This determines what <see cref="MidiEventType.Program"/> event value(s) the soundfont is used for.
		/// </summary>
		public int DestinationPreset;

        /// <summary>
        /// Destination bank number, or a base bank number when using all presets from all banks.
        /// This determines what <see cref="MidiEventType.Bank"/> event value(s) the soundfont is used for.
        /// </summary>
        public int DestinationBank;

        /// <summary>
        /// Destination bank number LSB.
        /// This is the <see cref="MidiEventType.BankLSB"/> event value that the soundfont is used for.
        /// </summary>
        public int DestinationBankLSB;
    }
}