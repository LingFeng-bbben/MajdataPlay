using System;

namespace ManagedBass
{
    /// <summary>
    /// Sync types to be used with <see cref="Bass.ChannelSetSync" /> (param flag) and <see cref="SyncProcedure" /> (data flag).
    /// </summary>
    [Flags]
    public enum SyncFlags
    {
        /// <summary>
        /// FLAG: sync only once, else continuously
        /// </summary>
        Onetime = -2147483648,

        /// <summary>
        /// FLAG: sync at mixtime, else at playtime
        /// </summary>
        Mixtime = 1073741824,

        /// <summary>
        /// Call the sync asynchronously in the dedicated sync thread. This only affects mixtime syncs (except <see cref="SyncFlags.Free"/> syncs)
        /// and allows the callback function to safely call <see cref="Bass.StreamFree"/> or <see cref="Bass.MusicFree"/> on the same channel handle. 
        /// </summary>
        Thread = 0x20000000,

        /// <summary>
        /// Sync when a channel reaches a position.
        /// param : position in bytes
        /// data : not used
        /// </summary>
        Position = 0,

        /// <summary>
        /// Sync when an instrument (sample for the non-instrument based formats) is played in a MOD music (not including retrigs).
        /// param : LOWORD=instrument (1=first) HIWORD=note (0=c0...119=b9, -1=all)
        /// data : LOWORD=note HIWORD=volume (0-64)
        /// </summary>
        MusicInstrument = 1,

        /// <summary>
        /// Sync when a channel reaches the end.
        /// param : not used
        /// data : 1 = the sync is triggered by a backward jump in a MOD music, otherwise not used
        /// </summary>
        End = 2,

        /// <summary>
        /// Sync when the "sync" effect (XM/MTM/MOD: E8x/Wxx, IT/S3M: S2x) is used.
        /// param : 0:data=Position, 1:data="x" value
        /// data : param=0: LOWORD=order HIWORD=row, param=1: "x" value
        /// </summary>
        MusicFx = 3,

        /// <summary>
        /// Sync when metadata is received in a stream.
        /// param : not used
        /// data : not used - the updated metadata is available from <see cref="Bass.ChannelGetTags"/>
        /// </summary>
        MetadataReceived = 4,

        /// <summary>
        /// Sync when an attribute slide is completed.
        /// param : not used
        /// data : the Type of slide completed (one of the <see cref="ChannelAttribute"/> values)
        /// </summary>
        Slided = 5,

        /// <summary>
        /// Sync when playback has stalled.
        /// param : not used
        /// data : 0=stalled, 1=resumed
        /// </summary>
        Stalled = 6,

        /// <summary>
        /// Sync when downloading of an internet (or "buffered" User file) stream has ended.
        /// param : not used
        /// data : not used
        /// </summary>
        Downloaded = 7,

        /// <summary>
        /// Sync when a channel is freed.
        /// param : not used
        /// data : not used
        /// </summary>
        Free = 8,

        /// <summary>
        /// Sync when a MOD music reaches an order:row position.
        /// param : LOWORD=order (0=first, -1=all) HIWORD=row (0=first, -1=all)
        /// data : LOWORD=order HIWORD=row
        /// </summary>
        MusicPosition = 10,

        /// <summary>
        /// Sync when seeking (inc. looping and restarting).
        /// So it could be used to reset DSP/etc.
        /// param : position in bytes
        /// data : 0=playback is unbroken, 1=if is it broken (eg. Buffer flushed).
        /// The latter would be the time to reset DSP/etc.
        /// </summary>
        Seeking = 11,

        /// <summary>
        /// Sync when a new logical bitstream begins in a chained OGG stream.
        /// Updated tags are available from <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used
        /// </summary>
        OggChange = 12,

        /// <summary>
        /// Sync when the DirectSound Buffer fails during playback, eg. when the device is no longer available.
        /// param : not used
        /// data : not used
        /// </summary>
        Stop = 14,

        /// <summary>
        /// WINAMP add-on: Sync when bitrate is changed or retrieved from a winamp Input plug-in.
        /// param : not used
        /// data : the bitrate retrieved from the winamp Input plug-in -
        /// called when it is retrieved or changed (VBR MP3s, OGGs, etc).
        /// </summary>
        WinampBitRate = 100,

