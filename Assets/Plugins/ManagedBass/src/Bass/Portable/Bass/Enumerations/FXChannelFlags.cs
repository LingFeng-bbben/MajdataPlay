using System;

namespace ManagedBass
{
    /// <summary>
	/// DSP channels flags.
	/// </summary>
	[Flags]
    public enum FXChannelFlags
    {
        /// <summary>
        /// All channels at once (as by default).
        /// </summary>
        All = -1,
        
        /// <summary>
        /// Disable an effect for all channels (resp. set the global volume of the Volume effect).
        /// </summary>
        None,
        
        /// <summary>
        /// left-front channel
        /// </summary>
        Channel1 = 0x1,
        
        /// <summary>
        /// right-front channel
        /// </summary>
        Channel2 = 0x2,
        
        /// <summary>
        /// Channel 3: depends on the multi-channel source (see above info).
        /// </summary>
        Channel3 = 0x4,
        
        /// <summary>
        /// Channel 4: depends on the multi-channel source (see above info).
        /// </summary>
        Channel4 = 0x8,
        
        /// <summary>
        /// Channel 5: depends on the multi-channel source (see above info).
        /// </summary>
        Channel5 = 0x10,
        
        /// <summary>
        /// Channel 6: depends on the multi-channel source (see above info).
        /// </summary>
        Channel6 = 0x20,
        
        /// <summary>
        /// Channel 7: depends on the multi-channel source (see above info).
        /// </summary>
        Channel7 = 0x40,
        
        /// <summary>
        /// Channel 8: depends on the multi-channel source (see above info).
        /// </summary>
        Channel8 = 0x80,
        
        /// <summary>
        /// Channel 9: depends on the multi-channel source (see above info).
        /// </summary>
        Channel9 = 0x100,
        
        /// <summary>
        /// Channel 10: depends on the multi-channel source (see above info).
        /// </summary>
        Channel10 = 0x200,
        
        /// <summary>
        /// Channel 11: depends on the multi-channel source (see above info).
        /// </summary>
        Channel11 = 0x400,
        
        /// <summary>
        /// Channel 12: depends on the multi-channel source (see above info).
        /// </summary>
        Channel12 = 0x800,
        
        /// <summary>
        /// Channel 13: depends on the multi-channel source (see above info).
        /// </summary>
        Channel13 = 0x1000,
        
        /// <summary>
        /// Channel 14: depends on the multi-channel source (see above info).
        /// </summary>
        Channel14 = 0x2000,
        
        /// <summary>
        /// Channel 15: depends on the multi-channel source (see above info).
        /// </summary>
        Channel15 = 0x4000,
        
        /// <summary>
        /// Channel 16: depends on the multi-channel source (see above info).
        /// </summary>
        Channel16 = 0x8000,
        
        /// <summary>
        /// Channel 17: depends on the multi-channel source (see above info).
        /// </summary>
        Channel17 = 0x10000,
        
        /// <summary>
        /// Channel 18: depends on the multi-channel source (see above info).
        /// </summary>
        Channel18 = 0x20000,
        
        /// <summary>
        /// Channel 19: depends on the multi-channel source (see above info).
        /// </summary>
        Channel19 = 0x40000,
        
        /// <summary>
        /// Channel 20: depends on the multi-channel source (see above info).
        /// </summary>
        Channel20 = 0x80000,
        
        /// <summary>
        /// Channel 21: depends on the multi-channel source (see above info).
        /// </summary>
        Channel21 = 0x100000,
        
        /// <summary>
        /// Channel 22: depends on the multi-channel source (see above info).
        /// </summary>
        Channel22 = 0x200000,
        
        /// <summary>
        /// Channel 23: depends on the multi-channel source (see above info).
        /// </summary>
        Channel23 = 0x400000,
        
        /// <summary>
        /// Channel 24: depends on the multi-channel source (see above info).
        /// </summary>
        Channel24 = 0x800000,
        
        /// <summary>
        /// Channel 25: depends on the multi-channel source (see above info).
        /// </summary>
        Channel25 = 0x1000000,
        
        /// <summary>
        /// Channel 26: depends on the multi-channel source (see above info).
        /// </summary>
        Channel26 = 0x2000000,
        
        /// <summary>
        /// Channel 27: depends on the multi-channel source (see above info).
        /// </summary>
        Channel27 = 0x4000000,
        
        /// <summary>
        /// Channel 28: depends on the multi-channel source (see above info).
        /// </summary>
        Channel28 = 0x8000000,
        
        /// <summary>
        /// Channel 29: depends on the multi-channel source (see above info).
        /// </summary>
        Channel29 = 0x10000000,
        
        /// <summary>
        /// Channel 30: depends on the multi-channel source (see above info).
        /// </summary>
        Channel30 = 0x20000000
    }
}