namespace ManagedBass
{
    /// <summary>
    /// Wave Format Encoding
    /// </summary>
    public enum WaveFormatTag : short
    {
        /// <summary>
        /// Wave Format Extensible, Microsoft Corporation
        /// </summary>
        Extensible = -2,

        /// <summary>
        /// Unknown, Microsoft Corporation
        /// </summary>
        Unknown = 0x0000,

        /// <summary>
        /// PCM, Microsoft Corporation
        /// </summary>
        Pcm = 0x0001,

        /// <summary>
        /// ADPCM, Microsoft Corporation
        /// </summary>
        Adpcm = 0x0002,

        /// <summary>
        /// IEEE Float, Microsoft Corporation
        /// </summary>
        IeeeFloat = 0x0003,

        /// <summary>
        /// VSELP, Compaq Computer Corp.
        /// </summary>
        Vselp = 0x0004,

        /// <summary>
        /// IBM CVSD, IBM Corporation
        /// </summary>
        IbmCvsd = 0x0005,

        /// <summary>
        /// ALAW, Microsoft Corporation
        /// </summary>
        ALaw = 0x0006,

        /// <summary>
        /// MULAW, Microsoft Corporation
        /// </summary>
        MuLaw = 0x0007,

        /// <summary>
        /// DTS, Microsoft Corporation
        /// </summary>
        Dts = 0x0008,

        /// <summary>
        /// DRM, Microsoft Corporation
        /// </summary>
        Drm = 0x0009,

        /// <summary>
        /// WMA VOICE 9
        /// </summary>
        WmaVoice9 = 0x000A,

        /// <summary>
        /// OKI ADPCM, OKI
        /// </summary>
        OkiAdpcm = 0x0010,

        /// <summary>
        /// DVI ADPCM, Intel Corporation
        /// </summary>
        DviAdpcm = 0x0011,

        /// <summary>
        /// IMA ADPCM, Intel Corporation
        /// </summary>
        ImaAdpcm = DviAdpcm,

        /// <summary>
        /// MEDIASPACE ADPCM, Videologic
        /// </summary>
        MediaspaceAdpcm = 0x0012,

        /// <summary>
        /// SIERRA ADPCM, Sierra Semiconductor Corp
        /// </summary>
        SierraAdpcm = 0x0013,

        /// <summary>
        /// G723 ADPCM, Antex Electronics Corporation
        /// </summary>
        G723Adpcm = 0x0014,

        /// <summary>
        /// DIGISTD, DSP Solutions, Inc.
        /// </summary>
        DigiStd = 0x0015,

        /// <summary>
        /// DIGIFIX, DSP Solutions, Inc.
        /// </summary>
        DigiFix = 0x0016,

        /// <summary>
        /// DIALOGIC OKI ADPCM, Dialogic Corporation
        /// </summary>
        DialogicOkiAdpcm = 0x0017,

        /// <summary>
        /// MEDIAVISION ADPCM, Media Vision, Inc.
        /// </summary>
        MediaVisionAdpcm = 0x0018,

        /// <summary>
        /// CU CODEC, Hewlett-Packard Company
        /// </summary>
        CUCodec = 0x0019,

        /// <summary>
        /// YAMAHA ADPCM, Yamaha Corporation of America
        /// </summary>
        YamahaAdpcm = 0x0020,

        /// <summary>
        /// SONARC, Speech Compression
        /// </summary>
        SonarC = 0x0021,

        /// <summary>
        /// DSPGROUP TRUESPEECH, DSP Group, Inc
        /// </summary>
        DspGroupTrueSpeech = 0x0022,

        /// <summary>
        /// ECHOSC1, Echo Speech Corporation
        /// </summary>
        EchoSpeechCorporation1 = 0x0023,

        /// <summary>
        /// AUDIOFILE AF36, Virtual Music, Inc.
        /// </summary>
        AudioFileAf36 = 0x0024,

        /// <summary>
        /// APTX, Audio Processing Technology
        /// </summary>
        Aptx = 0x0025,

        /// <summary>
        /// AUDIOFILE AF10, Virtual Music, Inc.
        /// </summary>
        AudioFileAf10 = 0x0026,

        /// <summary>
        /// PROSODY 1612, Aculab plc
        /// </summary>
        Prosody1612 = 0x0027,

        /// <summary>
        /// LRC, Merging Technologies S.A.
        /// </summary>
        Lrc = 0x0028,

        /// <summary>
        /// DOLBY AC2, Dolby Laboratories
        /// </summary>
        DolbyAc2 = 0x0030,

