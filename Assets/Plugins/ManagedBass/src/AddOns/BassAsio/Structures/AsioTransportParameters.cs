using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
    /// <summary>
    /// Used with <see cref="BassAsio.Future" /> and the Transport selector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class AsioTransportParameters
    {
        /// <summary>
        /// One of the <see cref="AsioTransportCommand"/> values (other values might be available).
        /// </summary>
        public AsioTransportCommand command;

        /// <summary>
        /// Number of samples data.
        /// </summary>
        public long SamplePosition;

        /// <summary>
        /// Track Index
        /// </summary>
        public int Track;

        /// <summary>
        /// 512 Tracks on/off
        /// </summary>
        public int[] TrackSwitches = new int[16];

        /// <summary>
        /// Max 64 characters.
        /// </summary>
        public string Future = string.Empty;
    }
}