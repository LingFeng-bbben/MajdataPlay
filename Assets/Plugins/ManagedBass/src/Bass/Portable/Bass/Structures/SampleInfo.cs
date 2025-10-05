using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Used with <see cref="Bass.SampleGetInfo(int,SampleInfo)" /> and <see cref="Bass.SampleSetInfo" /> to retrieve and set the default playback attributes of a sample.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a sample has 3D functionality, the <see cref="InsideAngle"/> and <see cref="OutsideAngle"/> angles decide how wide the sound is projected around the orientation angle (as set via <see cref="Bass.ChannelSet3DPosition" />).
    /// Within the inside angle the volume level is the level set in the volume member (or the <see cref="ChannelAttribute.Volume" /> attribute when the sample is playing).
    /// Outside the outer angle, the volume changes according to the outvol value.
    /// Between the inner and outer angles, the volume gradually changes between the inner and outer volume levels.
    /// If the inner and outer angles are 360 degrees, then the sound is transmitted equally in all directions.
    /// </para>
    /// <para>When VAM is enabled, and neither the <see cref="VAMMode.Hardware"/> or <see cref="VAMMode.Software"/> flags are specified, then the sample will be played in hardware if resources are available, and in software if no hardware resources are available.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public class SampleInfo
    {
        /// <summary>
        /// Default playback rate (set to 44100 by default).
        /// </summary>
        public int Frequency = 44100;

        /// <summary>
        /// Default volume... 0 (silent) to 1 (full, default).
        /// </summary>
        public float Volume = 1f;

        /// <summary>
        /// Default panning position -1 (full left) to +1 (full right) - defaulted to 0 = centre.
        /// </summary>
        public float Pan;

        /// <summary>
        /// A combination of <see cref="BassFlags"/>.
        /// </summary>
        public BassFlags Flags;

        /// <summary>
        /// The Length in bytes.
        /// </summary>
        public int Length;

        /// <summary>
        /// Maximum number of simultaneous playbacks (defaulted to 1).
        /// </summary>
        public int Max = 1;

        /// <summary>
        /// The original resolution (bits per sample)... 0 = undefined (default).
        /// </summary>
        public int OriginalResolution;

        /// <summary>
        /// Number of channels... 1=mono, 2=stereo (default), etc.
        /// </summary>
        public int Channels = 2;

        /// <summary>
        /// Minimum time gap in milliseconds between creating channels using <see cref="Bass.SampleGetChannel" />.
        /// This can be used to prevent flanging effects caused by playing a sample multiple times very close to eachother.
        /// The default setting, when loading/creating a sample, is 0 (disabled).
        /// </summary>
        public int MinGap;

        /// <summary>
        /// The 3D processing mode...
        /// </summary>
        public Mode3D Mode3D;

        /// <summary>
        /// The minimum distance (default 0). The sample's volume is at maximum when the listener is within this distance.
        /// </summary>
        public float MinDistance;

        /// <summary>
        /// The maximum distance (default 0). The sample's volume stops decreasing when the listener is beyond this distance.
        /// </summary>
        public float MaxDistance;

        /// <summary>
        /// The angle of the inside projection cone in degrees... 0 (no cone, default) - 360 (sphere).
        /// </summary>
        public int InsideAngle;

        /// <summary>
        /// The angle of the outside projection cone in degrees... 0 (no cone, default) - 360 (sphere).
        /// </summary>
        public int OutsideAngle;

        /// <summary>
        /// The delta-volume outside the outer projection cone... 0 (silent) to 1 (full, default) - same as inside the cone.
        /// </summary>
        public float OutsideVolume = 1f;

        /// <summary>
        /// The sample's DX7 voice allocation/management settings (if VAM is enabled)...a combination of <see cref="VAMMode" /> flags.
        /// </summary>
        public VAMMode VAM = VAMMode.Hardware;

        /// <summary>
        /// Priority, used with the <see cref="VAMMode.TerminatePriority"/> flag... 0 (min, default) - 0xFFFFFFFF (max)
        /// </summary>
        public int Priority;
    }
}
