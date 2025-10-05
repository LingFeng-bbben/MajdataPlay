using System;

namespace ManagedBass
{
    /// <summary>
    /// Stream/Sample/Music/Recording/AddOn create flags to be used with Stream Creation functions.
    /// </summary>
    [Flags]
    public enum BassFlags : uint
    {
        /// <summary>
        /// 0 = default create stream: 16 Bit, stereo, no Float, hardware mixing, no Loop, no 3D, no speaker assignments...
        /// </summary>
        Default,

        /// <summary>
        /// Use 8-bit resolution. If neither this or the <see cref="Float"/> flags are specified, then the stream is 16-bit.
        /// </summary>
        Byte = 0x1,

        /// <summary>
        /// Decode/play the stream (MP3/MP2/MP1 only) in mono, reducing the CPU usage (if it was originally stereo).
        /// This flag is automatically applied if <see cref="DeviceInitFlags.Mono"/> was specified when calling <see cref="Bass.Init"/>.
        /// </summary>
        Mono = 0x2,

        /// <summary>
        /// Loop the file. This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        Loop = 0x4,

        /// <summary>
        /// Use 3D functionality.
        /// This is ignored if <see cref="DeviceInitFlags.Device3D"/> wasn't specified when calling <see cref="Bass.Init"/>.
        /// 3D streams must be mono (chans=1).
        /// The Speaker flags can not be used together with this flag.
        /// </summary>
        Bass3D = 0x8,

        /// <summary>
        /// Force the stream to not use hardware mixing (Windows Only).
        /// </summary>
        SoftwareMixing = 0x10,

        /// <summary>
        /// Enable the old implementation of DirectX 8 effects (Windows Only).
        /// Use <see cref="Bass.ChannelSetFX"/> to add effects to the stream.
        /// Requires DirectX 8 or above.
        /// </summary>
        FX = 0x80,
        
        /// <summary>
        /// Use 32-bit floating-point sample data (see Floating-Point Channels for details).
        /// WDM drivers or the <see cref="Decode"/> flag are required to use this flag.
        /// </summary>
        Float = 0x100,

        /// <summary>
        /// Enable pin-point accurate seeking (to the exact byte) on the MP3/MP2/MP1 stream or MOD music.
        /// This also increases the time taken to create the stream,
        /// due to the entire file being pre-scanned for the seek points.
        /// Note: This flag is ONLY needed for files with a VBR, files with a CBR are always accurate.
        /// </summary>
        Prescan = 0x20000,

        /// <summary>
        /// Automatically free the music or stream's resources when it has reached the end,
        /// or when <see cref="Bass.ChannelStop"/> or <see cref="Bass.Stop"/> is called.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        AutoFree = 0x40000,

        /// <summary>
        /// Restrict the download rate of the file to the rate required to sustain playback.
        /// If this flag is not used, then the file will be downloaded as quickly as possible.
        /// This flag has no effect on "unbuffered" streams (Buffer=false).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        RestrictDownloadRate = 0x80000,

        /// <summary>
        /// Download and play the file in smaller chunks.
        /// Uses a lot less memory than otherwise,
        /// but it's not possible to seek or loop the stream - once it's ended,
        /// the file must be opened again to play it again.
        /// This flag will automatically be applied when the file Length is unknown.
        /// This flag also has the effect of resticting the download rate.
        /// This flag has no effect on "unbuffered" streams (Buffer=false).
        /// </summary>
        StreamDownloadBlocks = 0x100000,

        /// <summary>
        /// Decode the sample data, without outputting it.
        /// Use <see cref="Bass.ChannelGetData(int,IntPtr,int)"/> to retrieve decoded sample data.
        /// Bass.SoftwareMixing/<see cref="Bass3D"/>/BassFlags.FX/<see cref="AutoFree"/> are all ignored when using this flag, as are the Speaker flags.
        /// </summary>
        Decode = 0x200000,

        /// <summary>
        /// Pass status info (HTTP/ICY tags) from the server to the <see cref="DownloadProcedure"/> callback during connection.
        /// This can be useful to determine the reason for a failure.
        /// </summary>
        StreamStatus = 0x800000,

