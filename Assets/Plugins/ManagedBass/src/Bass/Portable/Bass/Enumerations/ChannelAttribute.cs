using System;

namespace ManagedBass
{
    /// <summary>
    /// Channel attribute options used by <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" /> and <see cref="Bass.ChannelGetAttribute(int,ChannelAttribute,out float)" />.
    /// </summary>
    public enum ChannelAttribute
    {
        /// <summary>
        /// The sample rate of a channel... 0 = original rate (when the channel was created).
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the BassMix add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// It is not possible to change the sample rate of a channel if the "with FX
        /// flag" DX8 effect implementation enabled on it, unless DirectX 9 or above is installed.
        /// </para>
        /// <para>
        /// Increasing the sample rate of a stream or MOD music increases its CPU usage,
        /// and reduces the Length of its playback Buffer in terms of time.
        /// If you intend to raise the sample rate above the original rate, then you may also need
        /// to increase the Buffer Length via the <see cref="Bass.PlaybackBufferLength"/>
        /// config option to avoid break-ups in the sound.
        /// </para>
        ///
        /// <para><b>Platform-specific</b></para>
        /// <para>On Windows, the sample rate will get rounded down to a whole number during playback.</para>
        /// </summary>
        Frequency = 0x1,

        /// <summary>
        /// The volume level of a channel... 0 (silent) to 1 (full).
        /// <para>This can go above 1.0 on decoding channels.</para>
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the BassMix add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// When using <see cref="Bass.ChannelSlideAttribute"/>
        /// to slide this attribute, a negative volume value can be used to fade-out and then stop the channel.
        /// </para>
        /// </summary>
        Volume = 0x2,

        /// <summary>
        /// The panning/balance position of a channel... -1 (Full Left) to +1 (Full Right), 0 = Centre.
        /// <para>
        /// This attribute applies to playback of the channel, and does not affect the
        /// channel's sample data, so has no real effect on decoding channels.
        /// It is still adjustable though, so that it can be used by the BassMix add-on,
        /// and anything else that wants to use it.
        /// </para>
        /// <para>
        /// It is not possible to set the pan position of a 3D channel.
        /// It is also not possible to set the pan position when using speaker assignment, but if needed,
        /// it can be done via a <see cref="DSPProcedure"/> instead (not on mono channels).
        /// </para>
        ///
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows, this attribute has no effect when speaker assignment is used,
        /// except on Windows Vista and newer with the Bass.VistaSpeakerAssignment config option enabled.
        /// Balance control could be implemented via a <see cref="DSPProcedure"/> instead
        /// </para>
        /// </summary>
        Pan = 0x3,

        /// <summary>
        /// The wet (reverb) / dry (no reverb) mix ratio... 0 (full dry) to 1 (full wet), -1 = automatically calculate the mix based on the distance (the default).
        /// <para>For a sample, stream, or MOD music channel with 3D functionality.</para>
        /// <para>
        /// Obviously, EAX functions have no effect if the output device does not support EAX.
        /// <see cref="Bass.GetInfo"/> can be used to check that.
        /// </para>
        /// <para>
        /// EAX only affects 3D channels, but EAX functions do not require <see cref="Bass.Apply3D"/> to apply the changes.
        /// LastError.NoEAX: The channel does not have EAX support.
        /// EAX only applies to 3D channels that are mixed by the hardware/drivers.
        /// <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)"/> can be used to check if a channel is being mixed by the hardware.
        /// EAX is only supported on Windows.
        /// </para>
        /// </summary>
        EaxMix = 0x4,

        /// <summary>
        /// Non-Windows: Disable playback buffering?... 0 = no, else yes..
        /// <para>
        /// A playing channel is normally asked to render data to its playback Buffer in advance,
        /// via automatic Buffer updates or the <see cref="Bass.Update"/> and <see cref="Bass.ChannelUpdate"/> functions,
        /// ready for mixing with other channels to produce the final mix that is given to the output device.
        /// </para>
        /// <para>
        /// When this attribute is switched on (the default is off), that buffering is skipped and
        /// the channel will only be asked to produce data as it is needed during the generation of the final mix.
        /// This allows the lowest latency to be achieved, but also imposes tighter timing requirements
        /// on the channel to produce its data and apply any DSP/FX (and run mixtime syncs) that are set on it;
        /// if too long is taken, there will be a break in the output, affecting all channels that are playing on the same device.
        /// </para>
        /// <para>
        /// The channel's data is still placed in its playback Buffer when this attribute is on,
        /// which allows <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> and <see cref="Bass.ChannelGetLevel(int)"/> to be used, although there is
        /// likely to be less data available to them due to the Buffer being less full.
        /// </para>
        /// <para>This attribute can be changed mid-playback.</para>
        /// <para>If switched on, any already buffered data will still be played, so that there is no break in sound.</para>
        /// <para>This attribute is not available on Windows, as BASS does not generate the final mix.</para>
        /// </summary>
        NoBuffer = 0x5,

