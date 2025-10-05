namespace ManagedBass.Fx
{
    /// <summary>
    /// A Reusable Channel which can Load files like a Player including Tempo, Pitch and Reverse options using BassFx.
    /// </summary>
    public class MediaPlayerFX : MediaPlayer
    {
        int _tempoHandle;

        #region Reverse
        bool _rev;

        /// <summary>
        /// Gets or Sets the Media playback direction.
        /// </summary>
        public bool Reverse
        {
            get => _rev;
            set
            {
                if (!Bass.ChannelSetAttribute(Handle, ChannelAttribute.ReverseDirection, value ? -1 : 1))
                    return;

                _rev = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Pitch
        double _pitch;

        /// <summary>
        /// Gets or Sets the Pitch in Semitones (-60 ... 0 ... 60).
        /// </summary>
        public double Pitch
        {
            get => _pitch;
            set
            {
                if (!Bass.ChannelSetAttribute(_tempoHandle, ChannelAttribute.Pitch, value))
                    return;

                _pitch = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Tempo
        double _tempo;

        /// <summary>
        /// Gets or Sets the Tempo in Percentage (-95% ... 0 ... 5000%)
        /// </summary>
        public double Tempo
        {
            get => _tempo;
            set
            {
                if (!Bass.ChannelSetAttribute(_tempoHandle, ChannelAttribute.Tempo, value))
                    return;

                _tempo = value;
                OnPropertyChanged();
            }
        }
        #endregion
        
        /// <summary>
        /// Loads the File Channel with FX.
        /// </summary>
        protected override int OnLoad(string FileName)
        {
            var h = Bass.CreateStream(FileName, Flags: BassFlags.Decode);

            if (h == 0)
                return 0;

            h = BassFx.TempoCreate(h, BassFlags.Decode | BassFlags.FxFreeSource);

            if (h == 0)
                return 0;

            _tempoHandle = h;

            return BassFx.ReverseCreate(h, 2, BassFlags.FxFreeSource);
        }

        /// <summary>
        /// Initializes Properties on every call to MediaPlayer.Load.
        /// </summary>
        protected override void InitProperties()
        {
            Reverse = _rev;

            base.InitProperties();

            Tempo = _tempo;
            Pitch = _pitch;
        }
    }
}