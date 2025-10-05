using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedBass.Fx
{
    public sealed class Volume : Effect<VolumeParameters>
    {
        /// <summary>
        /// The new volume level... 0 = silent, 1.0 = normal, above 1.0 = amplification. The default value is 1.
        /// </summary>
        public float Target
        {
            get => Parameters.fTarget;
            set
            {
                Parameters.fTarget = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The current volume level... -1 = leave existing current level when setting parameters. The default value is 1.
        /// </summary>
        public float Current
        {
            get => Parameters.fCurrent;
            set
            {
                Parameters.fCurrent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The time to take to transition from the current level to the new level, in seconds. The default value is 0.
        /// </summary>
        public float Time
        {
            get => Parameters.fTime;
            set
            {
                Parameters.fTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The curve to use in the transition... False for linear, true for logarithmic. The default value is false.
        /// </summary>
        public bool Curve
        {
            get => Convert.ToBoolean(Parameters.lCurve);
            set
            {
                Parameters.lCurve = (uint)(value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags Channels
        {
            get => Parameters.lChannel;
            set
            {
                Parameters.lChannel = value;

                OnPropertyChanged();
            }
        }
    }
}
