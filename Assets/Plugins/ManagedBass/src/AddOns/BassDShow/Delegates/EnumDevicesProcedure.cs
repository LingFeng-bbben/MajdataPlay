using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined callback function to receive capture devices (see <see cref="BassDShow.CaptureGetDevices" />) or audio render devices (see <see cref="BassDShow.GetAudioRenderers" />).
	/// </summary>
	/// <param name="Name">The name of the capture (or audio render) device.</param>
	/// <param name="Guid">The GUID of the capture (or audio render) device.</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.CaptureGetDevices" /> or <see cref="BassDShow.GetAudioRenderers" /> was called.</param>
	/// <returns><see langword="true" /> to continue, else <see langword="false" />.</returns>
	public delegate bool EnumDevicesProcedure(string Name, Guid Guid, IntPtr User);
}