        #region BassCd
        /// <summary>
        /// CD add-on: Sync when playback is stopped due to an error.
        /// For example, the drive door being opened.
        /// param : not used
        /// data : the position that was being read from the CD track at the time.
        /// </summary>
        CDError = 1000,

        /// <summary>
        /// CD add-on: Sync when the read speed is automatically changed due to the BassCd.AutoSpeedReduction setting.
        /// param : not used
        /// data : the new read speed.
        /// </summary>
        CDSpeed = 1002,
        #endregion

        #region BassMidi
        /// <summary>
        /// MIDI add-on: Sync when a marker is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker) call.
        /// </summary>
        MidiMarker = 0x10000,

        /// <summary>
        /// MIDI add-on: Sync when a cue is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker) call.
        /// </summary>
        MidiCue = 0x10001,

        /// <summary>
        /// MIDI add-on: Sync when a lyric event is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker) call.
        /// If the text begins with a '/' (slash) character, a new line should be started.
        /// If it begins with a '\' (backslash) character, the display should be cleared.
        /// </summary>
        MidiLyric = 0x10002,

        /// <summary>
        /// MIDI add-on: Sync when a text event is encountered.
        /// param : not used
        /// data : the marker index, which can be used in a BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker) call.
        /// Lyrics can sometimes be found in MidiMarkerType.Text instead of MidiMarkerType.Lyric markers.
        /// </summary>
        MidiText = 0x10003,

        /// <summary>
        /// MIDI add-on: Sync when a Type of event is processed, in either a MIDI file or BassMidi.StreamEvent(int,int,Midi.MidiEventType,int).
        /// param : event Type (0 = all types).
        /// data : LOWORD = event parameter, HIWORD = channel (high 8 bits contain the event Type when syncing on all types).
        /// See BassMidi.StreamEvent(int,int,Midi.MidiEventType,int) for a list of event types and their parameters.
        /// </summary>
        MidiEvent = 0x10004,

        /// <summary>
        /// MIDI add-on: Sync when reaching a tick position.
        /// param : tick position.
        /// data : not used
        /// </summary>
        MidiTick = 0x10005,

        /// <summary>
        /// MIDI add-on: Sync when a time signature event is processed.
        /// param : event Type.
        /// data : The time signature events are given (by BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker))
        /// in the form of "numerator/denominator metronome-pulse 32nd-notes-per-MIDI-quarter-note", eg. "4/4 24 8".
        /// </summary>
        MidiTimeSignature = 0x10006,

        /// <summary>
        /// MIDI add-on: Sync when a key signature event is processed.
        /// param : event Type.
        /// data : The key signature events are given (by Midi.BassMidi.StreamGetMark(int,Midi.MidiMarkerType,int,out Midi.MidiMarker)) in the form of "a b",
        /// where a is the number of sharps (if positive) or flats (if negative),
        /// and b signifies major (if 0) or minor (if 1).
        /// </summary>
        MidiKeySignature = 0x10007,
        #endregion

        #region BassWma
        /// <summary>
        /// WMA add-on: Sync on a track change in a server-side playlist.
        /// Updated tags are available via <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used
        /// </summary>
        WmaChange = 0x10100,

        /// <summary>
        /// WMA add-on: Sync on a mid-stream tag change in a server-side playlist.
        /// Updated tags are available via <see cref="Bass.ChannelGetTags"/>.
        /// param : not used
        /// data : not used - the updated metadata is available from <see cref="Bass.ChannelGetTags"/>.
        /// </summary>
        WmaMeta = 0x10101,
        #endregion

        #region BassMix
        /// <summary>
        /// MIX add-on: Sync when an envelope reaches the end.
        /// param : not used
        /// data : envelope Type
        /// </summary>
        MixerEnvelope = 0x10200,

        /// <summary>
        /// MIX add-on: Sync when an envelope node is reached.
        /// param : Optional limit the sync to a certain envelope Type (one of the BASSMIXEnvelope values).
        /// data : Will contain the envelope Type in the LOWORD and the current node number in the HIWORD.
        /// </summary>
        MixerEnvelopeNode = 0x10201,
        #endregion

        /// <summary>
        /// Sync when a new segment begins downloading.
        /// Mixtime only.
        /// param: not used.
        /// data: not used.
        /// </summary>
        HlsSegement = 0x10300
    }
}
