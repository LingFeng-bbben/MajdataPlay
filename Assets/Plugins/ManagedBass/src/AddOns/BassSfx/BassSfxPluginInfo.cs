using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Sfx
{
	/// <summary>
	/// Used with <see cref="BassSfx.WMPGetPlugin(int,out BassSfxPluginInfo)" /> to retrieve information on a registered Windows Media Player plugin.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassSfxPluginInfo
	{
		IntPtr name;
		IntPtr clsid;

		/// <summary>
		/// The description of the plugin.
		/// </summary>
		public string Name => Marshal.PtrToStringUni(name);

		/// <summary>
		/// The classid of the windows media player plugin that can be used with <see cref="BassSfx.PluginCreate" /> to create the plugin for use in BASS_SFX.
		/// </summary>
		public string ClsId => Marshal.PtrToStringUni(clsid);

		/// <summary>
		/// The description of the plugin.
		/// </summary>
		/// <returns>The description of the plugin.</returns>
		public override string ToString() => Name;
	}
}