        /// <summary>
        /// The CPU usage of a channel. (in percentage).
        /// <para>
        /// This attribute gives the percentage of CPU that the channel is using,
        /// including the time taken by decoding and DSP processing, and any FX that are
        /// not using the "with FX flag" DX8 effect implementation.
        /// It does not include the time taken to add the channel's data to the final output mix during playback.
        /// The processing of some add-on stream formats may also not be entirely included,
        /// if they use additional decoding threads; see the add-on documentation for details.
        /// </para>
        /// <para>
        /// Like <see cref="Bass.CPUUsage"/>, this function does not strictly tell the CPU usage, but rather how timely the processing is.
        /// For example, if it takes 10ms to generate 100ms of data, that would be 10%.
        /// </para>
        /// <para>
        /// If the reported usage exceeds 100%, that means the channel's data is taking longer to generate than to play.
        /// The duration of the data is based on the channel's current sample rate (<see cref="ChannelAttribute.Frequency"/>).
        /// A channel's CPU usage is updated whenever it generates data.
        /// That could be during a playback Buffer update cycle, or a <see cref="Bass.Update"/> call, or a <see cref="Bass.ChannelUpdate"/> call.
        /// For a decoding channel, it would be in a <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> or <see cref="Bass.ChannelGetLevel(int)"/> call.
        /// </para>
        /// <para>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int, ChannelAttribute, float)"/>.</para>
        /// </summary>
        CPUUsage = 0x7,

        /// <summary>
        /// The sample rate conversion quality of a channel
        /// <para>
        /// 0 = linear interpolation, 1 = 8 point sinc interpolation, 2 = 16 point sinc interpolation, 3 = 32 point sinc interpolation.
        /// Other values are also accepted but will be interpreted as 0 or 3, depending on whether they are lower or higher.
        /// </para>
        /// <para>
        /// When a channel has a different sample rate to what the output device is using,
        /// the channel's sample data will need to be converted to match the output device's rate during playback.
        /// This attribute determines how that is done.
        /// The linear interpolation option uses less CPU, but the sinc interpolation gives better sound quality (less aliasing),
        /// with the quality and CPU usage increasing with the number of points.
        /// A good compromise for lower spec systems could be to use sinc interpolation for music playback and linear interpolation for sound effects.
        /// </para>
        /// <para>
        /// Whenever possible, a channel's sample rate should match the output device's rate to avoid the need for any sample rate conversion.
        /// The device's sample rate could be used in <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />
        /// or <see cref="Bass.MusicLoad(string,long,int,BassFlags,int)" /> or BassMidi stream creation calls, for example.
        /// </para>
        /// <para>
        /// The sample rate conversion occurs (when required) during playback,
        /// after the sample data has left the channel's playback Buffer, so it does not affect the data delivered by <see cref="Bass.ChannelGetData(int,IntPtr,int)" />.
        /// Although this attribute has no direct effect on decoding channels,
        /// it is still available so that it can be used by the BassMix add-on and anything else that wants to use it.
        /// </para>
        /// <para>
        /// This attribute can be set at any time, and changes take immediate effect.
        /// A channel's initial setting is determined by the <see cref="Bass.SRCQuality" /> config option,
        /// or <see cref="Bass.SampleSRCQuality" /> in the case of a sample channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>On Windows, sample rate conversion is handled by Windows or the output device/driver rather than BASS, so this setting has no effect on playback there.</para>
        /// </summary>
        SampleRateConversion = 0x8,