        /// <summary>
        /// GSM610, Microsoft Corporation
        /// </summary>
        Gsm610 = 0x0031,

        /// <summary>
        /// MSNAUDIO, Microsoft Corporation
        /// </summary>
        MsnAudio = 0x0032,

        /// <summary>
        /// ANTEX ADPCME, Antex Electronics Corporation
        /// </summary>
        AntexAdpcme = 0x0033,

        /// <summary>
        /// CONTROL RES VQLPC, Control Resources Limited
        /// </summary>
        ControlResVqlpc = 0x0034,

        /// <summary>
        /// DIGIREAL, DSP Solutions, Inc.
        /// </summary>
        DigiReal = 0x0035,

        /// <summary>
        /// DIGIADPCM, DSP Solutions, Inc.
        /// </summary>
        DigiAdpcm = 0x0036,

        /// <summary>
        /// CONTROL RES CR10, Control Resources Limited
        /// </summary>
        ControlResCr10 = 0x0037,

        /// <summary>
        /// Natural MicroSystems
        /// </summary>
        NMS_VBXADPCM = 0x0038,

        /// <summary>
        /// Crystal Semiconductor IMA ADPCM
        /// </summary>
        CS_IMAADPCM = 0x0039,

        /// <summary>
        /// Echo Speech Corporation
        /// </summary>
        ECHOSC3 = 0x003A,

        /// <summary>
        /// Rockwell International
        /// </summary>
        ROCKWELL_ADPCM = 0x003B,

        /// <summary>
        /// Rockwell International
        /// </summary>
        ROCKWELL_DIGITALK = 0x003C,

        /// <summary>
        /// Xebec Multimedia Solutions Limited
        /// </summary>
        XEBEC = 0x003D,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        G721_ADPCM = 0x0040,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        G728_CELP = 0x0041,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        MSG723 = 0x0042,

        /// <summary>
        /// MPEG, Microsoft Corporation
        /// </summary>
        Mpeg = 0x0050,

        /// <summary>
        /// InSoft, Inc.
        /// </summary>
        RT24 = 0x0052,

        /// <summary>
        /// InSoft, Inc.
        /// </summary>
        PAC = 0x0053,

        /// <summary>
        /// MPEGLAYER3, ISO/MPEG Layer3 Format Tag
        /// </summary>
        Mp3 = 0x0055,

        /// <summary>
        /// Lucent Technologies
        /// </summary>
        LUCENT_G723 = 0x0059,

        /// <summary>
        /// Cirrus Logic
        /// </summary>
        CIRRUS = 0x0060,

        /// <summary>
        /// ESS Technology 
        /// </summary>
        ESPCM = 0x0061,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE = 0x0062,

        /// <summary>
        /// Canopus, co., Ltd.
        /// </summary>
        CANOPUS_ATRAC = 0x0063,

        /// <summary>
        /// APICOM
        /// </summary>
        G726_ADPCM = 0x0064,

        /// <summary>
        /// APICOM
        /// </summary>
        G722_ADPCM = 0x0065,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        DSAT_DISPLAY = 0x0067,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_BYTE_ALIGNED = 0x0069,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_AC8 = 0x0070,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_AC10 = 0x0071,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_AC16 = 0x0072,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_AC20 = 0x0073,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_RT24 = 0x0074,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_RT29 = 0x0075,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_RT29HW = 0x0076,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_VR12 = 0x0077,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_VR18 = 0x0078,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_TQ40 = 0x0079,

        /// <summary>
        /// Softsound, Ltd.
        /// </summary>
        SOFTSOUND = 0x0080,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        VOXWARE_TQ60 = 0x0081,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        MSRT24 = 0x0082,

        /// <summary>
        /// AT&amp;T Labs, Inc.
        /// </summary>
        G729A = 0x0083,

        /// <summary></summary>
        MVI_MVI2 = 0x0084, // Motion Pixels 
        /// <summary></summary>
        DF_G726 = 0x0085, // DataFusion Systems (Pty) (Ltd) 
        /// <summary></summary>
        DF_GSM610 = 0x0086, // DataFusion Systems (Pty) (Ltd) 
        /// <summary></summary>
        ISIAUDIO = 0x0088, // Iterated Systems, Inc. 
        /// <summary></summary>
        ONLIVE = 0x0089, // OnLive! Technologies, Inc. 
        /// <summary></summary>
        SBC24 = 0x0091, // Siemens Business Communications Sys 
        /// <summary></summary>
        DOLBY_AC3_SPDIF = 0x0092, // Sonic Foundry 
        /// <summary></summary>
        MEDIASONIC_G723 = 0x0093, // MediaSonic 
        /// <summary></summary>
        PROSODY_8KBPS = 0x0094, // Aculab plc 
        /// <summary></summary>
        ZYXEL_ADPCM = 0x0097, // ZyXEL Communications, Inc. 
        /// <summary></summary>
        PHILIPS_LPCBB = 0x0098, // Philips Speech Processing 
        /// <summary></summary>
        PACKED = 0x0099, // Studer Professional Audio AG 
        /// <summary></summary>
        MALDEN_PHONYTALK = 0x00A0, // Malden Electronics Ltd. 

