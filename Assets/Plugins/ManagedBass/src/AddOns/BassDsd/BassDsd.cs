using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Dsd
{
    /// <summary>
    /// BassDsd is a BASS addon enabling the playing of DSD (Direct Stream Digital) data in DSDIFF and DSF containers.
    /// </summary> 
    /// <remarks>
    /// Supports .dsf, .dff, .dsd
    /// </remarks>
    public static class BassDsd
    {
        const string DllName = "bassdsd";
                
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_DSD_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags, int Frequency = 0);

        [DllImport(DllName)]
        static extern int BASS_DSD_StreamCreateFile(bool mem, IntPtr file, long offset, long length, BassFlags flags, int Frequency = 0);

        /// <summary>Create a stream from file.</summary>
        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_DSD_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode, Frequency);
        }

        /// <summary>Create a stream from Memory (IntPtr).</summary>
        public static int CreateStream(IntPtr Memory, long Offset, long Length, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_DSD_StreamCreateFile(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags, Frequency);
        }

        /// <summary>Create a stream from Memory (byte[]).</summary>
        public static int CreateStream(byte[] Memory, long Offset, long Length, BassFlags Flags, int Frequency = 0)
        {
            return GCPin.CreateStreamHelper(Pointer => CreateStream(Pointer, Offset, Length, Flags), Memory);
        }

        [DllImport(DllName)]
        static extern int BASS_DSD_StreamCreateFileUser(StreamSystem system, BassFlags flags, [In, Out] FileProcedures procs, IntPtr user, int Frequency = 0);

        /// <summary>Create a stream using User File Procedures.</summary>
        public static int CreateStream(StreamSystem System, BassFlags Flags, FileProcedures Procedures, IntPtr User = default(IntPtr), int Frequency = 0)
        {
            var h = BASS_DSD_StreamCreateFileUser(System, Flags, Procedures, User, Frequency);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedures);

            return h;
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_DSD_StreamCreateURL(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User, int Frequency = 0);

        /// <summary>Create a stream from Url.</summary>
        public static int CreateStream(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User = default(IntPtr), int Frequency = 0)
        {
            var h = BASS_DSD_StreamCreateURL(Url, Offset, Flags | BassFlags.Unicode, Procedure, User, Frequency);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        /// <summary>
        /// The default sample rate when converting to PCM.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This setting determines what sample rate is used by default when converting to PCM.
        /// The rate actually used may be different if the specified rate is not valid for a particular DSD rate, in which case it will be rounded up (or down if there are none higher) to the nearest valid rate;
        /// the valid rates are 1/8, 1/16, 1/32, etc. of the DSD rate down to a minimum of 44100 Hz.
        /// </para>
        /// <para>
        /// The default setting is 88200 Hz.
        /// Changes only affect subsequently created streams, not any that already exist.
        /// </para>
        /// </remarks>
        public static int DefaultFrequency
        {
            get => Bass.GetConfig(Configuration.DSDFrequency);
            set => Bass.Configure(Configuration.DSDFrequency, value);
        }

        /// <summary>
        /// The default gain applied when converting to PCM.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This setting determines what gain is applied by default when converting to PCM.
        /// Changes only affect subsequently created streams, not any that already exist.
        /// An existing stream's gain can be changed via the <see cref="ChannelAttribute.DSDGain" /> attribute.
        /// </para>
        /// <para>The default setting is 6dB.</para>
        /// </remarks>
        public static int DefaultGain
        {
            get => Bass.GetConfig(Configuration.DSDGain);
            set => Bass.Configure(Configuration.DSDGain, value);
        }
    }
}