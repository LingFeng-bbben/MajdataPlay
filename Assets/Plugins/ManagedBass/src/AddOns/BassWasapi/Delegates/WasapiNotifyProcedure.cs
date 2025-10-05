using System;

namespace ManagedBass.Wasapi
{
    /// <summary>
	/// User defined notification callback function.
	/// </summary>
	/// <param name="Notify">The notification</param>
	/// <param name="Device">The device that the notification applies to.</param>
	/// <param name="User">The user instance data given when <see cref="BassWasapi.SetNotify(WasapiNotifyProcedure,IntPtr)" /> was called.</param>
	public delegate void WasapiNotifyProcedure(WasapiNotificationType Notify, int Device, IntPtr User);
}