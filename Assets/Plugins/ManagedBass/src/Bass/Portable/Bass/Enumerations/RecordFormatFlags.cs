using System;

namespace ManagedBass
{
    /// <summary>
    /// Formats flags of <see cref="RecordInfo.SupportedFormats"/> member to be used with <see cref="RecordInfo" />
    /// </summary>
    [Flags]
    public enum RecordFormatFlags
    {
        /// <summary>
        /// Unknown Format
        /// </summary>
        Unknown,

        /// <summary>
        /// 11.025 kHz, Mono, 8-bit
        /// </summary>
        WF1M08 = 0x1,

        /// <summary>
        /// 11.025 kHz, Stereo, 8-bit
        /// </summary>
        WF1S08 = 0x2,

        /// <summary>
        /// 11.025 kHz, Mono, 16-bit
        /// </summary>
        WF1M16 = 0x4,

        /// <summary>
        /// 11.025 kHz, Stereo, 16-bit
        /// </summary>
        WF1S16 = 0x8,

        /// <summary>
        /// 22.05 kHz, Mono, 8-bit
        /// </summary>
        WF2M08 = 0x10,

        /// <summary>
        /// 22.05 kHz, Stereo, 8-bit
        /// </summary>
        WF2S08 = 0x20,

        /// <summary>
        /// 22.05 kHz, Mono, 16-bit
        /// </summary>
        WF2M16 = 0x40,

        /// <summary>
        /// 22.05 kHz, Stereo, 16-bit
        /// </summary>
        WF2S16 = 0x80,

        /// <summary>
        /// 44.1 kHz, Mono, 8-bit
        /// </summary>
        WF4M08 = 0x100,

        /// <summary>
        /// 44.1 kHz, Stereo, 8-bit
        /// </summary>
        WF4S08 = 0x200,

        /// <summary>
        /// 44.1 kHz, Mono, 16-bit
        /// </summary>
        WF4M16 = 0x400,

        /// <summary>
        /// 44.1 kHz, Stereo, 16-bit
        /// </summary>
        WF4S16 = 0x800,

        /// <summary>
        /// 48 kHz, Mono, 8-bit
        /// </summary>
        WF48M08 = 0x1000,

        /// <summary>
        /// 48 kHz, Stereo, 8-bit
        /// </summary>
        WF48S08 = 0x2000,

        /// <summary>
        /// 48 kHz, Mono, 16-bit
        /// </summary>
        WF48M16 = 0x4000,

        /// <summary>
        /// 48 kHz, Stereo, 16-bit
        /// </summary>
        WF48S16 = 0x8000,

        /// <summary>
        /// 96 kHz, Mono, 8-bit
        /// </summary>
        WF96M08 = 0x10000,

        /// <summary>
        /// 96 kHz, Stereo, 8-bit
        /// </summary>
        WF96S08 = 0x20000,

        /// <summary>
        /// 96 kHz, Mono, 16-bit
        /// </summary>
        WF96M16 = 0x40000,

        /// <summary>
        /// 96 kHz, Stereo, 16-bit
        /// </summary>
        WF96S16 = 0x80000
    }
}
