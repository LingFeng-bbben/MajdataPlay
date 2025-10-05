using System;
using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.PluginGetInfo(int,out BassDShowPluginInfo)" /> to retrieve information on a plugin.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowPluginInfo
	{
		/// <summary>
		/// Plugin version.
		/// </summary>
		public int Version;

		/// <summary>
		/// Type of decoder: 1=audio, 2=video.
		/// </summary>
		int decoderType;

		IntPtr description;

		/// <summary>
		/// The plugin description.
		/// </summary>
		public string Description => description == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(description);

		/// <summary>
		/// Returns if the plugin is an audio plugin.
		/// </summary>
		public bool IsAudio => decoderType == 1;

		/// <summary>
		/// Returns if the plugin is a video plugin.
		/// </summary>
		public bool IsVideo => decoderType == 2;
	}
}