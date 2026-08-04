using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Platform.iOS.PInvoke
{
    internal static class AppStatusPInvoke
    {
#if UNITY_EDITOR_OSX
        const string LibraryName = "NativeKeyboard";
#else
        const string LibraryName = "__Internal";
#endif

        public delegate void AppStatusChangeDelegate(bool status);

        [DllImport(LibraryName, EntryPoint = "_RegisterAppStatusCallbacks")]
        public static extern void RegisterAppStatusCallbacks(AppStatusChangeDelegate foregroundCallback, AppStatusChangeDelegate focusCallback);

        [DllImport(LibraryName, EntryPoint = "_IsAppInForeground")]
        public static extern bool IsAppInForeground();

        [DllImport(LibraryName, EntryPoint = "_IsAppFocused")]
        public static extern bool IsAppFocused();
    }
}
