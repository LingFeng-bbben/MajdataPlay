using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined callback function to enumerate connected streams (see <see cref="BassDShow.ChannelEnumerateStreams" />).
	/// </summary>
	/// <param name="Format">The stream format (0: unknown, 1: video , 2: audio , 3: subtitle).</param>
	/// <param name="Name">The format name.</param>
	/// <param name="Index">The stream index (use this index to enable or disable it).</param>
	/// <param name="Enabled">Is this stream enabled?</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.ChannelEnumerateStreams" /> was called.</param>
	/// <returns><see langword="true" /> to continue, else <see langword="false" />.</returns>
	public delegate bool VideoStreamsProcedure(int Format, string Name, int Index, bool Enabled, IntPtr User);
}