using System;

namespace ManagedBass.Midi
{
    /// <summary>
    /// User defined callback to filter events.
    /// </summary>
    /// <param name="Handle">The MIDI stream handle.</param>
    /// <param name="Track">The track that the event is from... 0 = 1st track.</param>
    /// <param name="Event">The event structure.</param>
    /// <param name="Seeking">true = the event is being processed while seeking, false = the event is being played.</param>
    /// <param name="User">The user instance data given when <see cref="BassMidi.StreamSetFilter"/> was called.</param>
    /// <returns>true to process the event, and false to drop the event.</returns>
    /// <remarks>
    /// The event's type, parameter, and channel can be modified, but not its position.
    /// It is also possible to apply additional events at the same time via <see cref="BassMidi.StreamEvent(int, int, MidiEventType, byte, byte)"/>, but not <see cref="BassMidi.StreamEvents(int, MidiEventsMode, byte[], int)"/>.
    /// <see cref="MidiEventType.Note"/>, <see cref="MidiEventType.NotesOff"/>, and <see cref="MidiEventType.SoundOff"/> events are ignored while seeking so they will not be received by a filtering function then.
    /// <see cref="MidiEventType.Tempo"/> events can be changed while seeking but doing so when seeking in bytes (<see cref="PositionFlags.Bytes"/>) will result in reaching a different position.
    /// Seeking in ticks (<see cref="PositionFlags.MIDITick"/>) is unaffected by tempo changes.
    /// The <see cref="MidiEventType.Speed"/> MIDI_EVENT_SPEED event can be used to modify the tempo without affecting seeking.
    /// </remarks>
    public delegate bool MidiFilterProcedure(int Handle, int Track, MidiEvent Event, bool Seeking, IntPtr User);
}
