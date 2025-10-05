namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 Echo Effect.
    /// </summary>
    public sealed class DXEchoEffect : Effect<DXEchoParameters>
    {
        /// <summary>
        /// Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0 (default) through 100 (all wet).
        /// </summary>
        public double WetDryMix
        {
            get => Parameters.fWetDryMix;
            set
            {
                Parameters.fWetDryMix = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of output fed back into input, in the range from 0 through 100. The default value is 0.
        /// </summary>
        public double Feedback
        {
            get => Parameters.fFeedback;
            set
            {
                Parameters.fFeedback = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Delay for left channel, in milliseconds, in the range from 1 through 2000. The default value is 333 ms.
        /// </summary>
        public double LeftDelay
        {
            get => Parameters.fLeftDelay;
            set
            {
                Parameters.fLeftDelay = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Delay for right channel, in milliseconds, in the range from 1 through 2000. The default value is 333 ms.
        /// </summary>
        public double RightDelay
        {
            get => Parameters.fRightDelay;
            set
            {
                Parameters.fRightDelay = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Value that specifies whether to swap left and right delays with each successive echo. The default value is <see langword="false" />, meaning no swap.
        /// </summary>
        public bool PanDelay
        {
            get => Parameters.lPanDelay;
            set
            {
                Parameters.lPanDelay = value;

                OnPropertyChanged();
            }
        }
    }
}