        /// <summary>
        /// Use an async look-ahead cache.
        /// </summary>
        AsyncFile = 0x40000000,

        /// <summary>
        /// File is a Unicode (16-bit characters) filename
        /// </summary>
        Unicode = 0x80000000,

        #region BassFx
        /// <summary>
        /// BassFx add-on: If in use, then you can do other stuff while detection's in process.
        /// </summary>
        FxBpmBackground = 0x1,

        /// <summary>
        /// BassFx add-on: If in use, then will auto multiply bpm by 2 (if BPM &lt; MinBPM*2)
        /// </summary>
        FXBpmMult2 = 0x2,

        /// <summary>
        /// BassFx add-on (BassFx.TempoCreate): Uses a linear interpolation mode (simple).
        /// </summary>
        FxTempoAlgorithmLinear = 0x200,

        /// <summary>
        /// BassFx add-on (BassFx.TempoCreate): Uses a cubic interpolation mode (recommended, default).
        /// </summary>
        FxTempoAlgorithmCubic = 0x400,

        /// <summary>
        /// BassFx add-on (BassFx.TempoCreate):
        /// Uses a 8-tap band-limited Shannon interpolation (complex, but not much better than cubic).
        /// </summary>
        FxTempoAlgorithmShannon = 0x800,

        /// <summary>
        /// BassFx add-on: Free the source Handle as well?
        /// </summary>
        FxFreeSource = 0x10000,
        #endregion

        #region BassMidi
        /// <summary>
        /// BASSMIDI add-on: Don't send a WAVE header to the encoder.
        /// If this flag is used then the sample format (mono 16-bit)
        /// must be passed to the encoder some other way, eg. via the command-line.
        /// </summary>
        MidiNoHeader = 0x1,

        /// <summary>
        /// BASSMIDI add-on: Reduce 24-bit sample data to 16-bit before encoding.
        /// </summary>
        Midi16Bit = 0x2,

        /// <summary>
        /// BASSMIDI add-on: Ignore system reset events (MidiEventType.System) when the system mode is unchanged.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoSystemReset = 0x800,

        /// <summary>
        /// BASSMIDI add-on: Let the sound decay naturally (including reverb) instead of stopping it abruptly at the end of the file.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/> methods.
        /// </summary>
        MidiDecayEnd = 0x1000,

        /// <summary>
        /// BASSMIDI add-on: Disable the MIDI reverb/chorus processing.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoFx = 0x2000,

        /// <summary>
        /// BASSMIDI add-on: Let the old sound decay naturally (including reverb) when changing the position, including looping.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>, and can also be used in <see cref="Bass.ChannelSetPosition"/>
        /// calls to have it apply to particular position changes.
        /// </summary>
        MidiDecaySeek = 0x4000,

        /// <summary>
        /// BASSMIDI add-on: Do not remove empty space (containing no events) from the end of the file.
        /// </summary>
        MidiNoCrop = 0x8000,

        /// <summary>
        /// BASSMIDI add-on: Only release the oldest instance upon a note off event (MidiEventType.Note with velocity=0)
        /// when there are overlapping instances of the note.
        /// Otherwise all instances are released.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MidiNoteOff1 = 0x10000,

        /// <summary>
        /// BASSMIDI add-on: Map the file into memory.
        /// This flag is ignored if the soundfont is packed as the sample data cannot be played directly from a mapping;
        /// it needs to be decoded.
        /// This flag is also ignored if the file is too large to be mapped into memory.
        /// </summary>
        MidiFontMemoryMap = 0x20000,

        /// <summary>
        /// Use bank 127 in the soundfont for XG drum kits.
        /// When an XG drum kit is needed, bank 127 in soundfonts that have this flag set will be checked first,
        /// before falling back to bank 128 (the standard SF2 drum kit bank) if it is not available there.
        /// </summary>
        MidiFontXGDRUMS = 0x40000,
        #endregion

        /// <summary>
        /// Music and BASSMIDI add-on: Use sinc interpolated sample mixing.
        /// This increases the sound quality, but also requires more CPU.
        /// Otherwise linear interpolation is used.
        /// Music: If neither this or the <see cref="MusicNonInterpolated"/> flag is specified, linear interpolation is used.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        SincInterpolation = 0x800000,

