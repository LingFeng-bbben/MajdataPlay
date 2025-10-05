using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Configure BASS.
        /// </summary>
        /// <param name="Option">One of <see cref="Configuration"/> values.</param>
        /// <param name="NewValue">The New Value of the option.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_SetConfig")]
        public static extern bool Configure(Configuration Option, bool NewValue);

        /// <summary>
        /// Configure BASS.
        /// </summary>
        /// <param name="Option">One of <see cref="Configuration"/> values.</param>
        /// <param name="NewValue">The New Value of the option.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_SetConfig")]
        public static extern bool Configure(Configuration Option, int NewValue);

        /// <summary>
        /// Configure BASS.
        /// </summary>
        /// <param name="Option">One of <see cref="Configuration"/> values.</param>
        /// <param name="NewValue">The New Value of the option.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_SetConfigPtr")]
        public static extern bool Configure(Configuration Option, IntPtr NewValue);

        /// <summary>
        /// Get Configuration value.
        /// </summary>
        /// <param name="Option">One of <see cref="Configuration"/> values.</param>
        /// <returns>The configuration value or -1 on error. Use <see cref="LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_GetConfig")]
        public static extern int GetConfig(Configuration Option);

        /// <summary>
        /// Get Configuration value.
        /// </summary>
        /// <param name="Option">One of <see cref="Configuration"/> values.</param>
        /// <returns><see cref="IntPtr.Zero"/> on error. Use <see cref="LastError" /> to get the error code.</returns>
        [DllImport(DllName, EntryPoint = "BASS_GetConfigPtr")]
        public static extern IntPtr GetConfigPtr(Configuration Option);

        /// <summary>
        /// Get boolean Configuration value.
        /// </summary>
        /// <param name="Option">One of the <see cref="Configuration"/> values.</param>
        /// <returns>The configuration value. In case of error, <see langword="false" /> is returned.</returns>
        public static bool GetConfigBool(Configuration Option)
        {
            var val = GetConfig(Option);

            return val != -1 && val != 0;
        }

        /// <summary>
        /// Gets if Floating-Point audio is supported on the current platform.
        /// </summary>
        public static bool Float => GetConfigBool(Configuration.Float);

        /// <summary>
        /// The Buffer Length in milliseconds (default = 500).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The minimum Length is 1ms above the update period (See <see cref="UpdatePeriod"/>),
        /// the maximum is 5000 milliseconds.
        /// If the Length specified is outside this range, it is automatically capped.
        /// Increasing the Length, decreases
        /// the chance of the sound possibly breaking-up on slower computers, but also
        /// increases the latency for DSP/FX.
        /// </para>
        /// <para>
        /// Small Buffer lengths are only required if the sound is going to be changing in real-time, for example, in a soft-synth.
        /// If you need to use a small Buffer, then the <see cref="BassInfo.MinBufferLength"/> should be used to get the recommended
        /// minimum Buffer Length supported by the device and it's drivers.
        /// Even at this default Length, it's still possible that the sound could break up on some systems,
        /// it's also possible that smaller buffers may be fine.
        /// So when using small buffers, you should have an option in your software for the User to finetune the Length used, for optimal performance.
        /// Using this config option only affects the HMUSIC/HSTREAM channels that you create afterwards, not the ones that have already been created.
        /// So you can have channels with differing Buffer lengths by using this config option each time before creating them.
        /// If automatic updating is disabled, make sure you call <see cref="Update"/>
        /// frequently enough to keep the buffers updated.
        /// </para>
        /// </remarks>
        public static int PlaybackBufferLength
        {
            get => GetConfig(Configuration.PlaybackBufferLength);
            set => Configure(Configuration.PlaybackBufferLength, value);
        }

        /// <summary>
        /// The update period of HSTREAM and HMUSIC channel playback buffers in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 0 = disable automatic updating.
        /// The minimum period is 5ms, the maximum is 100ms.
        /// If the period specified is outside this range, it is automatically capped.
        /// The default period is 100ms.
        /// </para>
        /// <para>
        /// The update period is the amount of time between updates of the playback buffers of HSTREAM/HMUSIC channels.
        /// Shorter update periods allow smaller buffers to be set with the <see cref="PlaybackBufferLength"/> option, but
        /// as the rate of updates increases, so the overhead of setting up the updates becomes a greater part of the CPU usage.
        /// The update period only affects HSTREAM and HMUSIC channels, it does not affect samples.
        /// Nor does it have any effect on decoding channels, as they are not played.
        /// BASS creates one or more threads (determined by <see cref="UpdateThreads"/>)
        /// specifically to perform the updating, except when automatic updating is disabled
        /// (period=0) - then you must regularly call <see cref="Update"/> or <see cref="ChannelUpdate"/> instead.
        /// This allows you to synchronize BASS's CPU usage with your program's.
        /// For example, in a game loop you could call <see cref="Update"/>
        /// once per frame, which keeps all the processing in sync so that the frame rate is as smooth as possible.
        /// <see cref="Update"/> should be called at least around 8 times per second, even more often if the <see cref="PlaybackBufferLength"/>
        /// option is used to set smaller buffers.
        /// The update period can be altered at any time, including during playback.
        /// </para>
        /// </remarks>
        public static int UpdatePeriod
        {
            get => GetConfig(Configuration.UpdatePeriod);
            set => Configure(Configuration.UpdatePeriod, value);
        }

        /// <summary>
        /// Global sample volume level... 0 (silent) - 10000 (full).
        /// </summary>
        /// <remarks>
        /// This config option allows you to have control over the volume levels of all the samples,
        /// which is useful for setup options (eg. separate music and fx volume controls).
        /// A channel's final volume = channel volume * global volume / max volume.
        /// So, for example, if a stream channel's volume is 0.5 and the global stream volume is 8000,
        /// then effectively the stream's volume level is 0.4 (0.5 * 8000 / 10000 = 0.4).
        /// </remarks>
        public static int GlobalSampleVolume
        {
            get => GetConfig(Configuration.GlobalSampleVolume);
            set => Configure(Configuration.GlobalSampleVolume, value);
        }

        /// <summary>
        /// Global stream volume level... 0 (silent) - 10000 (full).
        /// </summary>
        /// <remarks>
        /// This config option allows you to have control over the volume levels of all streams,
        /// which is useful for setup options (eg. separate music and fx volume controls).
        /// A channel's final volume = channel volume * global volume / max volume.
        /// So, for example, if a stream channel's volume is 0.5 and the global stream volume is 8000,
        /// then effectively the stream's volume level is 0.4 (0.5 * 8000 / 10000 = 0.4).
        /// </remarks>
        public static int GlobalStreamVolume
        {
            get => GetConfig(Configuration.GlobalStreamVolume);
            set => Configure(Configuration.GlobalStreamVolume, value);
        }

        /// <summary>
        /// Global music volume level... 0 (silent) - 10000 (full).
        /// </summary>
        /// <remarks>
        /// This config option allows you to have control over the volume levels of all the MOD musics,
        /// which is useful for setup options (eg. separate music and fx volume controls).
        /// A channel's final volume = channel volume * global volume / max volume.
        /// So, for example, if a stream channel's volume is 0.5 and the global stream volume is 8000,
        /// then effectively the stream's volume level is 0.4 (0.5 * 8000 / 10000 = 0.4).
        /// </remarks>
        public static int GlobalMusicVolume
        {
            get => GetConfig(Configuration.GlobalMusicVolume);
            set => Configure(Configuration.GlobalMusicVolume, value);
        }

        /// <summary>
        /// Volume translation curve... false = Linear (Default), true = Logarithmic.
        /// </summary>
        /// <remarks>
        /// DirectSound uses logarithmic volume and panning curves, which can be awkward to work with.
        /// For example, with a logarithmic curve, the audible difference between 10000 and 9000,
        /// is not the same as between 9000 and 8000.
        /// With a linear "curve" the audible difference is spread equally across the whole range of values,
        /// so in the previous example the audible difference between 10000 and 9000,
        /// and between 9000 and 8000 would be identical.
        /// When using the linear curve, the volume range is from 0% (silent) to 100% (full).
        /// When using the logarithmic curve, the volume range is from -100 dB (effectively silent) to 0 dB (full).
        /// For example, a volume level of 0.5 is 50% linear or -50 dB logarithmic.
        /// </remarks>
        public static bool LogarithmicVolumeCurve
        {
            get => GetConfigBool(Configuration.LogarithmicVolumeCurve);
            set => Configure(Configuration.LogarithmicVolumeCurve, value);
        }

        /// <summary>
        /// Panning translation curve... false = Linear (Default), true = Logarithmic.
        /// </summary>
        /// <remarks>
        /// The panning curve affects panning in exactly the same way as the <see cref="LogarithmicVolumeCurve"/> affects the volume.
        /// </remarks>
        public static bool LogarithmicPanningCurve
        {
            get => GetConfigBool(Configuration.LogarithmicPanCurve);
            set => Configure(Configuration.LogarithmicPanCurve, value);
        }

        /// <summary>
        /// Pass 32-bit floating-point sample data to all <see cref="DSPProcedure"/> callback functions.
        /// </summary>
        /// <remarks>
        /// Normally DSP functions receive sample data in whatever format the channel is using, ie. it can be 8, 16 or 32-bit.
        /// But using this config option, BASS will convert 8/16-bit sample data to 32-bit floating-point before passing
        /// it to DSP functions, and then convert it back after all the DSP functions are done.
        /// As well as simplifying the DSP code (no need for 8/16-bit processing),
        /// this also means that there is no degradation of quality as sample data passes through a chain of DSP.
        /// This config option also applies to effects set via <see cref="ChannelSetFX"/>,
        /// except for DX8 effects when using the "With FX flag" DX8 effect implementation.
        /// Changing the setting while there are DSP or FX set could cause problems, so should be avoided.
        /// <para>
        /// <b>Platform-specific</b>: On Android and Windows CE, 8.24 bit fixed-point is used instead of floating-point.
        /// Floating-point DX8 effect processing requires DirectX 9 (or above) on Windows.
        /// </para>
        /// </remarks>
        public static bool FloatingPointDSP
        {
            get => GetConfigBool(Configuration.FloatDSP);
            set => Configure(Configuration.FloatDSP, value);
        }

        /// <summary>
        /// The number of threads to use for updating playback buffers... 0 = Disable automatic updating.
        /// </summary>
        /// <remarks>
        /// The number of update threads determines how many HSTREAM/HMUSIC channel playback buffers can be updated in parallel;
        /// each thread can process one channel at a time.
        /// The default is to use a single thread, but additional threads can be used to take advantage of multiple CPU cores.
        /// There is generally nothing much to be gained by creating more threads than there are CPU cores,
        /// but one benefit of using multiple threads even with a single CPU core is that
        /// a slow updating channel need not delay the updating of other channels.
        /// When automatic updating is disabled (threads = 0), <see cref="Update"/> or <see cref="ChannelUpdate"/> should be used instead.
        /// The number of update threads can be changed at any time, including during playback.
        /// <para><b>Platform-specific</b>: The number of update threads is limited to 1 on Windows CE platforms.</para>
        /// </remarks>
        public static int UpdateThreads
        {
            get => GetConfig(Configuration.UpdateThreads);
            set => Configure(Configuration.UpdateThreads, value);
        }

        /// <summary>
        /// The Buffer Length (in bytes) for asynchronous file reading (default setting is 65536 bytes (64KB)).
        /// </summary>
        /// <remarks>
        /// This will be rounded up to the nearest 4096 byte (4KB) boundary.
        /// This determines the amount of file data that can be read ahead of time with asynchronous file reading.
        /// Changes only affect streams that are created afterwards, not any that already exist.
        /// So it is possible to have streams with differing Buffer lengths
        /// by using this config option before creating each of them.
        /// When asynchronous file reading is enabled, the Buffer level is available from <see cref="StreamGetFilePosition"/>.
        /// </remarks>
        public static int AsyncFileBufferLength
        {
            get => GetConfig(Configuration.AsyncFileBufferLength);
            set => Configure(Configuration.AsyncFileBufferLength, value);
        }

        /// <summary>
        /// Gets the total number of HSTREAM/HSAMPLE/HMUSIC/HRECORD handles.
        /// </summary>
        /// <remarks>
        /// The Handle count may not only include the app-created stuff but also internal stuff.
        /// </remarks>
        public static int HandleCount => GetConfig(Configuration.HandleCount);

        /// <summary>
        /// Time (in milliseconds) to wait for a server to respond to a connection request.
        /// The default timeout is 5 seconds (5000 milliseconds).
        /// </summary>
        public static int NetTimeOut
        {
            get => GetConfig(Configuration.NetTimeOut);
            set => Configure(Configuration.NetTimeOut, value);
        }

        /// <summary>
        /// The time (in milliseconds) to wait for a server to deliver more data for an internet stream. (default=0, infinite).
        /// When the timeout is hit, the connection with the server will be closed.
        /// </summary>
        public static int NetReadTimeOut
        {
            get => GetConfig(Configuration.NetReadTimeOut);
            set => Configure(Configuration.NetReadTimeOut, value);
        }

        /// <summary>
        /// The internet download Buffer Length, in milliseconds.
        /// </summary>
        /// <remarks>
        /// Increasing the Buffer Length decreases the chance of the stream stalling,
        /// but also increases the time taken by <see cref="CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)"/>
        /// to create the stream, as it has to pre-Buffer more data (adjustable via the <see cref="NetPreBuffer"/> option).
        /// Aside from the pre-buffering, this setting has no effect on streams without either the <see cref="BassFlags.StreamDownloadBlocks"/>
        /// or <see cref="BassFlags.RestrictDownloadRate"/> flags.
        /// When streaming in blocks, this option determines the download Buffer Length.
        /// The effective Buffer Length can actually be a bit more than that specified,
        /// including data that has been read from the Buffer by the decoder but not yet decoded.
        /// This config option also determines the buffering used by "buffered" User file streams
        /// created with <see cref="CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/>.
        /// The default Buffer Length is 5 seconds (5000 milliseconds).
        /// The net Buffer Length should be larger than the Length of the playback Buffer (<see cref="PlaybackBufferLength"/>),
        /// otherwise the stream is likely to briefly stall soon after starting playback.
        /// Using this config option only affects streams created afterwards, not any that have already been created.
        /// </remarks>
        public static int NetBufferLength
        {
            get => GetConfig(Configuration.NetBufferLength);
            set => Configure(Configuration.NetBufferLength, value);
        }

        /// <summary>
        /// Prevent channels being played when the output is paused? (default = true)
        /// </summary>
        /// <remarks>
        /// When the output is paused using <see cref="Pause"/>, and this config option is enabled,
        /// channels can't be played until the output is resumed using <see cref="Start"/>.
        /// Attempts to play a channel will give a <see cref="Errors.Start"/> error.
        /// </remarks>
        public static int PauseNoPlay
        {
            get => GetConfig(Configuration.PauseNoPlay);
            set => Configure(Configuration.PauseNoPlay, value);
        }

        /// <summary>
        /// Amount (percentage) to pre-Buffer when opening internet streams. (default = 75%)
        /// </summary>
        /// <remarks>
        /// This setting determines what percentage of the Buffer Length (<see cref="NetBufferLength"/>)
        /// should be filled by <see cref="CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)"/>.
        /// Setting this lower (eg. 0) is useful if you want to display a "buffering progress" (using <see cref="StreamGetFilePosition"/>)
        /// when opening internet streams, but note that this setting is just a minimum.
        /// BASS will always pre-download a certain amount to verify the stream.
        /// As well as internet streams, this config setting also applies to "buffered" User file streams
        /// created with <see cref="CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/>.
        /// </remarks>
        public static int NetPreBuffer
        {
            get => GetConfig(Configuration.NetPreBuffer);
            set => Configure(Configuration.NetPreBuffer, value);
        }

        /// <summary>
        /// Use passive mode in FTP connections? (default = true)
        /// Changes take effect from the next internet stream creation call.
        /// </summary>
        public static bool FTPPassive
        {
            get => GetConfigBool(Configuration.NetPassive);
            set => Configure(Configuration.NetPassive, value);
        }

        /// <summary>
        /// Process URLs in PLS, M3U, WPL or ASX playlists?...
        /// 0 = never (Default),
        /// 1 = in <see cref="CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)"/> only,
        /// 2 = in <see cref="CreateStream(string,long,long,BassFlags)"/> and <see cref="CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/> too.
        /// </summary>
        /// <remarks>
        /// When enabled, BASS will process PLS, M3U, WPL and ASX playlists,
        /// going through each entry until it finds a URL that it can play.
        /// </remarks>
        public static int NetPlaylist
        {
            get => GetConfig(Configuration.NetPlaylist);
            set => Configure(Configuration.NetPlaylist, value);
        }

        /// <summary>
        /// The "User-Agent" request header sent to servers.
        /// </summary>
        public static string NetAgent
        {
            get => Marshal.PtrToStringAnsi(GetConfigPtr(Configuration.NetAgent));
            set
            {
                var ptr = Marshal.StringToHGlobalAnsi(value);

                Configure(Configuration.NetAgent, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Proxy server settings (in the form of "User:pass@server:port"... null = don't use a proxy). "" (empty string) = use the OS's default proxy settings.
        /// </summary>
        /// <remarks>
        /// If only the "User:pass@" part is specified, then those authorization credentials are used with the default proxy server.
        /// If only the "server:port" part is specified, then that proxy server is used without any authorization credentials.
        /// Changes take effect from the next internet stream creation call.
        /// </remarks>
        public static string NetProxy
        {
            get => Marshal.PtrToStringAnsi(GetConfigPtr(Configuration.NetProxy));
            set
            {
                var ptr = Marshal.StringToHGlobalAnsi(value);

                Configure(Configuration.NetProxy, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// The maximum number of virtual channels to use in the rendering of IT files... 1 (min) to 512 (max). (default = 64).
        /// </summary>
        /// <remarks>
        /// If the value specified is outside this range, it is automatically capped.
        /// This setting only affects IT files, as the other MOD music formats do not have virtual channels.
        /// Changes only apply to subsequently loaded files, not any that are already loaded.
        /// </remarks>
        public static int MusicVirtial
        {
            get => GetConfig(Configuration.MusicVirtual);
            set => Configure(Configuration.MusicVirtual, value);
        }

        /// <summary>
        /// The amount of data (in bytes) to check in order to verify/detect the file format... 1000 (min) to 100000 (max). (default = 16000 bytes).
        /// </summary>
        /// <remarks>
        /// If the value specified is outside this range, it is automatically capped.
        /// Of the file formats supported as standard, this setting only affects the detection of MP3/MP2/MP1 formats,
        /// but it may also be used by add-ons (see the documentation).
        /// For internet (and "buffered" User file) streams, a quarter of the Length is used, up to a minimum of 1000 bytes.
        /// The verification Length excludes any tags that may be at the start of the file.
        /// For internet (and "buffered" User file) streams, the <see cref="NetVerificationBytes"/> setting determines how much data is checked.
        /// </remarks>
        public static int FileVerificationBytes
        {
            get => GetConfig(Configuration.FileVerificationBytes);
            set => Configure(Configuration.FileVerificationBytes, value);
        }

        /// <summary>
        /// The amount of data to check (in bytes) in order to verify/detect the file format of internet streams... 1000 (min) to 1000000 (max),
        /// or 0 = 25% of the <see cref="FileVerificationBytes"/> setting (with a minimum of 1000 bytes).
        /// </summary>
        /// <remarks>
        /// If the value specified is outside this range, it is automatically capped.
        /// Of the file formats supported as standard, this setting only affects the detection of MP3/MP2/MP1 formats,
        /// but it may also be used by add-ons (see the documentation).
        /// The verification Length excludes any tags that may be found at the start of the file.
        /// The default setting is 0, which means 25% of the <see cref="FileVerificationBytes"/> setting.
        /// As well as internet streams, this config setting also applies to "buffered" User file streams
        /// created with <see cref="CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/>.
        /// </remarks>
        public static int NetVerificationBytes
        {
            get => GetConfig(Configuration.NetVerificationBytes);
            set => Configure(Configuration.NetVerificationBytes, value);
        }

        /// <summary>
        /// The output device Buffer Length in milliseconds.
        /// </summary>
        /// <remarks>
        /// The device Buffer is where the final mix of all playing channels is placed, ready for the device to play.
        /// Its Length affects the latency of things like starting and stopping playback of a channel,
        /// so you will probably want to avoid setting it unnecessarily high,
        /// but setting it too short could result in breaks in the output.
        /// When using a large device Buffer, the <see cref="ChannelAttribute.NoBuffer"/> attribute could be used to skip the channel buffering stage,
        /// to avoid further increasing latency for real-time generated sound and/or DSP/FX changes.
        /// Changes to this config setting only affect subsequently initialized devices, not any that are already initialized.
        /// This config option is only available on Linux, Android and Windows CE.
        /// The device's Buffer is determined automatically on other platforms.
        /// Platform-specific:
        /// On Windows, this config option only applies when WASAPI output is used.
        /// On Linux, the driver may choose to use a different Buffer Length
        /// if it decides that the specified Length is too short or long.
        /// The Buffer Length actually being used can be obtained with <see cref="BassInfo"/>,
        /// like this: <see cref="BassInfo.Latency"/> + <see cref="BassInfo.MinBufferLength"/> / 2.
        /// </remarks>
        public static int DeviceBufferLength
        {
            get => GetConfig(Configuration.DeviceBufferLength);
            set => Configure(Configuration.DeviceBufferLength, value);
        }

        /// <summary>
        /// Suppress silencing for corrupted MP3 frames. (default is false).
        /// </summary>
        /// <remarks>
        /// When BASS is detecting some corruption in an MP3 file's Huffman coding,
        /// it silences the frame to avoid any unpleasent noises that can result from corruption.
        /// Set this parameter to true in order to suppress this behavior.
        /// This applies only to the regular BASS version and NOT the "mp3-free" version.
        /// </remarks>
        public static bool SuppressMP3ErrorCorruptionSilence
        {
            get => GetConfigBool(Configuration.SuppressMP3ErrorCorruptionSilence);
            set => Configure(Configuration.SuppressMP3ErrorCorruptionSilence, value);
        }

        /// <summary>
        /// Gets or Sets the default sample rate conversion quality...
        /// 0 = linear interpolation,
        /// 1 = 8 point sinc interpolation (Default),
        /// 2 = 16 point sinc interpolation,
        /// 3 = 32 point sinc interpolation.
        /// Other values are also accepted.
        /// </summary>
        /// <remarks>
        /// This config option determines what sample rate conversion
        /// quality new channels will initially have, except for sample channels (HCHANNEL),
        /// which use the <see cref="SampleSRCQuality"/> setting.
        /// A channel's sample rate conversion quality can subsequently
        /// be changed via the <see cref="ChannelAttribute.SampleRateConversion"/> attribute (see <see cref="ChannelSetAttribute(int,ChannelAttribute,float)"/>).
        /// </remarks>
        public static int SRCQuality
        {
            get => GetConfig(Configuration.SRCQuality);
            set => Configure(Configuration.SRCQuality, value);
        }

        /// <summary>
        /// Gets or Sets the default sample rate conversion quality for samples...
        /// 0 = linear interpolation (Default),
        /// 1 = 8 point sinc interpolation,
        /// 2 = 16 point sinc interpolation,
        /// 3 = 32 point sinc interpolation.
        /// Other values are also accepted.
        /// </summary>
        /// <remarks>
        /// This config option determines what sample rate conversion quality a new sample
        /// channel will initially have, following a <see cref="SampleGetChannel"/> call.
        /// The channel's sample rate conversion quality can subsequently be changed
        /// via the <see cref="ChannelAttribute.SampleRateConversion"/> attribute (see <see cref="ChannelSetAttribute(int,ChannelAttribute,float)"/>).
        /// </remarks>
        public static int SampleSRCQuality
        {
            get => GetConfig(Configuration.SampleSRCQuality);
            set => Configure(Configuration.SampleSRCQuality, value);
        }

        /// <summary>
        /// Pre-scan chained OGG files? (enabled by default)
        /// </summary>
        /// <remarks>
        /// This option is equivalent to including the <see cref="BassFlags.Prescan"/> flag
        /// in a <see cref="CreateStream(string,long,long,BassFlags)"/> call when opening an OGG file.
        /// It can be disabled if seeking and an accurate Length reading are not required from chained OGG files,
        /// for faster stream creation.
        /// </remarks>
        public static bool OggPreScan
        {
            get => GetConfigBool(Configuration.OggPreScan);
            set => Configure(Configuration.OggPreScan, value);
        }

        /// <summary>
        /// Do not stop the output device when nothing is playing on it?
        /// </summary>
        public static bool DeviceNonStop
        {
            get => GetConfigBool(Configuration.DevNonStop);
            set => Configure(Configuration.DevNonStop, value);
        }
    }
}
