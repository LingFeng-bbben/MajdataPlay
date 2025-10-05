using System;
using System.Runtime.CompilerServices;

namespace ManagedBass
{
    public static class CrossPlatformHelper
    {
        /// <summary>
        /// Returns true is  
        /// </summary>
        public static bool IsDynamicCodeSupported =>
#if NETCOREAPP3_0_OR_GREATER
            RuntimeFeature.IsDynamicCodeSupported;
#elif __IOS__
            false;
#else
            true;
#endif
        
    }
}