        #region MOD Music
        /// <summary>
        /// Music: Use "normal" ramping (as used in FastTracker 2).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicRamp = 0x200,

        /// <summary>
        /// Music: Use "sensitive" ramping.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSensitiveRamping = 0x400,

        /// <summary>
        /// Music: Apply XMPlay's surround sound to the music (ignored in mono).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSurround = 0x800,

        /// <summary>
        /// Music: Apply XMPlay's surround sound mode 2 to the music (ignored in mono).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicSurround2 = 0x1000,

        /// <summary>
        /// Music: Play .MOD file as FastTracker 2 would.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicFT2Mod = 0x2000,

        /// <summary>
        /// Apply FastTracker 2 panning to XM files.
        /// </summary>
        MusicFT2PAN = 0x2000,

        /// <summary>
        /// Music: Play .MOD file as ProTracker 1 would.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPT1Mod = 0x4000,

        /// <summary>
        /// Music: Stop all notes when seeking (using <see cref="Bass.ChannelSetPosition"/>).
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPositionReset = 0x8000,

        /// <summary>
        /// Music: Use non-interpolated mixing.
        /// This generally reduces the sound quality, but can be good for chip-tunes.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicNonInterpolated = 0x10000,

        /// <summary>
        /// Music: Stop the music when a backward jump effect is played.
        /// This stops musics that never reach the end from going into endless loops.
        /// Some MOD musics are designed to jump all over the place,
        /// so this flag would cause those to be stopped prematurely.
        /// If this flag is used together with the <see cref="Loop"/> flag,
        /// then the music would not be stopped but any <see cref="SyncFlags.End"/> sync would be triggered.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicStopBack = 0x80000,

        /// <summary>
        /// Music: Don't load the samples.
        /// This reduces the time taken to load the music, notably with MO3 files,
        /// which is useful if you just want to get the name and Length of the music without playing it.
        /// </summary>
        MusicNoSample = 0x100000,

        /// <summary>
        /// Music: Stop all notes and reset bpm/etc when seeking.
        /// This flag can be toggled at any time using <see cref="Bass.ChannelFlags"/>.
        /// </summary>
        MusicPositionResetEx = 0x400000,
        #endregion

        #region Sample
        /// <summary>
        /// Sample: muted at max distance (3D only)
        /// </summary>
        MuteMax = 0x20,

        /// <summary>
        /// Sample: uses the DX7 voice allocation and management
        /// </summary>
        VAM = 0x40,

        /// <summary>
        /// Sample: override lowest volume
        /// </summary>
        SampleOverrideLowestVolume = 0x10000,

        /// <summary>
        /// Sample: override longest playing
        /// </summary>
        SampleOverrideLongestPlaying = 0x20000,

        /// <summary>
        /// Sample: override furthest from listener (3D only)
        /// </summary>
        SampleOverrideDistance = 0x30000,

        /// <summary>
        /// Sample: Do not recycle/override one of the sample's existing channels.
        /// This should be used as an argument to <see cref="Bass.SampleGetChannel"/>.
        /// </summary>
        SampleChannelNew = 0x1,

        /// <summary>
        /// Sample: Create a stream rather than a sample channel.
        /// This should be used as an argument to <see cref="Bass.SampleGetChannel"/>.
        /// </summary>
        SampleChannelStream = 0x2,

        #endregion

        #region BassCd
        /// <summary>
        /// BASSCD add-on: Read sub-channel data.
        /// 96 bytes of de-interleaved sub-channel data will be returned after each 2352 bytes of audio.
        /// This flag can not be used with the <see cref="Float"/> flag,
        /// and is ignored if the <see cref="Decode"/> flag is not used.
        /// </summary>
        CDSubChannel = 0x200,

        /// <summary>
        /// BASSCD add-on: Read sub-channel data, without using any hardware de-interleaving.
        /// This is identical to the <see cref="CDSubChannel"/> flag, except that the
        /// de-interleaving is always performed by BASSCD even if the drive is apparently capable of de-interleaving itself.
        /// </summary>
        CDSubchannelNoHW = 0x400,

