using System;

namespace ManagedBass.Midi
{    
	/// <summary>
	/// Font Init Flags to be used with <see cref="BassMidi.FontInit(string,FontInitFlags)"/>.
	/// </summary>
    [Flags]
    public enum FontInitFlags
    {
        /// <summary>
        /// Map the file into memory.
        /// This flag is ignored if the soundfont is packed as the sample data cannot be played directly from a mapping; it needs to be decoded.
        /// This flag is also ignored if the file is too large to be mapped into memory.
        /// </summary>
        MemoryMap = 0x20000,

        /// <summary>
        /// Use bank 127 in the soundfont for XG drum kits.
        /// When an XG drum kit is needed, bank 127 in soundfonts that have this flag set will be checked first, before falling back to bank 128 (the standard SF2 drum kit bank) if it is not available there.
        /// </summary>
        XGDrums = 0x40000,

        /// <summary>
        /// Ignore the reverb/chorus levels of the presets in the soundfont (only use the levels in the MIDI events).
        /// </summary>
        NoFx = 0x80000,

        /// <summary>
        /// Unicode File Names.
        /// </summary>
        Unicode = -2147483648
    }
}