        /// <summary>
        /// The download Buffer level required to resume stalled playback in percent... 0 - 100 (the default is 50%).
        /// <para>
        /// This attribute determines what percentage of the download Buffer (<see cref="Bass.NetBufferLength"/>)
        /// needs to be filled before playback of a stalled internet stream will resume.
        /// It also applies to 'buffered' User file streams created with <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)"/>.
        /// </para>
        /// </summary>
        NetworkResumeBufferLevel = 0x9,

        /// <summary>
        /// The scanned info of a channel.
        /// </summary>
        ScannedInfo = 0xa,

        /// <summary>
        /// Disable playback ramping? 
        /// </summary>
        NoRamp = 0xB,

        /// <summary>
        /// The average bitrate of a file stream. 
        /// </summary>
        Bitrate = 0xC,

        /// <summary>
        /// Playback buffering length.
        /// </summary>
        Buffer = 0xD,

        /// <summary>
        /// Processing granularity. (HMUSIC/HSTREAM/HRECORD)
        /// </summary>
        Granule = 0xE,
        
        #region MOD Music
        /// <summary>
        /// The amplification level of a MOD music... 0 (min) to 100 (max).
        /// <para>This will be rounded down to a whole number.</para>
        /// <para>
        /// As the amplification level get's higher, the sample data's range increases, and therefore, the resolution increases.
        /// But if the level is set too high, then clipping can occur, which can result in distortion of the sound.
        /// You can check the current level of a MOD music at any time by <see cref="Bass.ChannelGetLevel(int)"/>.
        /// By doing so, you can decide if a MOD music's amplification level needs adjusting.
        /// The default amplification level is 50.
        /// </para>
        /// <para>
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicAmplify = 0x100,

        /// <summary>
        /// The pan separation level of a MOD music... 0 (min) to 100 (max), 50 = linear.
        /// <para>
        /// This will be rounded down to a whole number.
        /// By default BASS uses a linear panning "curve".
        /// If you want to use the panning of FT2, use a pan separation setting of around 35.
        /// To use the Amiga panning (ie. full left and right) set it to 100.
        /// </para>
        /// </summary>
        MusicPanSeparation = 0x101,

        /// <summary>
        /// The position scaler of a MOD music... 1 (min) to 256 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// When calling <see cref="Bass.ChannelGetPosition"/>, the row (HIWORD) will be scaled by this value.
        /// By using a higher scaler, you can get a more precise position indication.
        /// The default scaler is 1.
        /// </para>
        /// </summary>
        MusicPositionScaler = 0x102,

        /// <summary>
        /// The BPM of a MOD music... 1 (min) to 255 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's BPM, so the value can be changed via effects in the MOD itself.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicBPM = 0x103,

        /// <summary>
        /// The speed of a MOD music... 0 (min) to 255 (max).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's speed, so the value can be changed via effects in the MOD itself.
        /// The "speed" is the number of ticks per row.
        /// Setting it to 0, stops and ends the music.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicSpeed = 0x104,

        /// <summary>
        /// The global volume level of a MOD music... 0 (min) to 64 (max, 128 for IT format).
        /// <para>
        /// This will be rounded down to a whole number.
        /// This attribute is a direct mapping of the MOD's global volume, so the value can be changed via effects in the MOD itself.
        /// The "speed" is the number of ticks per row.
        /// Setting it to 0, stops and ends the music.
        /// Note that by changing this attribute, you are changing the playback Length.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeGlobal = 0x105,

        /// <summary>
        /// The number of active channels in a MOD music.
        /// <para>
        /// This attribute gives the number of channels (including virtual) that are currently active in the decoder,
        /// which may not match what is being heard during playback due to buffering.
        /// To reduce the time difference, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.
        /// </para>
        /// </summary>
        MusicActiveChannelCount = 0x106,

        /// <summary>
        /// The volume level... 0 (silent) to 1 (full) of a channel in a MOD music + channel#.
        /// <para>channel: The channel to set the volume of... 0 = 1st channel.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// The volume level of all channels is initially 1 (full).
        /// This attribute can also be used to count the number of channels in a MOD Music.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeChannel = 0x200,

