using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings
{
    public enum HidOpenPriority
    {
        /// <summary>
        /// The lowest priority.
        /// </summary>
        Idle = -2,

        /// <summary>
        /// Very low priority.
        /// </summary>
        VeryLow = -1,

        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 0,

        /// <summary>
        /// The default priority.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// High priority.
        /// </summary>
        High = 2,

        /// <summary>
        /// The highest priority.
        /// </summary>
        VeryHigh = 3
    }
}