        /// <summary>
        /// BASSCD add-on: Include C2 error info.
        /// 296 bytes of C2 error info is returned after each 2352 bytes of audio (and optionally 96 bytes of sub-channel data).
        /// This flag cannot be used with the <see cref="Float"/> flag, and is ignored if the <see cref="Decode"/> flag is not used.
        /// The first 294 bytes contain the C2 error bits (one bit for each byte of audio),
        /// followed by a byte containing the logical OR of all 294 bytes,
        /// which can be used to quickly check if there were any C2 errors.
        /// The final byte is just padding.
        /// Note that if you request both sub-channel data and C2 error info, the C2 info will come before the sub-channel data!
        /// </summary>
        CdC2Errors = 0x800,
        #endregion

        #region BassMix

        /// <summary>
        /// Only relevant for StreamAddChannelEx(): Start is an absolute position in the mixer output rather than relative to the mixer's current position. If the position has already passed then the source will start immediately.
        /// </summary>
        MixerChanAbsolute = 0x1000,

        /// <summary>
        /// BASSmix add-on: only read buffered data.
        /// </summary>
        SplitSlave = 0x1000,
        
        /// <summary>
        /// BASSmix add-on: The splitter's length and position is based on the splitter's (rather than the source's) channel count.
        /// </summary>
        SplitPosition = 0x2000,

        /// <summary>
        /// BASSmix add-on: resume a stalled mixer immediately upon new/unpaused source
        /// </summary>
        MixerResume = 0x1000,

        /// <summary>
        /// BASSmix add-on: enable BassMix.ChannelGetPosition(int,PositionFlags,int) support.
        /// </summary>
        MixerPositionEx = 0x2000,

        /// <summary>
        /// BASSmix add-on: Buffer source data for BassMix.ChannelGetData(int,IntPtr,int) and BassMix.ChannelGetLevel(int).
        /// </summary>
        MixerChanBuffer = 0x2000,

        /// <summary>
        /// BASSmix add-on: Buffer source data for BassMix.ChannelGetData(int,IntPtr,int) and BassMix.ChannelGetLevel(int).
        /// </summary>
        [Obsolete("Renamed to MixerChanBuffer for clarity.")]
        MixerBuffer = MixerChanBuffer,

        /// <summary>
        /// BASSmix add-on: Limit mixer processing to the amount available from this source.
        /// </summary>
        MixerChanLimit = 0x4000,

        /// <summary>
        /// BASSmix add-on: Limit mixer processing to the amount available from this source.
        /// </summary>
        [Obsolete("Renamed to MixerChanLimit for clarity.")]
        MixerLimit = MixerChanLimit,

        /// <summary>
        /// BASSmix add-on: end the stream when there are no sources
        /// </summary>
        MixerEnd = 0x10000,

        /// <summary>
        /// BASSmix add-on: Matrix mixing
        /// </summary>
        MixerChanMatrix = 0x10000,

        /// <summary>
        /// BASSmix add-on: Matrix mixing
        /// </summary>
        [Obsolete("Renamed to MixerChanMatrix for clarity.")]
        MixerMatrix = MixerChanMatrix,

        /// <summary>
        /// BASSmix add-on: don't stall when there are no sources
        /// </summary>
        MixerNonStop = 0x20000,

        /// <summary>
        /// BASSmix add-on: don't process the source
        /// </summary>
        MixerChanPause = 0x20000,

        /// <summary>
        /// BASSmix add-on: don't process the source
        /// </summary>
        [Obsolete("Renamed to MixerChanPause for clarity.")]
        MixerPause = MixerChanPause,

        /// <summary>
        /// BASSmix add-on: downmix to stereo (or mono if mixer is mono)
        /// </summary>
        MixerChanDownMix = 0x400000,

        /// <summary>
        /// BASSmix add-on: downmix to stereo (or mono if mixer is mono)
        /// </summary>
        [Obsolete("Renamed to MixerChanDownMix for clarity.")]
        MixerDownMix = MixerChanDownMix,

        /// <summary>
        /// BASSmix add-on: don't ramp-in the start
        /// </summary>
        MixerChanNoRampin = 0x800000,

