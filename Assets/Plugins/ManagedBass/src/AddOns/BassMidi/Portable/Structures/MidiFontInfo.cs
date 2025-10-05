using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
	/// <summary>
	/// Used with <see cref="BassMidi.FontGetInfo(int, out MidiFontInfo)" /> to retrieve information on a soundfont.
	/// </summary>
	/// <remarks>
	/// The <see cref="Name"/>, <see cref="Copyright"/> and <see cref="Comment"/> information might not be included in some SF2 files. 
	/// Only the <see cref="Presets"/>, <see cref="SampleDataLoaded"/> and <see cref="SampleDataType"/> members are available with SFZ files, with the <see cref="SampleDataType"/> value reflecting the most recently loaded encoded sample (it is possible for different samples to use different encoding).
	/// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiFontInfo
    {
        IntPtr name;
        IntPtr copyright;
        IntPtr comment;

        /// <summary>
        /// The number of presets/instruments in the soundfont.
        /// </summary>
        public int Presets;
        
        /// <summary>
        /// The total size (in bytes) of the sample data in the soundfont.
        /// </summary>
        public int SampleDataSize;

        /// <summary>
        /// The amount of sample data currently loaded... -1 = the soundfont is memory mapped.
        /// </summary>
        public int SampleDataLoaded;

        /// <summary>
        /// The <see cref="ChannelType"/> format of the sample data if it's packed... -1 = Unknown format (appropriate BASS add-on not loaded), 0 = not packed.
        /// </summary>
        public int SampleDataType;

        /// <summary>
        /// Name of the soundfont.
        /// </summary>
        public string Name => name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(name);

        /// <summary>
        /// Copyright notice.
        /// </summary>
        public string Copyright => copyright == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(copyright);

        /// <summary>
        /// Any comments.
        /// </summary>
        public string Comment => comment == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(comment);
    }
}