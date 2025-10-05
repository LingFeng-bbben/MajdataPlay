using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Flac
{
    /// <summary>
    /// Flac Cuesheet tag track structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class FlacCueTrack
    {
        /// <summary>
        /// Track offset in samples.
        /// </summary>
        public long Offset;

        /// <summary>
        /// The track number.
        /// </summary>
        public int Number;

        IntPtr isrc;

        /// <summary>
        /// The International Standard Recording Code.
        /// </summary>
        public string ISRC => isrc == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(isrc);

        /// <summary>
        /// The Track type.
        /// </summary>
        public CueSheetTrackType TrackFlags;

        /// <summary>
        /// The number of indices.
        /// </summary>
        public int IndexesCount;

        IntPtr indexes;

        /// <summary>
        /// The array of indices or <see langword="null" />.
        /// </summary>
        public FlacCueTrackIndex[] Indexes
		{
			get
			{
				if (IndexesCount == 0)
                    return null;
				
				var arr = new FlacCueTrackIndex[IndexesCount];
				var ptr = indexes;

				for (var i = 0; i < IndexesCount; i++)
				{
					arr[i] = BassMarshal.PtrToStructure<FlacCueTrackIndex>(ptr);
					ptr += BassMarshal.SizeOf(arr[i]);
				}

				return arr;
			}
		}
    }
}