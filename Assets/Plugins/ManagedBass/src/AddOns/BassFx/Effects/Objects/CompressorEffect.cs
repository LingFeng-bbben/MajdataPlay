namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Compressor Effect.
    /// </summary>
    /// <remarks>
    /// Compressors are commonly used in recording to control the level, by making loud passages quieter, and quiet passages louder.
    /// This is useful in allowing a vocalist to sing quiet and loud for different emphasis, and always be heard clearly in the mix.
    /// Compression is generally applied to guitar to give clean sustain, where the start of a note is "squashed" with the gain automatically increased as the not fades away.
    /// Compressors take a short time to react to a picked note, and it can be difficult to find settings that react quickly enough to the volume change without killing the natural attack sound of your guitar.
    /// </remarks>
    public sealed class CompressorEffect : Effect<CompressorParameters>
    {
        /// <summary>
        /// Time in ms before compression reaches its full value, in the range from 0.01 to 500. Default = 20.
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
        /// Time (speed) in ms at which compression is stopped after Input drops below <see cref="Threshold"/>, in the range from 50 to 3000. Default = 200.
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
        /// Point in dB at which compression begins, in decibels, in the range from -60 to 0. Default = -15.
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
        /// Output gain in dB of signal after compression, in the range from -60 to 60. Default = 5.
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
        /// Compression ratio, in the range from 1 to 100. Default = 3.
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