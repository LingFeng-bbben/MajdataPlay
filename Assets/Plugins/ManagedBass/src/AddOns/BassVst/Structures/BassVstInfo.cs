using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Vst
{
	/// <summary>
	/// <see cref="BassVst.GetInfo" /> writes some information about a vstHandle returned by <see cref="BassVst.ChannelSetDSP" /> to this BassVstInfo structure.
	/// </summary>
	/// <remarks>
	/// <para>
	/// VST effects that have no input channels (so called "Instruments") are not loaded by BASS_VST.
	/// So you can assume <see cref="ChansIn"/> and <see cref="ChansOut"/> to be at least 1.
	/// </para>
	/// <para>
	/// Multi-channel streams should work correctly, if supported by a effect.
	/// If not, only the first chansIn channels are processed by the effect, the other ones stay unaffected.  
	/// The opposite, eg. assigning multi-channel effects to stereo channels, should be no problem at all.
	/// </para>
	/// <para>
	/// If mono effects are assigned to stereo channels, the result will be mono, expanded to both channels.
	/// This behaviour can be switched of using the <see cref="BassVstDsp.KeepChannels"/> in <see cref="BassVst.ChannelSetDSP" />.
	/// </para>
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassVstInfo
	{
		/// <summary>
		/// The Channel Handle as given to <see cref="BassVst.ChannelSetDSP" />.
		/// </summary>
		public int ChannelHandle;

		/// <summary>
		/// A unique ID for the effect (the IDs are registered at Steinberg).
		/// </summary>
		public int UniqueID;

	    IntPtr effectName;

	    /// <summary>
	    /// The effect name (empty string is returned if a plugin does not provide these information).
	    /// </summary>
	    public string EffectName => Marshal.PtrToStringUni(effectName);

		/// <summary>
		/// The effect version (example 0x01010000 for version 1.1.0.0).
		/// </summary>
		public int EffectVersion;

		/// <summary>
		/// The VST version, the effect was written for (example 0x02030000 for version 2.3.0.0).
		/// </summary>
		public int EffectVstVersion;

		/// <summary>
		/// The VST version supported by BASS_VST (e.g. 2.4).
		/// </summary>
		public int HostVstVersion;

	    IntPtr productName;

	    /// <summary>
	    /// The product name (may be empty).
	    /// </summary>
	    public string ProductName => Marshal.PtrToStringUni(productName);

	    IntPtr vendorName;

	    /// <summary>
	    /// The vendor name (may be empty).
	    /// </summary>
	    public string VendorName => Marshal.PtrToStringUni(vendorName);

		/// <summary>
		/// The vendor-specific version number (example 0x01010000 for version 1.1.0.0).
		/// </summary>
		public int VendorVersion;

		/// <summary>
		/// Maximum number of possible input channels (should be at least 1 here).
		/// </summary>
		public int ChansIn;

		/// <summary>
		/// Maximum number of possible output channels (should be at least 1 here).
		/// </summary>
		public int ChansOut;

		/// <summary>
		/// For algorithms which need input in the first place, in number of samples.
		/// </summary>
		public int InitialDelay;

		/// <summary>
		/// Has this plugin an embedded editor?
		/// <para>If <see langword="true" />, the <see cref="BassVst.EmbedEditor" /> method can be called.</para>
		/// </summary>
		public bool HasEditor;

		/// <summary>
		/// Initial/current width of the embedded editor, see also <see cref="BassVstAction.EditorResized"/>.
		/// <para>For a very few plugins, editorWidth and editorHeight may be 0 if the editor is not yet opened.</para>
		/// </summary>
		public int EditorWidth;

        /// <summary>
        /// Initial/current height of the embedded editor, see also <see cref="BassVstAction.EditorResized"/>.
        /// <para>For a very few plugins, editorWidth and editorHeight may be 0 if the editor is not yet opened.</para>
        /// </summary>
        public int EditorHeight;

		/// <summary>
		/// The underlying AEffect object (see the VST SDK).
		/// </summary>
		public IntPtr AEffect;

		/// <summary>
		/// <see langword="true" />=the VST plugin is an instrument, <see langword="false" />=the VST plugin is an effect.
		/// </summary>
		public bool IsInstrument;

		/// <summary>
		/// The internal DSP handle.
		/// </summary>
		public int DspHandle;
	}
}