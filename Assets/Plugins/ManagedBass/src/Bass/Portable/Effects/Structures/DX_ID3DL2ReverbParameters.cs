using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 ID3L2 Reverb Effect (Windows only).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DX_ID3DL2ReverbParameters : IEffectParameter
    {
        /// <summary>
        /// Attenuation of the room effect, in millibels (mB), in the range from -10000 to 0. The default value is -1000 mB.
        /// </summary>
        public int lRoom = -1000;

        /// <summary>
        /// Attenuation of the room high-frequency effect, in mB, in the range from -10000 to 0. The default value is 0 mB.
        /// </summary>
        public int lRoomHF;

        /// <summary>
        /// Rolloff factor for the reflected signals, in the range from 0 to 10. The default value is 0.0.
        /// </summary>
        public float flRoomRolloffFactor;

        /// <summary>
        /// Decay time, in seconds, in the range from 0.1 to 20. The default value is 1.49 second.
        /// </summary>
        public float flDecayTime = 1.49f;

        /// <summary>
        /// Ratio of the decay time at high frequencies to the decay time at low frequencies, in the range from 0.1 to 2. The default value is 0.83.
        /// </summary>
        public float flDecayHFRatio = 0.83f;

        /// <summary>
        /// Attenuation of early reflections relative to lRoom, in mB, in the range from -10000 to 1000. The default value is -2602 mB.
        /// </summary>
        public int lReflections = -2602;

        /// <summary>
        /// Delay time of the first reflection relative to the direct path, in seconds, in the range from 0 to 0.3. The default value is 0.007 seconds.
        /// </summary>
        public float flReflectionsDelay = 0.007f;

        /// <summary>
        /// Attenuation of late reverberation relative to lRoom, in mB, in the range from -10000 to 2000. The default value is 200 mB.
        /// </summary>
        public int lReverb = 200;

        /// <summary>
        /// Time limit between the early reflections and the late reverberation relative to the time of the first reflection, in seconds, in the range from 0 to 0.1. The default value is 0.011 seconds.
        /// </summary>
        public float flReverbDelay = 0.011f;

        /// <summary>
        /// Echo density in the late reverberation decay, in percent, in the range from 0 to 100. The default value is 100.0 percent.
        /// </summary>
        public float flDiffusion = 100;

        /// <summary>
        /// Modal density in the late reverberation decay, in percent, in the range from 0 to 100. The default value is 100.0 percent.
        /// </summary>
        public float flDensity = 100;

        /// <summary>
        /// Reference high frequency, in hertz, in the range from 20 to 20000. The default value is 5000.0 Hz.
        /// </summary>
        public float flHFReference = 5000;
        
        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DX_I3DL2Reverb;
    }
}