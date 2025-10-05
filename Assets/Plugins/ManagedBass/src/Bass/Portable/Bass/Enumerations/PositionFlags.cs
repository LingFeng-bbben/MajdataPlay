using System;

namespace ManagedBass
{
    /// <summary>
    /// Channel Position Mode flags to be used with e.g. <see cref="Bass.ChannelGetLength" />, <see cref="Bass.ChannelGetPosition" />,
    /// <see cref="Bass.ChannelSetPosition" /> or <see cref="Bass.StreamGetFilePosition" />.
    /// </summary>
    [Flags]
    public enum PositionFlags
    {
        /// <summary>
        /// Byte position.
        /// </summary>
        Bytes,

        /// <summary>
        /// Order.Row position (HMUSIC only).
        /// LoWord = order, HiWord = row * scaler (<see cref="ChannelAttribute.MusicPositionScaler" />).
        /// </summary>
        MusicOrders,

        /// <summary>
        /// Tick position (MIDI streams only).
        /// </summary>
        MIDITick,

        /// <summary>
        /// OGG bitstream number.
        /// </summary>
        OGG,

        /// <summary>
        /// CD Add-On: the track number.
        /// </summary>
        CDTrack,

        /// <summary>
        /// ZXTune Sub Count.
        /// </summary>
        ZXTuneSubCount = 0x00F10000,

        /// <summary>
        /// ZXTune Sub Length.
        /// </summary>
        ZXTuneSubLength = 0x00F20000,

        /// <summary>
        /// Midi Add-On: Let the old sound decay naturally (including reverb) when changing the position,
        /// including looping and such can also be used in <see cref="Bass.ChannelSetPosition"/> calls to have it apply to particular position changes.
        /// </summary>
        MIDIDecaySeek = 0x4000,

        /// <summary>
        /// Mixer playback buffering when seeking.
        /// </summary>
        MixerReset = 0x10000,

        /// <summary>
        /// MOD Music Flag: Stop all notes when moving position.
        /// </summary>
        MusicPositionReset = 0x8000,

        /// <summary>
        /// MOD Music Flag: Stop all notes and reset bmp/etc when moving position.
        /// </summary>
        MusicPositionResetEx = 0x400000,

        /// <summary>
        /// Mixer Flag: Don't ramp-in the start after seeking.
        /// </summary>
        MixerNoRampIn = 0x800000,

        /// <summary>
        /// Flag: Allow inexact seeking.
        /// For speed, seeking may stop at the beginning of a block rather than partially processing the block to reach the requested position.
        /// </summary>
        Inexact = 0x8000000,

        /// <summary>
        /// Flag: The requested position is relative to the current position. pos is treated as signed in this case and can be negative.
        /// Unless the <see cref="PositionFlags.MixerReset"/> flag is also used, this is relative to the current decoding/processing position, which will be ahead of the currently heard position if the mixer output is buffered. 
        /// </summary>
        Relative = 0x4000000,

        /// <summary>
        /// Get the decoding (not playing) position.
        /// </summary>
        Decode = 0x10000000,

        /// <summary>
        /// Flag: decode to the position instead of seeking.
        /// </summary>
        DecodeTo = 0x20000000,

        /// <summary>
        /// Scan the file to build a seek table up to the position, if it has not already been scanned.
        /// Scanning will continue from where it left off previously rather than restarting from the beginning of the file each time.
        /// This flag only applies to MP3/MP2/MP1 files and will be ignored with other file formats.
        /// </summary>
        Scan = 0x40000000,

        /// <summary>
        /// HLS Segment sequence number.
        /// </summary>
        HlsSegment = 0x10000
    }
}
