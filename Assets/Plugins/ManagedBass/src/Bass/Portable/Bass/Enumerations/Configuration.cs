namespace ManagedBass
{
    /// <summary>
    /// Bass Configuration Values.
    /// </summary>
    public enum Configuration
    {
        /// <summary>
        /// See <see cref="Bass.PlaybackBufferLength"/>.
        /// </summary>
        PlaybackBufferLength = 0,

        /// <summary>
        /// See <see cref="Bass.UpdatePeriod"/>.
        /// </summary>
        UpdatePeriod = 1,

        /// <summary>
        /// See <see cref="Bass.GlobalSampleVolume"/>.
        /// </summary>
        GlobalSampleVolume = 4,

        /// <summary>
        /// See <see cref="Bass.GlobalStreamVolume"/>.
        /// </summary>
        GlobalStreamVolume = 5,

        /// <summary>
        /// See <see cref="Bass.GlobalMusicVolume"/>.
        /// </summary>
        GlobalMusicVolume = 6,

        /// <summary>
        /// See <see cref="Bass.LogarithmicVolumeCurve"/>.
        /// </summary>
        LogarithmicVolumeCurve = 7,

        /// <summary>
        /// See <see cref="Bass.LogarithmicPanningCurve"/>.
        /// </summary>
        LogarithmicPanCurve = 8,

        /// <summary>
        /// See <see cref="Bass.FloatingPointDSP"/>.
        /// </summary>
        FloatDSP = 9,

        /// <summary>
        /// See <see cref="Bass.Algorithm3D"/>.
        /// </summary>
        Algorithm3D = 10,

        /// <summary>
        /// See <see cref="Bass.NetTimeOut"/>.
        /// </summary>
        NetTimeOut = 11,

        /// <summary>
        /// See <see cref="Bass.NetBufferLength"/>.
        /// </summary>
        NetBufferLength = 12,

        /// <summary>
        /// See <see cref="Bass.PauseNoPlay"/>.
        /// </summary>
        PauseNoPlay = 13,

        /// <summary>
        /// See <see cref="Bass.NetPreBuffer"/>.
        /// </summary>
        NetPreBuffer = 15,

        /// <summary>
        /// See <see cref="Bass.NetAgent"/>.
        /// </summary>
        NetAgent = 16,

        /// <summary>
        /// See <see cref="Bass.NetProxy"/>.
        /// </summary>
        NetProxy = 17,

        /// <summary>
        /// See <see cref="Bass.FTPPassive"/>.
        /// </summary>
        NetPassive = 18,

        /// <summary>
        /// See <see cref="Bass.RecordingBufferLength"/>.
        /// </summary>
        RecordingBufferLength = 19,

        /// <summary>
        /// See <see cref="Bass.NetPlaylist"/>.
        /// </summary>
        NetPlaylist = 21,

        /// <summary>
        /// See <see cref="Bass.MusicVirtial"/>.
        /// </summary>
        MusicVirtual = 22,

        /// <summary>
        /// See <see cref="Bass.FileVerificationBytes"/>.
        /// </summary>
        FileVerificationBytes = 23,

        /// <summary>
        /// See <see cref="Bass.UpdateThreads"/>.
        /// </summary>
        UpdateThreads = 24,

        /// <summary>
        /// See <see cref="Bass.DeviceBufferLength"/>.
        /// </summary>
        DeviceBufferLength = 27,

        /// <summary>
        /// Enable the loopback recording feature.
        /// </summary>
        LoopbackRecording = 28,

        /// <summary>
        /// See <see cref="Bass.NoTimerResolution"/>.
        /// </summary>
        NoTimerResolution = 29,

        /// <summary>
        /// See <see cref="Bass.VistaTruePlayPosition"/>.
        /// </summary>
        TruePlayPosition = 30,

        /// <summary>
        /// See <see cref="Bass.IOSMixAudio"/>.
        /// </summary>
        IOSMixAudio = 34,

        /// <summary>
        /// Audio session configuration on iOS
        /// </summary>
        IOSSession = 34,

        /// <summary>
        /// See <see cref="Bass.SuppressMP3ErrorCorruptionSilence"/>.
        /// </summary>
        SuppressMP3ErrorCorruptionSilence = 35,

        /// <summary>
        /// See <see cref="Bass.IncludeDefaultDevice"/>.
        /// </summary>
        IncludeDefaultDevice = 36,

        /// <summary>
        /// See <see cref="Bass.NetReadTimeOut"/>.
        /// </summary>
        NetReadTimeOut = 37,

        /// <summary>
        /// See <see cref="Bass.VistaSpeakerAssignment"/>.
        /// </summary>
        VistaSpeakerAssignment = 38,

        /// <summary>
        /// See <see cref="Bass.IOSSpeaker"/>.
        /// </summary>
        IOSSpeaker = 39,

        /// <summary>
        /// MF Disable.
        /// </summary>
        MFDisable = 40,

        /// <summary>
        /// See <see cref="Bass.HandleCount"/>.
        /// </summary>
        HandleCount = 41,

        /// <summary>
        /// See <see cref="Bass.UnicodeDeviceInformation"/>.
        /// </summary>
        UnicodeDeviceInformation = 42,

        /// <summary>
        /// See <see cref="Bass.SRCQuality"/>.
        /// </summary>
        SRCQuality = 43,

        /// <summary>
        /// See <see cref="Bass.SampleSRCQuality"/>.
        /// </summary>
        SampleSRCQuality = 44,

        /// <summary>
        /// See <see cref="Bass.AsyncFileBufferLength"/>.
        /// </summary>
        AsyncFileBufferLength = 45,

        /// <summary>
        /// See <see cref="Bass.IOSNotification"/>.
        /// </summary>
        IOSNotify = 46,

        /// <summary>
        /// See <see cref="Bass.OggPreScan"/>.
        /// </summary>
        OggPreScan = 47,

        /// <summary>
        /// See <see cref="Bass.MFVideo"/>.
        /// </summary>
        MFVideo = 48,

        /// <summary>
        /// See <see cref="Bass.EnableAirplayReceivers"/>.
        /// </summary>
        Airplay = 49,

        /// <summary>
        /// See <see cref="Bass.DeviceNonStop"/>.
        /// </summary>
        DevNonStop = 50,

        /// <summary>
        /// See <see cref="Bass.IOSNoCategory"/>.
        /// </summary>
        IOSNoCategory = 51,

        /// <summary>
        /// See <see cref="Bass.NetVerificationBytes"/>.
        /// </summary>
        NetVerificationBytes = 52,

        /// <summary>
        /// Device Period.
        /// </summary>
        DevicePeriod = 53,

        /// <summary>
        /// See <see cref="Bass.Float"/>.
        /// </summary>
        Float = 54,

        /// <summary>
        /// Gets the Android AAudio session ID
        /// </summary>
        AndroidSessionId = 62,

        /// <summary>
        /// Enable AAudio output
        /// </summary>
        AndroidAAudio =	67,

        /// <summary>
        /// See BassAc3.DRC.
        /// </summary>
        AC3DynamicRangeCompression = 65537,

        /// <summary>
        /// See BassWma.PrebufferInternetStreams.
        /// </summary>
        WmaNetPreBuffer = 65793,

        /// <summary>
        /// See BassWma.UseBassFileHandling.
        /// </summary>
        WmaBassFileHandling = 65795,

        /// <summary>
        /// See BassWma.CanSeekNetworkStreams.
        /// </summary>
        WmaNetSeek = 65796,

        /// <summary>
        /// See BassWma.PlayWMVAudio.
        /// </summary>
        WmaVideo = 65797,

        /// <summary>
        /// See BassWma.AsyncDecoding.
        /// </summary>
        WmaAsync = 65807,

        /// <summary>
        /// See BassCd.FreeOld.
        /// </summary>
        CDFreeOld = 66048,

        /// <summary>
        /// See BassCd.RetryCount.
        /// </summary>
        CDRetry = 66049,

        /// <summary>
        /// See BassCd.AutoSpeedReduction.
        /// </summary>
        CDAutoSpeed = 66050,

        /// <summary>
        /// See BassCd.SkipError.
        /// </summary>
        CDSkipError = 66051,

        /// <summary>
        /// See BassCd.CDDBServer.
        /// </summary>
        CDDBServer = 66052,

        /// <summary>
        /// See BassEnc.DSPPriority.
        /// </summary>
        EncodePriority = 66304,

        /// <summary>
        /// See BassEnc.Queue.
        /// </summary>
        EncodeQueue = 66305,

        /// <summary>
        /// See BassEnc.ACMLoad.
        /// </summary>
        EncodeACMLoad = 66306,

        /// <summary>
        /// See BassEnc.CastTimeout.
        /// </summary>
        EncodeCastTimeout = 66320,

        /// <summary>
        /// See BassEnc.CastProxy.
        /// </summary>
        EncodeCastProxy = 66321,

        /// <summary>
        /// See BassMidi.Compact.
        /// </summary>
        MidiCompact = 66560,

        /// <summary>
        /// See BassMidi.Voices.
        /// </summary>
        MidiVoices = 66561,

        /// <summary>
        /// See BassMidi.AutoFont.
        /// </summary>
        MidiAutoFont = 66562,

        /// <summary>
        /// See BassMidi.DefaultFont.
        /// </summary>
        MidiDefaultFont = 66563,

        /// <summary>
        /// See BassMidi.InputPorts.
        /// </summary>
        MidiInputPorts = 66564,

        /// <summary>
        /// See MixerBufferLength.
        /// </summary>
        MixerBufferLength = 67073,

        /// <summary>
        /// See MixerPositionEx.
        /// </summary>
        MixerPositionEx = 67074,

        /// <summary>
        /// See SplitBufferLength.
        /// </summary>
        SplitBufferLength = 67088,

        /// <summary>
        /// See BassAac.PlayAudioFromMp4.
        /// </summary>
        PlayAudioFromMp4 = 67328,

        /// <summary>
        /// See BassAac.AacSupportMp4.
        /// </summary>
        AacSupportMp4 = 67329,

        /// <summary>
        /// See BassDsd.DefaultFrequency.
        /// </summary>
        DSDFrequency = 67584,

        /// <summary>
        /// See BassWinamp.InputTimeout.
        /// </summary>
        WinampInputTimeout = 67584,

        /// <summary>
        /// See BassDsd.DefaultGain.
        /// </summary>
        DSDGain = 67585,

        /// <summary>
        /// See BassZXTune.MaxFileSize.
        /// </summary>
        ZXTuneMaxFileSize = unchecked((int)0xCF1D0100)
    }
}
