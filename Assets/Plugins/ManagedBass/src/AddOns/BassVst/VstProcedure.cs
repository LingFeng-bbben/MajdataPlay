using System;

namespace ManagedBass.Vst
{
	/// <summary>
	/// User defined VST callback method to be used with <see cref="BassVst.SetCallback" />.
	/// </summary>
	/// <param name="VstHandle">The VST plugin handle as returned from <see cref="BassVst.ChannelSetDSP" />.</param>
	/// <param name="Action">The action parameter, one of the <see cref="BassVstAction" /> values (see below).</param>
	/// <param name="Param1">The first parameter (see the VST SDK for further details).</param>
	/// <param name="Param2">The second parameter (see the VST SDK for further details).</param>
	/// <param name="User">The user parameter as specified in the <see cref="BassVst.SetCallback" /> call.</param>
	/// <returns>Unless defined otherwise, the callback function should always return 0.</returns>
	public delegate int VstProcedure(int VstHandle, BassVstAction Action, int Param1, int Param2, IntPtr User);
}