using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Used with <see cref="BassMidi.StreamEvents(int,MidiEventsMode,MidiEvent[],int)"/> to apply events and <see cref="BassMidi.StreamGetEvents(int,int,MidiEventType,MidiEvent[])"/> to retrieve events, and <see cref="BassMidi.CreateStream(MidiEvent[],int,BassFlags,int)"/> to play event sequences.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiEvent
    {
        /// <summary>
        /// The Event Type
        /// </summary>
        public MidiEventType EventType;

        /// <summary>
        /// The Event Parameter
        /// </summary>
        public int Parameter;

        /// <summary>
        /// The MIDI Channel of the event... 0 = channel 1
        /// </summary>
        public int Channel;

        /// <summary>
        /// The Position of the event, in ticks
        /// </summary>
        public int Ticks;

        /// <summary>
        /// The Position of the event, in bytes
        /// </summary>
        public int Position;
    }
}