        /// <summary>
        /// BASSmix add-on: don't ramp-in the start
        /// </summary>
        [Obsolete("Renamed to MixerChanNoRampin for clarity.")]
        MixerNoRampin = MixerChanNoRampin,
        #endregion

        #region Recording
        /// <summary>
        /// Recording: Start the recording paused. Use <see cref="Bass.ChannelPlay"/> to start it.
        /// </summary>
        RecordPause = 0x8000,

        /// <summary>
		/// Recording: Enable Echo Cancellation (only available on certain devices, like iOS).
		/// </summary>
        RecordEchoCancel = 0x2000,

        /// <summary>
		/// Recording: Enable Automatic Gain Control (only available on certain devices, like iOS).
		/// </summary>
        RecordAGC = 0x4000,
        #endregion

        #region Speaker Assignment
        /// <summary>
        /// Front speakers (channel 1/2)
        /// </summary>
        SpeakerFront = 0x1000000,

        /// <summary>
        /// Rear/Side speakers (channel 3/4)
        /// </summary>
        SpeakerRear = 0x2000000,

        /// <summary>
        /// Center and LFE speakers (5.1, channel 5/6)
        /// </summary>
        SpeakerCenterLFE = 0x3000000,

        /// <summary>
        /// Rear Center speakers (7.1, channel 7/8)
        /// </summary>
        SpeakerRearCenter = 0x4000000,

        #region Pairs
        /// <summary>
        /// Speakers Pair 1
        /// </summary>
        SpeakerPair1 = 1 << 24,

        /// <summary>
        /// Speakers Pair 2
        /// </summary>
        SpeakerPair2 = 2 << 24,

        /// <summary>
        /// Speakers Pair 3
        /// </summary>
        SpeakerPair3 = 3 << 24,

        /// <summary>
        /// Speakers Pair 4
        /// </summary>
        SpeakerPair4 = 4 << 24,

        /// <summary>
        /// Speakers Pair 5
        /// </summary>
        SpeakerPair5 = 5 << 24,

        /// <summary>
        /// Speakers Pair 6
        /// </summary>
        SpeakerPair6 = 6 << 24,

        /// <summary>
        /// Speakers Pair 7
        /// </summary>
        SpeakerPair7 = 7 << 24,

        /// <summary>
        /// Speakers Pair 8
        /// </summary>
        SpeakerPair8 = 8 << 24,

        /// <summary>
        /// Speakers Pair 9
        /// </summary>
        SpeakerPair9 = 9 << 24,

        /// <summary>
        /// Speakers Pair 10
        /// </summary>
        SpeakerPair10 = 10 << 24,

        /// <summary>
        /// Speakers Pair 11
        /// </summary>
        SpeakerPair11 = 11 << 24,

        /// <summary>
        /// Speakers Pair 12
        /// </summary>
        SpeakerPair12 = 12 << 24,

        /// <summary>
        /// Speakers Pair 13
        /// </summary>
        SpeakerPair13 = 13 << 24,

        /// <summary>
        /// Speakers Pair 14
        /// </summary>
        SpeakerPair14 = 14 << 24,

        /// <summary>
        /// Speakers Pair 15
        /// </summary>
        SpeakerPair15 = 15 << 24,
        #endregion

        #region Modifiers
        /// <summary>
        /// Speaker Modifier: left channel only
        /// </summary>
        SpeakerLeft = 0x10000000,

        /// <summary>
        /// Speaker Modifier: right channel only
        /// </summary>
        SpeakerRight = 0x20000000,
        #endregion

        /// <summary>
        /// Front Left speaker only (channel 1)
        /// </summary>
        SpeakerFrontLeft = SpeakerFront | SpeakerLeft,

        /// <summary>
        /// Rear/Side Left speaker only (channel 3)
        /// </summary>
        SpeakerRearLeft = SpeakerRear | SpeakerLeft,

        /// <summary>
        /// Center speaker only (5.1, channel 5)
        /// </summary>
        SpeakerCenter = SpeakerCenterLFE | SpeakerLeft,

        /// <summary>
        /// Rear Center Left speaker only (7.1, channel 7)
        /// </summary>
        SpeakerRearCenterLeft = SpeakerRearCenter | SpeakerLeft,

