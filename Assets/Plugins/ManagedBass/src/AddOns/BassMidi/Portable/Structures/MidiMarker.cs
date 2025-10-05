using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Used with <see cref="BassMidi.StreamGetMark(int,MidiMarkerType,int,out MidiMarker)" />, <see cref="SyncFlags.MidiLyric" />, <see cref="SyncFlags.MidiCue" /> and <see cref="SyncFlags.MidiMarker" /> to retrieve markers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiMarker
    {
        int track;
        int pos;
        IntPtr text;

        /// <summary>
        /// The marker, cue, lyric, keysig, timesig text.
        /// </summary>
        /// <remarks>
        /// If the lyric text begins with a '/' (slash) character, a new line should be started. 
        /// If it begins with a '\' (backslash) character, the display should be cleared.
        /// <para>
        /// For a key signature event (MIDI meta event 89).
        /// The marker text is in the form of "a b", where a is the number of sharps (if positive) or flats (if negative), and b signifies major (if 0) or minor (if 1).
        /// </para>
        /// <para>
        /// For a time signature events (MIDI meta event 88).
        /// The marker text is in the form of "a/b c d", where a is the numerator, b is the denominator, c is the metronome pulse, and d is the number of 32nd notes per MIDI quarter-note.
        /// </para>
        /// </remarks>
        public string Text => text == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(text);

        /// <summary>
        /// The MIDI track (number) containing marker (0=first).
        /// </summary>
        public int Track => track;

        /// <summary>
        /// The position (in bytes) of the marker.
        /// </summary>
        public int Position => pos;
    }
}