namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 ID3DL2 Reverb Effect (Windows only).
    /// </summary>
    public sealed class DX_ID3DL2ReverbEffect : Effect<DX_ID3DL2ReverbParameters>
    {
        /// <summary>
        /// Attenuation of the room effect, in millibels (mB), in the range from -10000 to 0. The default value is -1000 mB.
        /// </summary>
        public int Room
        {
            get => Parameters.lRoom;
            set
            {
                Parameters.lRoom = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Attenuation of the room high-frequency effect, in mB, in the range from -10000 to 0. The default value is 0 mB.
        /// </summary>
        public int RoomHF
        {
            get => Parameters.lRoomHF;
            set
            {
                Parameters.lRoomHF = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Rolloff factor for the reflected signals, in the range from 0 to 10. The default value is 0.0.
        /// </summary>
        public double RoomRolloffFactor
        {
            get => Parameters.flRoomRolloffFactor;
            set
            {
                Parameters.flRoomRolloffFactor = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Decay time, in seconds, in the range from 0.1 to 20. The default value is 1.49 second.
        /// </summary>
        public double DecayTime
        {
            get => Parameters.flDecayTime;
            set
            {
                Parameters.flDecayTime = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ratio of the decay time at high frequencies to the decay time at low frequencies, in the range from 0.1 to 2. The default value is 0.83.
        /// </summary>
        public double DecayHFRatio
        {
            get => Parameters.flDecayHFRatio;
            set
            {
                Parameters.flDecayHFRatio = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Attenuation of early reflections relative to lRoom, in mB, in the range from -10000 to 1000. The default value is -2602 mB.
        /// </summary>
        public int Reflections
        {
            get => Parameters.lReflections;
            set
            {
                Parameters.lReflections = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Delay time of the first reflection relative to the direct path, in seconds, in the range from 0 to 0.3. The default value is 0.007 seconds.
        /// </summary>
        public double ReflectionsDelay
        {
            get => Parameters.flReflectionsDelay;
            set
            {
                Parameters.flReflectionsDelay = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Attenuation of late reverberation relative to lRoom, in mB, in the range from -10000 to 2000. The default value is 200 mB.
        /// </summary>
        public int Reverb
        {
            get => Parameters.lReverb;
            set
            {
                Parameters.lReverb = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Time limit between the early reflections and the late reverberation relative to the time of the first reflection, in seconds, in the range from 0 to 0.1. The default value is 0.011 seconds.
        /// </summary>
        public double ReverbDelay
        {
            get => Parameters.flReverbDelay;
            set
            {
                Parameters.flReverbDelay = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Echo density in the late reverberation decay, in percent, in the range from 0 to 100. The default value is 100.0 percent.
        /// </summary>
        public double Diffusion
        {
            get => Parameters.flDiffusion;
            set
            {
                Parameters.flDiffusion = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Modal density in the late reverberation decay, in percent, in the range from 0 to 100. The default value is 100.0 percent.
        /// </summary>
        public double Density
        {
            get => Parameters.flDensity;
            set
            {
                Parameters.flDensity = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reference high frequency, in hertz, in the range from 20 to 20000. The default value is 5000.0 Hz.
        /// </summary>
        public double HFReference
        {
            get => Parameters.flHFReference;
            set
            {
                Parameters.flHFReference = (float)value;

                OnPropertyChanged();
            }
        }
    }
}