        /// <summary>
        /// The volume level... 0 (silent) to 1 (full) of an instrument in a MOD music + inst#.
        /// <para>inst: The instrument to set the volume of... 0 = 1st instrument.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// The volume level of all instruments is initially 1 (full).
        /// For MOD formats that do not use instruments, read "sample" for "instrument".
        /// This attribute can also be used to count the number of instruments in a MOD music.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// </para>
        /// </summary>
        MusicVolumeInstrument = 0x300,
        #endregion

        #region BassFx
        /// <summary>
        /// BassFx Tempo: The Tempo in percents (-95%..0..+5000%).
        /// </summary>
        Tempo = 0x10000,

        /// <summary>
        /// BassFx Tempo: The Pitch in semitones (-60..0..+60).
        /// </summary>
        Pitch = 0x10001,

        /// <summary>
        /// BassFx Tempo: The Samplerate in Hz, but calculates by the same % as <see cref="Tempo"/>.
        /// </summary>
        TempoFrequency = 0x10002,

        /// <summary>
        /// BassFx Tempo Option: Use FIR low-pass (anti-alias) filter (gain speed, lose quality)? true=1 (default), false=0.
        /// <para>See BassFx.TempoCreate for details.</para>
        /// <para>On iOS, Android, WinCE and Linux ARM platforms this is by default disabled for lower CPU usage.</para>
        /// </summary>
        TempoUseAAFilter = 0x10010,

        /// <summary>
        /// BassFx Tempo Option: FIR low-pass (anti-alias) filter Length in taps (8 .. 128 taps, default = 32, should be %4).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoAAFilterLength = 0x10011,

        /// <summary>
        /// BassFx Tempo Option: Use quicker tempo change algorithm (gain speed, lose quality)? true=1, false=0 (default).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoUseQuickAlgorithm = 0x10012,

        /// <summary>
        /// BassFx Tempo Option: Tempo Sequence in milliseconds (default = 82).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoSequenceMilliseconds = 0x10013,

        /// <summary>
        /// BassFx Tempo Option: SeekWindow in milliseconds (default = 14).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoSeekWindowMilliseconds = 0x10014,

        /// <summary>
        /// BassFx Tempo Option: Tempo Overlap in milliseconds (default = 12).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoOverlapMilliseconds = 0x10015,

        /// <summary>
        /// BassFx Tempo Option: Prevents clicks with tempo changes (default = FALSE).
        /// <para>See BassFx.TempoCreate for details.</para>
        /// </summary>
        TempoPreventClick = 0x10016,

        /// <summary>
        /// Playback direction (-1 = Reverse or 1 = Forward).
        /// </summary>
        ReverseDirection = 0x11000,
        #endregion

        #region BassMidi
        /// <summary>
        /// BASSMIDI: Gets the Pulses Per Quarter Note (or ticks per beat) value of the MIDI file.
        /// <para>
        /// This attribute is the number of ticks per beat as defined by the MIDI file;
        /// it will be 0 for MIDI streams created via BassMidi.CreateStream(int,BassFlags,int),
        /// It is read-only, so can't be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.
        /// </para>
        /// </summary>
        MidiPPQN = 0x12000,

        /// <summary>
        /// BASSMIDI: The maximum percentage of CPU time that a MIDI stream can use... 0 to 100, 0 = unlimited.
        /// <para>
        /// It is not strictly the CPU usage that is measured, but rather how timely the stream is able to render data.
        /// For example, a limit of 50% would mean that the rendering would need to be at least 2x real-time speed.
        /// When the limit is exceeded, BassMidi will begin killing voices, starting with the  most quiet.
        /// When the CPU usage is limited, the stream's samples are loaded asynchronously
        /// so that any loading delays (eg. due to slow disk) do not hold up the stream for too long.
        /// If a sample cannot be loaded in time, then it will be silenced
        /// until it is available and the stream will continue playing other samples as normal in the meantime.
        /// This does not affect sample loading via BassMidi.StreamLoadSamples, which always operates synchronously.
        /// By default, a MIDI stream will have no CPU limit.
        /// </para>
        /// </summary>
        MidiCPU = 0x12001,

        /// <summary>
        /// BASSMIDI: The number of MIDI channels in a MIDI stream... 1 (min) - 128 (max).
        /// <para>
        /// For a MIDI file stream, the minimum is 16.
        /// New channels are melodic by default.
        /// Any notes playing on a removed channel are immediately stopped.
        /// </para>
        /// </summary>
        MidiChannels = 0x12002,

