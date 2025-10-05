using System;

namespace ManagedBass.Midi
{    
	/// <summary>
	/// The type of event data to apply, to be used with <see cref="BassMidi.StreamEvents(int,MidiEventsMode,IntPtr,int)" />.
	/// </summary>
    [Flags]
    public enum MidiEventsMode
    {
        /// <summary>
        /// An array of <see cref="MidiEvent" /> structures (Default).
        /// </summary>
        Struct,

        /// <summary>
        /// Raw MIDI event data, as would be sent to a MIDI device. 
		/// Running status is supported.
		/// To overcome the 16 channel limit, the event data's channel information can optionally be overridden by adding the new channel number to this parameter, where +1 = the 1st channel.
        /// </summary>
        Raw = 0x10000,

        /// <summary>
        /// Trigger event syncs
        /// </summary>
        Sync = 0x1000000,

        /// <summary>
        /// Disable running status meaning each event must include a status byte.
        /// </summary>
        NoRunningStatus = 0x2000000,

        /// <summary>
        /// Cancel pending events
        /// </summary>
        Cancel = 0x4000000,

        /// <summary>
        /// Delta-time info is present
        /// </summary>
        Time = 0x8000000
    }
}