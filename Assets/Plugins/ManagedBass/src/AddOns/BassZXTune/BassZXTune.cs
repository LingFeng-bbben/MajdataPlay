using System;
using System.Runtime.InteropServices;

namespace ManagedBass.ZXTune
{
    /// <summary>
    /// BassZXTune is a BASS addon adding support for chiptune tracker formats.
    /// </summary>
    public static class BassZXTune
    {
        const string DllName = "basszxtune";
		        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_ZXTUNE_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags);

        [DllImport(DllName)]
        static extern int BASS_ZXTUNE_StreamCreateFile(bool mem, IntPtr file, long offset, long length, BassFlags flags);

        /// <summary>Create a stream from file.</summary>
        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default)
        {
            return BASS_ZXTUNE_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode);
        }

        /// <summary>Create a stream from Memory (IntPtr).</summary>
        public static int CreateStream(IntPtr Memory, long Offset, long Length, BassFlags Flags = BassFlags.Default)
        {
            return BASS_ZXTUNE_StreamCreateFile(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags);
        }

        /// <summary>Create a stream from Memory (byte[]).</summary>
        public static int CreateStream(byte[] Memory, long Offset, long Length, BassFlags Flags)
        {
            return GCPin.CreateStreamHelper(Pointer => CreateStream(Pointer, Offset, Length, Flags), Memory);
        }

        [DllImport(DllName)]
        static extern int BASS_ZXTUNE_StreamCreateFileUser(StreamSystem system, BassFlags flags, [In, Out] FileProcedures procs, IntPtr user);

        /// <summary>Create a stream using User File Procedures.</summary>
        public static int CreateStream(StreamSystem System, BassFlags Flags, FileProcedures Procedures, IntPtr User = default(IntPtr))
        {
            var h = BASS_ZXTUNE_StreamCreateFileUser(System, Flags, Procedures, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedures);

            return h;
        }

        /// <summary>
        /// Gets or Sets the Maximum allowed File Size (can be somewhat useful on mobile devices with limited processing power).
        /// </summary>
        public static int MaxFileSize
        {
            get => Bass.GetConfig(Configuration.ZXTuneMaxFileSize);
            set => Bass.Configure(Configuration.ZXTuneMaxFileSize, value);
        }
    }
}
