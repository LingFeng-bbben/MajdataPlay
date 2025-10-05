using System;

namespace ManagedBass.Midi
{
    /// <summary>
    /// The Marker type, to be used with <see cref="BassMidi.StreamGetMark(int,MidiMarkerType,int,out MidiMarker)" />.
    /// </summary>
    [Flags]
    public enum MidiMarkerType
    {
        /// <summary>
        /// Marker events (MIDI meta event 6).
        /// </summary>
        Marker,

        /// <summary>
        /// Cue events (MIDI meta event 7).
        /// </summary>
        CuePoint,

        /// <summary>
        /// Lyric events (MIDI meta event 5).
        /// </summary>
        Lyric,

        /// <summary>
        /// Text events (MIDI meta event 1).
        /// </summary>
        Text,

        /// <summary>
        /// Time signature event (MIDI meta event 88).
        /// The time signature events are given in the form of "numerator/denominator metronome-pulse 32nd-notes-per-MIDI-quarter-note", eg. "4/4 24 8".
        /// </summary>
        TimeSignature,

        /// <summary>
		/// Key signature events (MIDI meta event 89).
		/// That gives the key signature (in <see cref="MidiMarker.Text"/>) in the form of "a b", where "a" is the number of sharps (if positive) or flats (if negative) and "b" signifies major (if 0) or minor (if 1).
		/// </summary>
        KeySignature,

        /// <summary>
        /// Copyright notice (MIDI meta event 2).
        /// </summary>
        Copyright,

        /// <summary>
        /// Track name events (MIDI meta event 3).
        /// </summary>
        TrackName,

        /// <summary>
        /// Instrument name events (MIDI meta event 4). 
        /// </summary>
        InstrumentName,

        /// <summary>
        /// FLAG: get position in ticks instead of bytes
        /// </summary>
        Tick = 0x10000
    }
}