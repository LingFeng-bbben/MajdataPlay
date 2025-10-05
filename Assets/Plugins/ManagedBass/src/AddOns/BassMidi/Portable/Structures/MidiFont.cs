using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Used with <see cref="BassMidi.StreamSetFonts(int,MidiFont[],int)" /> and <see cref="BassMidi.StreamGetFonts(int,MidiFont[],int)" /> to set and retrieve soundfont configurations.
    /// </summary>
    /// <remarks>
    /// When using an individual preset from a soundfont, BASSMIDI will first look for the exact preset and bank match, but if that is not present, the first preset from the soundfont will be used.
    /// This is useful for single preset soundfonts.
    /// Individual presets can be assigned to program numbers beyond the standard 127 limit, up to 65535, which can then be accessed via <see cref="BassMidi.StreamEvent(int,int,MidiEventType,int)"/>.
    /// <para>
    /// When using all presets in a soundfont, the bank member is a base number that is added to the soundfont's banks.
    /// For example, if bank=1 then the soundfont's bank 0 becomes bank 1, etc.
    /// Negative base numbers are allowed.
    /// </para>
    /// <para>For more flexible mapping of soundfont presets to MIDI programs, see the <see cref="MidiFontEx" /> structure.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiFont
    {
        /// <summary>
        /// Soundfont handle, previously inititialized with <see cref="BassMidi.FontInit(string,FontInitFlags)" />.
        /// </summary>
        public int Handle;

        /// <summary>
        /// Preset number... 0-65535, -1 = use all presets in the soundfont.
        /// This determines what <see cref="MidiEventType.Program"/> event value(s) the soundfont is used for.
        /// </summary>
        public int Preset;

        /// <summary>
        /// Base bank number, or the bank number of the individual preset.
        /// This determines what <see cref="MidiEventType.Bank"/> event value(s) the soundfont is used for.
        /// </summary>
        public int Bank;
    }
}