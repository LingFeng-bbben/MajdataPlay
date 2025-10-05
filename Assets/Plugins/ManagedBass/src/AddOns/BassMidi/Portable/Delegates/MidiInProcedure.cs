using System;

namespace ManagedBass.Midi
{
    /// <summary>
    /// User defined callback delegate to receive MIDI data (to be used with <see cref="BassMidi.InInit" />).
    /// </summary>
    /// <param name="Device">The MIDI input device that the data is from.</param>
    /// <param name="Time">Timestamp, in seconds since <see cref="BassMidi.InStart" /> was called.</param>
    /// <param name="Buffer">Pointer to the MIDI data.</param>
    /// <param name="Length">The amount of data in bytes.</param>
    /// <param name="User">The user instance data given when <see cref="BassMidi.InInit" /> was called.</param>
    public delegate void MidiInProcedure(int Device, double Time, IntPtr Buffer, int Length, IntPtr User);
}