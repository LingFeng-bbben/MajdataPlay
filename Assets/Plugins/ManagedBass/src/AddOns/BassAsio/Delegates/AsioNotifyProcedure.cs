using System;

namespace ManagedBass.Asio
{
    /// <summary>
    /// User defined notification callback function (to be used with <see cref="BassAsio.SetNotify" />).
    /// </summary>
    /// <param name="Notify">The notification.</param>
    /// <param name="User">The User instance data given when <see cref="BassAsio.SetNotify" /> was called.</param>
    /// <remarks>
    /// When using multiple devices, <see cref="BassAsio.CurrentDevice" /> can be used to determine which the notification applies to.
    /// </remarks>
    public delegate void AsioNotifyProcedure(AsioNotify Notify, IntPtr User);
}