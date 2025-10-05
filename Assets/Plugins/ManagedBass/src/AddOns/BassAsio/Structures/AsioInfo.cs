using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
    /// <summary>
    /// Used with <see cref="BassAsio.Info" /> to retrieve information on the current device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AsioInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        char[] name;

        int version;
        int inputs;
        int outputs;
        int bufmin;
        int bufmax;
        int bufpref;
        int bufgran;
        AsioInitFlags initflags;

        /// <summary>
        /// Flags used when initialising the AsioDevice.
        /// </summary>
        public AsioInitFlags InitFlags => initflags;

        /// <summary>
        /// The name of the device/driver.
        /// </summary>
        public string Name => new string(name).Replace("\0", "").Trim();

        /// <summary>
        /// The driver version.
        /// </summary>
        public int DriverVersion => version;

        /// <summary>
        /// The number of input channels available.
        /// </summary>
        public int Inputs => inputs;

        /// <summary>
        /// The number of output channels available.
        /// </summary>
        public int Outputs => outputs;

        /// <summary>
        /// The minimum Buffer Length, in samples.
        /// </summary>
        public int MinBufferLength => bufmin;

        /// <summary>
        /// The maximum Buffer Length, in samples.
        /// </summary>
        public int MaxBufferLength => bufmax;

        /// <summary>
        /// The preferred/default Buffer Length, in samples.
        /// </summary>
        public int PreferredBufferLength => bufpref;

        /// <summary>
        /// The Buffer Length granularity, that is the smallest possible Length change... -1 = the possible Buffer lengths increase in powers of 2.
        /// </summary>
        public int BufferLengthGranularity => bufgran;

        /// <summary>
        /// Returns the <see cref="Name"/> of the AsioDevice.
        /// </summary>
        public override string ToString() => Name;
    }
}
