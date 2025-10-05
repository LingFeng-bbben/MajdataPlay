namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 Compressor Effect (Windows only).
    /// </summary>
    public sealed class DXCompressorEffect : Effect<DXCompressorParameters>
    {
        /// <summary>
        /// Output gain of signal in dB after compression, in the range from -60 to 60. The default value is 0 dB.
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
        /// Time in ms before compression reaches its full value, in the range from 0.01 to 500. The default value is 10 ms.
        /// </summary>
        public double Attack
        {
            get => Parameters.fAttack;
            set
            {
                Parameters.fAttack = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Time (speed) in ms at which compression is stopped after input drops below <see cref="Threshold"/>, in the range from 50 to 3000. The default value is 200 ms.
        /// </summary>
        public double Release
        {
            get => Parameters.fRelease;
            set
            {
                Parameters.fRelease = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Point at which compression begins, in dB, in the range from -60 to 0. The default value is -20 dB.
        /// </summary>
        public double Threshold
        {
            get => Parameters.fThreshold;
            set
            {
                Parameters.fThreshold = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Compression ratio, in the range from 1 to 100. The default value is 3, which means 3:1 compression.
        /// </summary>
        public double Ratio
        {
            get => Parameters.fRatio;
            set
            {
                Parameters.fRatio = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Time in ms after <see cref="Threshold"/> is reached before attack phase is started, in milliseconds, in the range from 0 to 4. The default value is 4 ms.
        /// </summary>
        public double Predelay
        {
            get => Parameters.fPredelay;
            set
            {
                Parameters.fPredelay = (float)value;

                OnPropertyChanged();
            }
        }
    }
}