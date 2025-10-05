using System;

namespace ManagedBass.Cd
{
    /// <summary>
    /// The drive's reading &amp; writing capabilities used with <see cref="BassCd.GetInfo(int,out CDInfo)" />.
    /// </summary>
    [Flags]
    public enum CDReadWriteFlags
    {
        /// <summary>
        /// The drive can read CD-R media.
        /// </summary>
        CDR = 0x1,
        
        /// <summary>
        /// The drive can read CD-RW media.
        /// </summary>
        CDRW = 0x2,
        
        /// <summary>
        /// The drive can read CD-R/RW media where the addressing Type is "method 2".
        /// </summary>
        CDRW2 = 0x4,
        
        /// <summary>
        /// The drive can read DVD-ROM media.
        /// </summary>
        DVD = 0x8,
        
        /// <summary>
        /// The drive can read DVD-R media.
        /// </summary>
        DVDR = 0x10,
        
        /// <summary>
        /// The drive can read DVD-RAM media.
        /// </summary>
        DVDRAM = 0x20,
        
        /// <summary>
        /// The drive is capable of analog playback.
        /// </summary>
        Analog = 0x10000,
        
        /// <summary>
        /// The drive can read in "mode 2 form 1" format.
        /// </summary>
        M2F1 = 0x100000,
        
        /// <summary>
        /// The drive can read in "mode 2 form 2" format.
        /// </summary>
        M2F2 = 0x200000,
        
        /// <summary>
        /// The drive can read multi-session discs.
        /// </summary>
        MultiSession = 0x400000,
        
        /// <summary>
        /// The drive can read CD audio.
        /// </summary>
        CDDA = 0x1000000,
        
        /// <summary>
        /// The drive supports "stream is accurate".
        /// </summary>
        CDDASIA = 0x2000000,
        
        /// <summary>
        /// The drive can read sub-channel data.
        /// </summary>
        SubChannel = 0x4000000,
        
        /// <summary>
        /// The drive can read sub-channel data, and de-interleave it.
        /// </summary>
        SubChannelDeInterleave = 0x8000000,
        
        /// <summary>
        /// The drive can provide C2 error info.
        /// </summary>
        C2 = 0x10000000,
        
        /// <summary>
        /// The drive can read ISRC numbers.
        /// </summary>
        ISRC = 0x20000000,
        
        /// <summary>
        /// The drive can read UPC numbers.
        /// </summary>
        UPC = 0x40000000
    }
}