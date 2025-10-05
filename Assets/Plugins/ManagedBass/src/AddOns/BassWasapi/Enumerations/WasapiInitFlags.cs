using System;

namespace ManagedBass.Wasapi
{
    /// <summary>
	/// BassWasapi initialization flags to be used with <see cref="BassWasapi.Init" />.
	/// </summary>
	[Flags]
    public enum WasapiInitFlags
    {
        /// <summary>
        /// Init the device (endpoint) in shared mode.
        /// </summary>
        Shared,

        /// <summary>
        /// Init the device (endpoint) in exclusive mode.
        /// </summary>
        Exclusive = 0x1,
        
        /// <summary>
        /// Automatically choose another sample format if the specified format is not supported.
        /// If possible, a higher sample rate than freq will be used, rather than a lower one.
        /// </summary>
        AutoFormat = 0x2,

        /// <summary>
        /// Enable double buffering, for use by <see cref="BassWasapi.GetData(IntPtr,int)"/> and <see cref="BassWasapi.GetLevel()"/>. 
        /// This requires the BASS <see cref="Bass.NoSoundDevice"/> device to have been initilized, via <see cref="Bass.Init"/>.
        /// Internally, a BASS stream is used for that, so the usual <see cref="DataFlags"/> flags are supported.
        /// </summary>
        Buffer = 0x4,
        
        /// <summary>
        /// Enables the event-driven WASAPI system.
        /// It is only supported when a <see cref="WasapiProcedure"/> function is provided, ie. not when using <see cref="BassWasapi.PutData(IntPtr,int)"/>.
        /// When used with shared mode, the User-provided 'Buffer' and 'period' lengths are ignored 
        /// and WASAPI decides what Buffer to use (<see cref="BassWasapi.Info"/> can be used to check that).
        /// </summary>
        EventDriven = 0x10,

        /// <summary>
        /// buffer and period are in samples rather than seconds.
        /// </summary>
        Samples = 0x20,

        /// <summary>
        /// Apply dither (TPDF) when converting floating-point sample data to the device's format.
        /// This flag only has effect on exclusive mode output.
        /// </summary>
        Dither = 0x40,

        /// <summary>
        /// Request raw mode, which bypasses any sound enhancements that have been enabled on the device. This is only available on Windows 8.1 and above. 
        /// </summary>
        Raw = 0x80,

        /// <summary>
        /// Call the callback function asynchronously. This only applies to event-driven exclusive mode output and is otherwise ignored. When enabled,
        /// a buffer is filled asynchronously in advance. This reduces the chances of underruns but also increases latency by up to one buffer length. If an underrun does occur, a silent buffer (rather than nothing) is still sent to the device, which can prevent sound glitches on some devices following an underrun. 
        /// </summary>
        Async = 0x100,


        // --- WASAPI Device Category Flags ---
        // One of the following can also be used to set the audio category on Windows 8 and above (otherwise ignored).

        /// <summary>
        /// A mask to isolate the category flags. 
        /// </summary>
        CategoryMask = 0xf000,

        /// <summary>
        /// Other audio stream. 
        /// </summary>
        CategoryOther = 0x0000,

        /// <summary>
        /// Unknown flag.
        /// </summary>
        CategoryForegroundMediaOnly	=0x1000,

        /// <summary>
        /// Unknown flag.
        /// </summary>
        CategoryBackgroundMediaCapable=0x2000,

        /// <summary>
        /// Real-time communications, such as VOIP or chat. 
        /// </summary>
        CategoryCommunications = 0x3000,

        /// <summary>
        /// Alert sounds. For output devices only. 
        /// </summary>
        CategoryAlerts = 0x4000,

        /// <summary>
        /// Sound effects. For output devices only. 
        /// </summary>
        CategorySoundEffects = 0x5000,

        /// <summary>
        /// Game sound effects. For output devices only. 
        /// </summary>
        CategoryGameEffects = 0x6000,

        /// <summary>
        /// Background audio for games. For output devices only. 
        /// </summary>
        CategoryGameMedia = 0x7000,

        /// <summary>
        /// Game chat audio. Similar to COMMUNICATIONS except that this will not attenuate other streams. For output devices only. 
        /// </summary>
        CategoryGameChat = 0x8000,

        /// <summary>
        /// Speech. 
        /// </summary>
        CategorySpeech = 0x9000,

        /// <summary>
        /// Stream that includes audio with dialog. For output devices only. 
        /// </summary>
        CategoryMovie = 0xa000,

        /// <summary>
        /// Stream that includes audio without dialog. For output devices only. 
        /// </summary>
        CategoryMedia = 0xb000
    }
}