        /// <summary>
        /// Front Right speaker only (channel 2)
        /// </summary>
        SpeakerFrontRight = SpeakerFront | SpeakerRight,

        /// <summary>
        /// Rear/Side Right speaker only (channel 4)
        /// </summary>
        SpeakerRearRight = SpeakerRear | SpeakerRight,

        /// <summary>
        /// LFE speaker only (5.1, channel 6)
        /// </summary>
        SpeakerLFE = SpeakerCenterLFE | SpeakerRight,

        /// <summary>
        /// Rear Center Right speaker only (7.1, channel 8)
        /// </summary>
        SpeakerRearCenterRight = SpeakerRearCenter | SpeakerRight,
        #endregion

        #region BassAac
        /// <summary>
        /// BassAac add-on: use 960 samples per frame
        /// </summary>
        AacFrame960 = 0x1000,

        /// <summary>
        /// BassAac add-on: Downmatrix to Stereo
        /// </summary>
        AacStereo = 0x400000,
        #endregion

        #region BassDSD
        /// <summary>
        /// BassDSD add-on: Produce DSD-over-PCM data (with 0x05/0xFA markers). DSD-over-PCM data is 24-bit, so the <see cref="Float"/> flag is required.
        /// </summary>
        DSDOverPCM = 0x400,

        /// <summary>
        /// BassDSD add-on: Produce raw DSD data instead of PCM. The DSD data is in blocks of 8 bits (1 byte) per-channel with the MSB being first/oldest.
        /// DSD data is not playable by BASS, so the <see cref="Decode"/> flag is required.
        /// </summary>
        DSDRaw = 0x200,
        #endregion

        #region BassAc3
        /// <summary>
        /// BassAC3 add-on: downmix to stereo
        /// </summary>
        Ac3DownmixStereo = 0x200,

        /// <summary>
        /// BASS_AC3 add-on: downmix to quad
        /// </summary>
        Ac3DownmixQuad = 0x400,

        /// <summary>
        /// BASS_AC3 add-on: downmix to dolby
        /// </summary>
        Ac3DownmixDolby = 0x600,

        /// <summary>
        /// BASS_AC3 add-on: enable dynamic range compression
        /// </summary>
        Ac3DRC = 0x800,
        #endregion

        #region BassDShow
        /// <summary>
        /// DSHOW add-on: Use this flag to disable audio processing.
        /// </summary>
        DShowNoAudioProcessing = 0x80000,

        /// <summary>
        /// DSHOW add-on: Use this flag to enable mixing video on a channel.
        /// </summary>
        DShowStreamMix = 0x1000000,

        /// <summary>
        /// DSHOW add-on: Use this flag to enable auto dvd functions(on mouse down, keys etc).
        /// </summary>
        DShowAutoDVD = 0x4000000,

        /// <summary>
        /// DSHOW add-on: Use this flag to restart the stream when it's finished.
        /// </summary>
        DShowLoop = 0x8000000,

        /// <summary>
        /// DSHOW add-on: Use this to enable video processing.
        /// </summary>
        DShowVideoProcessing = 0x20000,
        #endregion

        #region BassLoud
        /// <summary>
        /// BassLoud add-on: Loudness in LUFS of the last 400ms or the duration (in milliseconds) specified in the HIWORD
        /// </summary>
		BassLoudnessCurrent = 0,

        /// <summary>
        /// BassLoud add-on: Integrated loudness in LUFS. This is the average since measurement started.
        /// </summary>
		BassLoudnessIntegrated = 1,

        /// <summary>
        /// BassLoud add-on: Loudness range in LU.
        /// </summary>
		BassLoudnessRange = 2,

        /// <summary>
        /// BassLoud add-on: Peak level in linear scale.
        /// </summary>
		BassLoudnessPeak = 4,

        /// <summary>
        /// BassLoud add-on: True peak level in linear scale.
        /// </summary>
		BassLoudnessTruePeak = 8,

        /// <summary>
        /// BassLoud add-on: Automatically free the measurement when the channel is freed.
        /// </summary>
		BassLoudnessAutofree = 0x8000,
        #endregion

        /// <summary>
        /// BassWV add-on: Limit to stereo
        /// </summary>
        WVStereo = 0x400000
    }
}