        /// <summary>
        /// WAVE_FORMAT_GSM
        /// </summary>
        Gsm = 0x00A1,

        /// <summary>
        /// WAVE_FORMAT_G729
        /// </summary>
        G729 = 0x00A2,

        /// <summary>
        /// WAVE_FORMAT_G723
        /// </summary>
        G723 = 0x00A3,

        /// <summary>
        /// WAVE_FORMAT_ACELP
        /// </summary>
        Acelp = 0x00A4,

        /// <summary>
        /// WAVE_FORMAT_RAW_AAC1
        /// </summary>
        RawAac = 0x00FF,
        /// <summary></summary>
        RHETOREX_ADPCM = 0x0100, // Rhetorex Inc. 
        /// <summary></summary>
        IRAT = 0x0101, // BeCubed Software Inc. 
        /// <summary></summary>
        VIVO_G723 = 0x0111, // Vivo Software 
        /// <summary></summary>
        VIVO_SIREN = 0x0112, // Vivo Software 
        /// <summary></summary>
        DIGITAL_G723 = 0x0123, // Digital Equipment Corporation 
        /// <summary></summary>
        SANYO_LD_ADPCM = 0x0125, // Sanyo Electric Co., Ltd. 
        /// <summary></summary>
        SIPROLAB_ACEPLNET = 0x0130, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        SIPROLAB_ACELP4800 = 0x0131, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        SIPROLAB_ACELP8V3 = 0x0132, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        SIPROLAB_G729 = 0x0133, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        SIPROLAB_G729A = 0x0134, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        SIPROLAB_KELVIN = 0x0135, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        G726ADPCM = 0x0140, // Dictaphone Corporation 
        /// <summary></summary>
        QUALCOMM_PUREVOICE = 0x0150, // Qualcomm, Inc. 
        /// <summary></summary>
        QUALCOMM_HALFRATE = 0x0151, // Qualcomm, Inc. 
        /// <summary></summary>
        TUBGSM = 0x0155, // Ring Zero Systems, Inc. 
        /// <summary></summary>
        MSAUDIO1 = 0x0160, // Microsoft Corporation

        /// <summary>
        /// Windows Media Audio, WAVE_FORMAT_WMAUDIO2, Microsoft Corporation
        /// </summary>
        WMA = 0x0161,

        /// <summary>
        /// Windows Media Audio Professional WAVE_FORMAT_WMAUDIO3, Microsoft Corporation
        /// </summary>
        WMAProfessional = 0x0162,

        /// <summary>
        /// Windows Media Audio Lossless, WAVE_FORMAT_WMAUDIO_LOSSLESS
        /// </summary>
        WMALosseless = 0x0163,

        /// <summary>
        /// Windows Media Audio Professional over SPDIF WAVE_FORMAT_WMASPDIF (0x0164)
        /// </summary>
        WMA_SPDIF = 0x0164,

