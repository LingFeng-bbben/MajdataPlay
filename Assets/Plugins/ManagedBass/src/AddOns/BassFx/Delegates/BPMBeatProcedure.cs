using System;

namespace ManagedBass.Fx
{
    /// <summary>
	/// User defined callback function, to get the Beat position in seconds.
	/// </summary>
	/// <param name="Channel">Handle that the <see cref="BassFx.BPMBeatCallbackSet" /> or <see cref="BassFx.BPMBeatDecodeGet" /> has applied to.</param>
	/// <param name="BeatPosition">The exact beat position in seconds.</param>
	/// <param name="User">The user instance data given when <see cref="BassFx.BPMBeatCallbackSet" /> or <see cref="BassFx.BPMBeatDecodeGet" /> was called.</param>
	public delegate void BPMBeatProcedure(int Channel, double BeatPosition, IntPtr User);
}