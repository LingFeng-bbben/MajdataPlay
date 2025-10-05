using System;

namespace ManagedBass
{
    /// <summary>
    /// Initialization flags to be used with <see cref="Bass.Init" />
    /// </summary>
    [Flags]
    public enum DeviceInitFlags
    {
        /// <summary>
        /// 0 = 16 bit, stereo, no 3D, no Latency calc, no Speaker Assignments
        /// </summary>
        Default,

        /// <summary>
        /// Use 8 bit resolution, else 16 bit.
        /// </summary>
        Byte = 0x1,

        /// <summary>
        /// Use mono, else stereo.
        /// </summary>
        Mono = 0x2,

        /// <summary>
        /// Enable 3D functionality.
        /// Note: If this is not specified when initilizing BASS,
        /// then the <see cref="BassFlags.Bass3D"/> is ignored when loading/creating a sample/stream/music.
        /// </summary>
        Device3D = 0x4,

        /// <summary>
        /// Limit output to 16 bit.
        /// </summary>
        Bits16 = 0x8,

        /// <summary>
        /// Calculate device latency (<see cref="BassInfo"/> struct).
        /// </summary>
        Latency = 0x100,

        /// <summary>
        /// Use the Windows control panel setting to detect the number of speakers.
        /// Only use this option if BASS doesn't detect the correct number of supported
        /// speakers automatically and you want to force BASS to use the number of speakers
        /// as configured in the windows control panel.
        /// </summary>
        CPSpeakers = 0x400,

        /// <summary>
        /// Force enabling of speaker assignment (always 8 speakers will be used regardless if the soundcard supports them).
        /// Only use this option if BASS doesn't detect the correct number of supported
        /// speakers automatically and you want to force BASS to use 8 speakers.
        /// </summary>
        ForcedSpeakerAssignment = 0x800,

        /// <summary>
        /// Ignore speaker arrangement
        /// </summary>
        NoSpeakerAssignment = 0x1000,

        /// <summary>
        /// Linux-only: Initialize the device using the ALSA "dmix" plugin, else initialize the device for exclusive access.
        /// </summary>
        DMix = 0x2000,

        /// <summary>
        /// Set the device's output rate to freq, otherwise leave it as it is.
        /// </summary>
        Frequency = 0x4000,

        /// <summary>
        /// Limit output to stereo.
        /// </summary>
        Stereo = 0x8000,

        /// <summary>
        /// Hog/Exclusive mode.
        /// </summary>
        Hog = 0x10000,

        /// <summary>
        /// Use AudioTrack output.
        /// </summary>
        AudioTrack = 0x20000,

        /// <summary>
        /// Use DirectSound output.
        /// </summary>
        DirectSound = 0x40000
    }
}
