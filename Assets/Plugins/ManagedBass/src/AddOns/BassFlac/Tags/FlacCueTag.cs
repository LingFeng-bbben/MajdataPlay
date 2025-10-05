using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Flac
{
    /// <summary>
    /// Flac Cuesheet tag structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class FlacCueTag
	{
		IntPtr catalog;

        /// <summary>
        /// The media catalog number.
        /// </summary>
        public string Catalog => catalog == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(catalog);

        /// <summary>
        /// The number of lead-in samples.
        /// </summary>
        public int LeadInSampleCount;

        /// <summary>
        /// The cuesheet corresponds to a CD?
        /// </summary>
        public bool IsCD;

        /// <summary>
        /// The number of tracks.
        /// </summary>
        public int TrackCount;

		IntPtr tracks;

        /// <summary>
        /// The array of tracks or <see langword="null" />.
        /// </summary>
        public FlacCueTrack[] Tracks
		{
			get
			{
				if (TrackCount == 0)
                    return null;

				var arr = new FlacCueTrack[TrackCount];
				var ptr = tracks;

				for (var i = 0; i < TrackCount; i++)
				{
					arr[i] = BassMarshal.PtrToStructure<FlacCueTrack>(ptr);
					ptr += BassMarshal.SizeOf(arr[i]);
				}

				return arr;
			}
		}

        /// <summary>
        /// Reads the tag from a Channel.
        /// </summary>
        public static FlacCueTag Read(int Channel)
        {
            return BassMarshal.PtrToStructure<FlacCueTag>(Bass.ChannelGetTags(Channel, TagType.FlacCue));
        }
    }
}