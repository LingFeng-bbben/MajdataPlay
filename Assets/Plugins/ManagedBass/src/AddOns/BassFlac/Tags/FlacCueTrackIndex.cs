using System.Runtime.InteropServices;

namespace ManagedBass.Flac
{
    /// <summary>
    /// Flac Cuesheet tag Track Index structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class FlacCueTrackIndex
    {        
        /// <summary>
        /// Index offset in samples relative to the track offset.
        /// </summary>
        public long Offser;

        /// <summary>
        /// The index number.
        /// </summary>
        public int Number;
    }
}