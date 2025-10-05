using System.Runtime.InteropServices;

namespace ManagedBass.Adx
{
    /// <summary>
    /// ADX Loop tag structure
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ADXLoopTag
	{
		/// <summary>
		/// Is looping is enabled.
		/// </summary>
		public bool LoopEnabled;

		/// <summary>
		/// The sample start position.
		/// </summary>
		public long SampleStart;

		/// <summary>
		/// The byte start position.
		/// </summary>
		public long ByteStart;

		/// <summary>
		/// The sample end position.
		/// </summary>
		public long SampleEnd;

		/// <summary>
		/// The byte end position.
		/// </summary>
		public long ByteEnd;

        /// <summary>
        /// Read the tag from a channel.
        /// </summary>
        public static ADXLoopTag Read(int Channel)
        {
            return BassMarshal.PtrToStructure<ADXLoopTag>(Bass.ChannelGetTags(Channel, TagType.AdxLoop));
        }
	}
}