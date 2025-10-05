using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
    /// <summary>
    /// Used with <see cref="BassAsio.GetDeviceInfo(int,out AsioDeviceInfo)" /> to retrieve information on an asio device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AsioDeviceInfo
    {
        IntPtr name;
        IntPtr driver;

        static string PtrToString(IntPtr ptr) => BassAsio.Unicode ? Marshal.PtrToStringUni(ptr) : (ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr));

        /// <summary>
        /// The description of the device.
        /// </summary>
        public string Name => PtrToString(name);

        /// <summary>
        /// The filename of the driver being used.
        /// <para>Further information can be obtained from the file using the GetFileVersionInfo Win32 API function.</para>
        /// </summary>
        public string Driver => PtrToString(driver);

        /// <summary>
        /// Returns the <see cref="Name"/> of the device.
        /// </summary>
        public override string ToString() => Name;
    }
}