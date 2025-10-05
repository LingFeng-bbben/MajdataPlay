namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Reverb (Freeverb) Effect.
    /// </summary>
    public sealed class ReverbEffect : Effect<ReverbParameters>
    {
        /// <summary>
        /// Damping factor (0.0...1.0, def. 0.5).
        /// </summary>
        public double Damp
        {
            get => Parameters.fDamp;
            set
            {
                Parameters.fDamp = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Dry (unaffected) signal mix (0.0...1.0, def. 0).
        /// </summary>
        public double DryMix
        {
            get => Parameters.fDryMix;
            set
            {
                Parameters.fDryMix = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Room size (0.0...1.0, def. 0.5).
        /// </summary>
        public double RoomSize
        {
            get => Parameters.fRoomSize;
            set
            {
                Parameters.fRoomSize = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Wet (affected) signal mix (0.0...3.0, def. 1.0).
        /// </summary>
        public double WetMix
        {
            get => Parameters.fWetMix;
            set
            {
                Parameters.fWetMix = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Stereo width (0.0...1.0, def. 1.0).
        /// </summary>
        /// <remarks>It should at least be 4 for moderate scaling ratios. A value of 32 is recommended for best quality (better quality = higher CPU usage).</remarks>
        public double Width
        {
            get => Parameters.fWidth;
            set
            {
                Parameters.fWidth = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Mode: 0 = no freeze or 1 = freeze, def. 0 (no freeze).
        /// </summary>
        public int Mode
        {
            get => Parameters.lMode;
            set
            {
                Parameters.lMode = value;
                
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