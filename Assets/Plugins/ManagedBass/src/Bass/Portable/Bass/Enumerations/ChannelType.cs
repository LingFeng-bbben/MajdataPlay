using System;

namespace ManagedBass
{
    /// <summary>
    /// Channel Type flags to be used with <see cref="ChannelInfo" /> (see also <see cref="Bass.ChannelGetInfo(int,out ChannelInfo)" />).
    /// </summary>
    [Flags]
    public enum ChannelType
    {
        /// <summary>
        /// Unknown channel format.
        /// </summary>
        Unknown,

        /// <summary>
        /// Sample channel. (HCHANNEL)
        /// </summary>
        Sample = 0x1,

        /// <summary>
        /// Recording channel. (HRECORD)
        /// </summary>
        Recording = 0x2,

        /// <summary>
        /// MO3 format music.
        /// </summary>
        MO3 = 0x100,

        /// <summary>
        /// ZXTune.
        /// </summary>
        ZXTune = unchecked((int)0xCF1D0000),

        #region HStream
        /// <summary>
        /// User sample stream.
        /// This can also be used as a flag to test if the channel is any kind of HSTREAM.
        /// </summary>
        Stream = 0x10000,

        /// <summary>
        /// OGG format stream.
        /// </summary>
        OGG = 0x10002,

        /// <summary>
        /// MP1 format stream.
        /// </summary>
        MP1 = 0x10003,

        /// <summary>
        /// MP2 format stream.
        /// </summary>
        MP2 = 0x10004,

        /// <summary>
        /// MP3 format stream.
        /// </summary>
        MP3 = 0x10005,

        /// <summary>
        /// WAV format stream.
        /// </summary>
        AIFF = 0x10006,

        /// <summary>
        /// CoreAudio codec stream. Additional information is avaliable via the <see cref="TagType.CoreAudioCodec"/> tag (iOS and Mac).
        /// </summary>
        CA = 0x10007,

        /// <summary>
        /// Media Foundation codec stream. Additional information is avaliable via the <see cref="TagType.MF"/> tag.
        /// </summary>
        MF = 0x10008,

        /// <summary>
        /// Audio-CD, CDA
        /// </summary>
        CD = 0x10200,

        /// <summary>
        /// WMA format stream.
        /// </summary>
        WMA = 0x10300,

        /// <summary>
        /// MP3 over WMA format stream.
        /// </summary>
        WMA_MP3 = 0x10301,

        /// <summary>
        /// Winamp Input format stream.
        /// </summary>
        WINAMP = 0x10400,

        /// <summary>
        /// WavPack Lossless format stream.
        /// </summary>
        WV = 0x10500,

        /// <summary>
        /// WavPack Hybrid Lossless format stream.
        /// </summary>
        WV_H = 0x10501,

        /// <summary>
        /// WavPack Lossy format stream.
        /// </summary>
        WV_L = 0x10502,

        /// <summary>
        /// WavPack Hybrid Lossy format stream.
        /// </summary>
        WV_LH = 0x10503,

        /// <summary>
        /// Optimfrog format stream.
        /// </summary>
        OFR = 0x10600,

        /// <summary>
        /// APE format stream.
        /// </summary>
        APE = 0x10700,

        /// <summary>
        /// BassMix mixer stream.
        /// </summary>
        Mixer = 0x10800,

        /// <summary>
        /// BassMix splitter stream.
        /// </summary>
        Split = 0x10801,

        /// <summary>
        /// FLAC format stream.
        /// </summary>
        FLAC = 0x10900,

        /// <summary>
        /// FLAC OGG format stream.
        /// </summary>
        FLAC_OGG = 0x10901,

        /// <summary>
        /// MPC format stream.
        /// </summary>
        MPC = 0x10a00,

        /// <summary>
        /// AAC format stream.
        /// </summary>
        AAC = 0x10b00,

        /// <summary>
        /// MP4 format stream.
        /// </summary>
        MP4 = 0x10b01,

        /// <summary>
        /// Speex format stream.
        /// </summary>
        SPX = 0x10c00,

        /// <summary>
        /// MIDI sound format stream.
        /// </summary>
        MIDI = 0x10d00,

        /// <summary>
        /// Apple Lossless (ALAC) format stream.
        /// </summary>
        ALAC = 0x10e00,

        /// <summary>
        /// TTA format stream.
        /// </summary>
        TTA = 0x10f00,

        /// <summary>
        /// AC3 format stream.
        /// </summary>
        AC3 = 0x11000,

        /// <summary>
        /// Video format stream.
        /// </summary>
        Video = 0x11100,

        /// <summary>
        /// Opus format stream.
        /// </summary>
        OPUS = 0x11200,

        /// <summary>
        /// Direct Stream Digital (DSD) format stream.
        /// </summary>
        DSD = 0x11700,

        /// <summary>
        /// ADX format stream.
        /// <para>
        /// ADX is a lossy proprietary audio storage and compression format developed
        /// by CRI Middleware specifically for use in video games, it is derived from ADPCM.
        /// </para>
        /// </summary>
        ADX = 0x1f000,

        /// <summary>
        /// AIX format stream.
        /// Only for ADX of all versions (with AIXP support).
        /// </summary>
        AIX = 0x1f001,

        /// <summary>
        /// BassFx tempo stream.
        /// </summary>
        Tempo = 0x1f200,

        /// <summary>
        /// BassFx reverse stream.
        /// </summary>
        Reverse = 0x1f201,
        #endregion

        #region HMusic
        /// <summary>
        /// MOD format music.
        /// This can also be used as a flag to test if the channel is any kind of HMusic.
        /// </summary>
        MOD = 0x20000,

        /// <summary>
        /// MTM format music.
        /// </summary>
        MTM = 0x20001,

        /// <summary>
        /// S3M format music.
        /// </summary>
        S3M = 0x20002,

        /// <summary>
        /// XM format music.
        /// </summary>
        XM = 0x20003,

        /// <summary>
        /// IT format music.
        /// </summary>
        IT = 0x20004,
        #endregion

        /// <summary>
        /// Wave format stream, LoWord = codec.
        /// </summary>
        Wave = 0x40000,

        /// <summary>
        /// Wave format stream, PCM 16-bit.
        /// </summary>
        WavePCM = 0x50001,

        /// <summary>
        /// Wave format stream, Float 32-bit.
        /// </summary>
        WaveFloat = 0x50003,

        /// <summary>
        /// Dummy stream.
        /// </summary>
        Dummy = 0x18000,

        /// <summary>
        /// Device mix stream.
        /// </summary>
        Device = 0x18001
    }
}
