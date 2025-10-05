namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Rotate Effect.
    /// </summary>
    /// <remarks>
    /// <para>This is a volume rotate effect between even channels, just like 2 channels playing ping-pong between each other.</para>
    /// <para>The <see cref="Rate"/> defines the speed in Hz.</para>
    /// </remarks>
    public sealed class RotateEffect : Effect<RotateParameters>
    {
        /// <summary>
        /// Rotation rate/speed in Hz (A negative rate can be used for reverse direction).
        /// </summary>
        public double Rate
        {
            get => Parameters.fRate;
            set
            {
                Parameters.fRate = (float)value;

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