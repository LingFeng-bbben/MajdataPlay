using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Vst
{
	/// <summary>
	/// This class is only needed if you subclass the audioMaster callback using <see cref="BassVstAction.AudioMaster"/> in the <see cref="VstProcedure" /> (see the VST DSK for more information).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassVstAudioMasterParam
	{
		/// <summary>
		/// Parameter forwarded from the audioMaster callback.
		/// </summary>
		public IntPtr AEffect;

		/// <summary>
		/// Parameter forwarded from the audioMaster callback (one of the <see cref="BassVstDispatcherOpCodes" />).
		/// </summary>
		public BassVstDispatcherOpCodes OpCode;

		/// <summary>
		/// Parameter forwarded from the audioMaster callback.
		/// </summary>
		public int Index;

		/// <summary>
		/// Parameter forwarded from the audioMaster callback.
		/// </summary>
		public int Value;

		/// <summary>
		/// Parameter forwarded from the audioMaster callback.
		/// </summary>
		public IntPtr Pointer;

		/// <summary>
		/// Parameter forwarded from the audioMaster callback.
		/// </summary>
		public float Option;

		/// <summary>
		/// Set this to 0 if you want to skip the normal BASS_VST audioMaster processing.
		/// In this case the return value is forwarded to the effect.
		/// </summary>
		public long DoDefault;
	}
}