using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Used with <see cref="Bass.PluginGetInfo" /> to retrieve information on a plugin.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PluginInfo
    {
        int version;
        int formatc;
        IntPtr formats;

        /// <summary>
        /// Plugin version.
        /// </summary>
        public Version Version => Extensions.GetVersion(version);

        /// <summary>
        /// The collection of supported formats.
        /// </summary>
        /// <remarks>
        /// Note: There is no guarantee that the list of supported formats is complete or might contain formats not being supported on your particular OS/machine (due to additional or missing audio codecs).
        /// </remarks>
        public PluginFormat[] Formats
        {
            get
            {
                var arr = new PluginFormat[formatc];
                var sizeOfPluginFormat = BassMarshal.SizeOf<PluginFormat>();

                for (var i = 0; i < formatc; ++i, formats += sizeOfPluginFormat)
                    arr[i] = BassMarshal.PtrToStructure<PluginFormat>(formats);
                
                return arr;
            }
        }
    }
}