        /// <summary>
        /// BASSMIDI: The maximum number of samples to play at a time (polyphony) in a MIDI stream... 1 (min) - 1000 (max).
        /// <para>
        /// If there are currently more voices active than the new limit, then some voices will be killed to meet the limit.
        /// The number of voices currently active is available via the Voices attribute.
        /// A MIDI stream will initially have a default number of voices as determined by the Voices config option.
        /// </para>
        /// </summary>
        MidiVoices = 0x12003,

        /// <summary>
        /// BASSMIDI: The number of samples (voices) currently playing in a MIDI stream.
        /// <para>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)"/>.</para>
        /// </summary>
        MidiVoicesActive = 0x12004,

        /// <summary>
        /// BASSMIDI: The current state of a MIDI stream.
        /// </summary>
        MidiState = 0x12005,

        /// <summary>
        /// BASSMIDI: The sample rate conversion quality of a MIDI stream's samples.
        /// </summary>
        MidiSRC = 0x12006,

        MidiKill = 0x12007,

        /// <summary>
        /// BASSMIDI: The volume level (0.0=silent, 1.0=normal/default) of a track in a MIDI file stream + track#.
        /// <para>track#: The track to set the volume of... 0 = first track.</para>
        /// <para>
        /// The volume curve used by this attribute is always linear, eg. 0.5 = 50%.
        /// The <see cref="Bass.LogarithmicVolumeCurve"/> config option setting has no effect on this.
        /// During playback, the effect of changes to this attribute are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="Bass.PlaybackBufferLength"/> config option to reduce the Buffer Length.
        /// This attribute can also be used to count the number of tracks in a MIDI file stream.
        /// MIDI streams created via BassMidi.CreateStream(int,BassFlags,int) do not have any tracks.
        /// </para>
        /// </summary>
        MidiTrackVolume = 0x12100,
        #endregion

        /// <summary>
        /// BassOpus: The sample rate of an Opus stream's source material.
        /// <para>
        /// Opus streams always have a sample rate of 48000 Hz, and an Opus encoder will resample the source material to that if necessary.
        /// This attribute presents the original sample rate, which may be stored in the Opus file header.
        /// This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />.
        /// </para>
        /// </summary>
        OpusOriginalFrequency = 0x13000,

        /// <summary>
        /// BassDSD: The gain (in decibels) applied when converting to PCM.
        /// </summary>
        /// <remarks>
        /// This attribute is only applicable when converting to PCM, and is unavailable when producing DSD-over-PCM or raw DSD data.
        /// The default setting is determined by the <see cref="DSDGain" /> config option
        /// </remarks>
        DSDGain = 0x14000,

        /// <summary>
        /// BassDSD: The DSD sample rate.
        /// </summary>
        /// <remarks>This attribute is read-only, so cannot be modified via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />.</remarks>
        DSDRate = 0x14001,

        /// <summary>
        /// BassMix: Custom output latency in seconds... default = 0 (no accounting for latency). Changes take immediate effect.
        /// </summary>
        /// <remarks>
        /// When a mixer is played by BASS, the BassMix.ChannelGetData(int,IntPtr,int), BassMix.ChannelGetLevel(int), BassMix.ChannelGetLevel(int,float[],float,LevelRetrievalFlags)", and BassMix.ChannelGetPosition(int,PositionFlags) functions will get the output latency and account for that so that they reflect what is currently being heard, but that cannot be done when a different output system is used, eg. ASIO or WASAPI.
        /// In that case, this attribute can be used to tell the mixer what the output latency is, so that those functions can still account for it.
        /// The mixer needs to have the <see cref="BassFlags.Decode"/> and <see cref="BassFlags.MixerPositionEx"/> flags set to use this attribute. 
        /// </remarks>
        MixerLatency = 0x15000,

        /// <summary>
        /// Amount of data to asynchronously buffer from a splitter's source.
        /// 0 = disable asynchronous buffering. The asynchronous buffering will be limited to the splitter's buffer length.
        /// </summary>
        SplitAsyncBuffer = 0x15010,

        /// <summary>
        /// Maximum amount of data to asynchronously buffer at a time from a splitter's source.
        /// 0 = as much as possible.
        /// </summary>
        SplitAsyncPeriod = 0x15011
    }
}
