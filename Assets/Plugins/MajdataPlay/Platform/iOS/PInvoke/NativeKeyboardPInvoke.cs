using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Platform.iOS.PInvoke
{
    internal static class NativeKeyboardPInvoke
    {
#if UNITY_EDITOR_OSX
        const string LibraryName = "NativeKeyboard";
#else
        const string LibraryName = "__Internal";
#endif

        [DllImport(LibraryName, EntryPoint = "Init")]
        public static extern ErrorCode InitInternal();

        [DllImport(LibraryName, EntryPoint = "GetKeyboardHandle")]
        public static extern nint GetKeyboardHandleInternal();

        [DllImport(LibraryName, EntryPoint = "IsPressed")]
        public static extern ErrorCode IsPressedInternal(uint keyCode, out byte isPressedOut);

        [DllImport(LibraryName, EntryPoint = "IsPressedWithHandle")]
        public static extern ErrorCode IsPressedWithHandleInternal(nint keyboardHandle, uint keyCode, out byte isPressedOut);

        [DllImport(LibraryName, EntryPoint = "Free")]
        public static extern ErrorCode FreeInternal();
    }
}
