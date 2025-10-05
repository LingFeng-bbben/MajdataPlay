using System;

namespace ManagedBass.Flac
{
    /// <summary>
    /// Cue Sheet Track type to be used with <see cref="FlacCueTrack.TrackFlags"/>
    /// </summary>
    [Flags]
    public enum CueSheetTrackType
    {
        /// <summary>
        /// Audio
        /// </summary>
        Audio,
        
        /// <summary>
        /// Data
        /// </summary>
        Data,

        /// <summary>
        /// Pre
        /// </summary>
        Pre
    }
}