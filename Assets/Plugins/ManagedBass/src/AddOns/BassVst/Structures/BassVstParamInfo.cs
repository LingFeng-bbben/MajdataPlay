using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Vst
{
	/// <summary>
	/// Common information structure about an editable parameter to a VST plugin parameter to be used with <see cref="BassVst.GetParamInfo(int,int,out BassVstParamInfo)" />.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassVstParamInfo
	{
	    IntPtr name;
	    IntPtr unit;
	    IntPtr display;
        float defaultValue;

        /// <summary>
        /// Name of the parameter (empty strings returned if a plugin does not provide these information).
        /// <para>Examples: Time, Gain, RoomType</para>
        /// </summary>
        public string Name => Marshal.PtrToStringUni(name);

		/// <summary>
		/// Unit of the parameter.
		/// <para>Examples: sec, dB, type</para>
		/// </summary>
		public string Unit => Marshal.PtrToStringUni(unit);

	    /// <summary>
	    /// The current value in a readable format (empty strings returned if a plugin does not provide these information).
	    /// <para>Examples: 0.5, -3, PLATE</para>
	    /// </summary>
	    public string Display => Marshal.PtrToStringUni(display);

	    /// <summary>
	    /// The default value (in the range of 0.0 and 1.0).
	    /// </summary>
	    public float DefaultValue => defaultValue;
	}
}