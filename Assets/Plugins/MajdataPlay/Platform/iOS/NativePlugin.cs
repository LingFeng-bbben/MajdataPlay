using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Platform.iOS;
public static class NativePlugin
{
    public delegate void OnFileOpenCallback(string tempFilePath);

    [DllImport("__Internal")]
    public static extern void RegisterOnFileOpenCallback(OnFileOpenCallback callback);
    [DllImport("__Internal")]
    public static extern void UnregisterOnFileOpenCallback();
}
