using System;

namespace ManagedBass
{
    /// <summary>
    /// Used with <see cref="Bass.RecordGetInput(int, out float)"/> applying <see cref="InputTypeMask"/> on the return value;
    /// </summary>
    [Flags]
    public enum InputTypeFlags
    {
        /// <summary>
        /// The Type of Input is also indicated in the high 8-bits of <see cref="Bass.RecordGetInput(int, out float)"/> (use this to test the return value).
        /// </summary>
        InputTypeMask = -16777216,

        /// <summary>
        /// The Type of Input is errorness.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Anything that is not covered by the other types
        /// </summary>
        Undefined,

        /// <summary>
        /// Digital Input source, for example, a DAT or audio CD.
        /// </summary>
        Digital = 0x1000000,

        /// <summary>
        /// Line-in. On some devices, "Line-in" may be combined with other analog sources into a single Analog Input.
        /// </summary>
        Line = 0x2000000,

        /// <summary>
        /// Microphone.
        /// </summary>
        Microphone = 0x3000000,

        /// <summary>
        /// Internal MIDI synthesizer.
        /// </summary>
        MIDISynthesizer = 0x4000000,

        /// <summary>
        /// Analog audio CD.
        /// </summary>
        AnalogCD = 0x5000000,

        /// <summary>
        /// Telephone.
        /// </summary>
        Phone = 0x6000000,

        /// <summary>
        /// PC speaker.
        /// </summary>
        Speaker = 0x7000000,

        /// <summary>
        /// The device's WAVE/PCM output.
        /// </summary>
        Wave = 0x8000000,

        /// <summary>
        /// Auxiliary. Like "Line-in", "Aux" may be combined with other analog sources into a single Analog Input on some devices.
        /// </summary>
        Auxiliary = 0x9000000,

        /// <summary>
        /// Analog, typically a mix of all analog sources (what you hear).
        /// </summary>
        Analog = 0xa000000
    }
}