        /// <summary></summary>
        UNISYS_NAP_ADPCM = 0x0170, // Unisys Corp. 
        /// <summary></summary>
        UNISYS_NAP_ULAW = 0x0171, // Unisys Corp. 
        /// <summary></summary>
        UNISYS_NAP_ALAW = 0x0172, // Unisys Corp. 
        /// <summary></summary>
        UNISYS_NAP_16K = 0x0173, // Unisys Corp. 
        /// <summary></summary>
        CREATIVE_ADPCM = 0x0200, // Creative Labs, Inc 
        /// <summary></summary>
        CREATIVE_FASTSPEECH8 = 0x0202, // Creative Labs, Inc 
        /// <summary></summary>
        CREATIVE_FASTSPEECH10 = 0x0203, // Creative Labs, Inc 
        /// <summary></summary>
        UHER_ADPCM = 0x0210, // UHER informatic GmbH 
        /// <summary></summary>
        QUARTERDECK = 0x0220, // Quarterdeck Corporation 
        /// <summary></summary>
        ILINK_VC = 0x0230, // I-link Worldwide 
        /// <summary></summary>
        RAW_SPORT = 0x0240, // Aureal Semiconductor 
        /// <summary></summary>
        ESST_AC3 = 0x0241, // ESS Technology, Inc. 
        /// <summary></summary>
        IPI_HSX = 0x0250, // Interactive Products, Inc. 
        /// <summary></summary>
        IPI_RPELP = 0x0251, // Interactive Products, Inc. 
        /// <summary></summary>
        CS2 = 0x0260, // Consistent Software 
        /// <summary></summary>
        SONY_SCX = 0x0270, // Sony Corp. 
        /// <summary></summary>
        FM_TOWNS_SND = 0x0300, // Fujitsu Corp. 
        /// <summary></summary>
        BTV_DIGITAL = 0x0400, // Brooktree Corporation 
        /// <summary></summary>
        QDESIGN_MUSIC = 0x0450, // QDesign Corporation 
        /// <summary></summary>
        VME_VMPCM = 0x0680, // AT&T Labs, Inc. 
        /// <summary></summary>
        TPC = 0x0681, // AT&T Labs, Inc. 
        /// <summary></summary>
        OLIGSM = 0x1000, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        OLIADPCM = 0x1001, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        OLICELP = 0x1002, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        OLISBC = 0x1003, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        OLIOPR = 0x1004, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        LH_CODEC = 0x1100, // Lernout & Hauspie 
        /// <summary></summary>
        NORRIS = 0x1400, // Norris Communications, Inc. 
        /// <summary></summary>
        SOUNDSPACE_MUSICOMPRESS = 0x1500, // AT&T Labs, Inc. 

        /// <summary>
        /// Advanced Audio Coding (AAC) audio in Audio Data Transport Stream (ADTS) format.
        /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_ADTS_AAC.
        /// </summary>
        /// <remarks>
        /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
        /// prior to applying spectral band replication (SBR) or parametric stereo (PS) tools, if present.
        /// No additional data is required after the WAVEFORMATEX structure.
        /// </remarks>
        MPEG_ADTS_AAC = 0x1600,

        /// <summary></summary>
        MPEG_RAW_AAC = 0x1601,

        /// <summary>
        /// MPEG-4 audio transport stream with a synchronization layer (LOAS) and a multiplex layer (LATM).
        /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_LOAS.
        /// </summary>
        /// <remarks>
        /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
        /// prior to applying spectral SBR or PS tools, if present.
        /// No additional data is required after the WAVEFORMATEX structure.
        /// </remarks>
        MPEG_LOAS = 0x1602,

        /// <summary>NOKIA_MPEG_ADTS_AAC</summary>
        NOKIA_MPEG_ADTS_AAC = 0x1608,

        /// <summary>NOKIA_MPEG_RAW_AAC</summary>
        NOKIA_MPEG_RAW_AAC = 0x1609,

        /// <summary>VODAFONE_MPEG_ADTS_AAC</summary>
        VODAFONE_MPEG_ADTS_AAC = 0x160A,

        /// <summary>VODAFONE_MPEG_RAW_AAC</summary>
        VODAFONE_MPEG_RAW_AAC = 0x160B,

        /// <summary>
        /// High-Efficiency Advanced Audio Coding (HE-AAC) stream.
        /// The format block is an HEAACWAVEFORMAT structure.
        /// </summary>
        MPEG_HEAAC = 0x1610,

        /// <summary>
        /// WAVE_FORMAT_DVM
        /// </summary>
        DVM = 0x2000, // FAST Multimedia AG 

        // others - not from MS headers
        /// <summary>
        /// WAVE_FORMAT_VORBIS1 "Og" Original stream compatible
        /// </summary>
        Vorbis1 = 0x674f,

        /// <summary>
        /// WAVE_FORMAT_VORBIS2 "Pg" Have independent header
        /// </summary>
        Vorbis2 = 0x6750,

        /// <summary>
        /// WAVE_FORMAT_VORBIS3 "Qg" Have no codebook header
        /// </summary>
        Vorbis3 = 0x6751,

        /// <summary>
        /// WAVE_FORMAT_VORBIS1P "og" Original stream compatible
        /// </summary>
        Vorbis1P = 0x676f,

        /// <summary>
        /// WAVE_FORMAT_VORBIS2P "pg" Have independent headere
        /// </summary>
        Vorbis2P = 0x6770,

        /// <summary>
        /// WAVE_FORMAT_VORBIS3P "qg" Have no codebook header
        /// </summary>
        Vorbis3P = 0x6771
    }
}