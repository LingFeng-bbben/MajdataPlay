using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined callback function to receive capture device profiles (see <see cref="BassDShow.CaptureDeviceProfiles" />).
	/// </summary>
	/// <param name="Profile">The profile name of the capture device.</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.CaptureDeviceProfiles" /> was called.</param>
	/// <returns><see langword="true" /> to continue, else <see langword="false" />.</returns>
	public delegate bool EnumProfilesProcedure(string Profile, IntPtr User);
}