using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MajdataPlay.Platform.Win32.PInvoke
{
    public static partial class Win32API
    {
        public static class Registry
        {
            public static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
            public const int KEY_READ = 0x20019;
            public const int ERROR_SUCCESS = 0;
            public const int ERROR_NO_MORE_ITEMS = 259;

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
            public static extern int RegOpenKeyEx(
                UIntPtr hKey,
                string lpSubKey,
                uint ulOptions,
                int samDesired,
                out IntPtr phkResult);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
            public static extern int RegEnumValue(
                IntPtr hKey,
                uint dwIndex,
                StringBuilder lpValueName,
                ref uint lpcchValueName,
                IntPtr lpReserved,
                out uint lpType,
                StringBuilder lpData,
                ref uint lpcbData);

            [DllImport("advapi32.dll")]
            public static extern int RegCloseKey(IntPtr hKey);
        }
    }
}
