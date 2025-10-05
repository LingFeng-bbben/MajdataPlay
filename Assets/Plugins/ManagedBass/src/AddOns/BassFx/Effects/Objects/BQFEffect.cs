namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx BiQuad Filter Effect.
    /// </summary>
    /// <remarks>
    /// BiQuad filters are second-order recursive linear filters.
    /// </remarks>
    public class BQFEffect : Effect<BQFParameters>
    {
        /// <summary>
        /// Gain in dB (-15...0...+15). Default 0dB (used only for PEAKINGEQ and Shelving filters).
        /// </summary>
        public double Gain
        {
            get => Parameters.fGain;
            set
            {
                Parameters.fGain = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// BQF Effect Kind.
        /// </summary>
        public BQFType EffectType
        {
            get => Parameters.lFilter;
            set
            {
                Parameters.lFilter = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bandwidth in octaves (0.1...4...n), Q is not in use (<see cref="Bandwidth"/> has priority over <see cref="Q"/>). Default = 1 (0=not in use).
        /// The bandwidth in octaves (between -3 dB frequencies for <see cref="BQFType.BandPass"/> and <see cref="BQFType.Notch"/> or between midpoint (dBgain/2) gain frequencies for PEAKINGEQ).
        /// </summary>
        public double Bandwidth
        {
            get => Parameters.fBandwidth;
            set
            {
                Parameters.fBandwidth = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Cut-off frequency (Center in PEAKINGEQ and Shelving filters) in Hz (1...info.freq/2). Default = 200Hz.
        /// </summary>
        public double Center
        {
            get => Parameters.fCenter;
            set
            {
                Parameters.fCenter = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// EE kinda definition of Q (0.1...1...n), if <see cref="Bandwidth"/> is not in use. Default = 0.0 (0=not in use).
        /// </summary>
        public double Q
        {
            get => Parameters.fQ;
            set
            {
                Parameters.fQ = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A shelf slope parameter (linear, used only with Shelving filters) (0...1...n). Default = 0.0.
        /// When 1, the shelf slope is as steep as you can get it and remain monotonically increasing or decreasing gain with frequency.
        /// </summary>
        public double S
        {
            get => Parameters.fS;
            set
            {
                Parameters.fS = (float)value;

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