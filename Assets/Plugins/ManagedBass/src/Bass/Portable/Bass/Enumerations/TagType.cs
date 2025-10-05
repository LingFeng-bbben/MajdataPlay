namespace ManagedBass
{
    /// <summary>
    /// Types of what's returned by <see cref="Bass.ChannelGetTags" />.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// ID3v1 tags : A pointer to a 128 byte block is returned.
        /// See www.id3.org for details of the block's structure.
        /// </summary>
        ID3 = 0,

        /// <summary>
        /// ID3v2 tags : A pointer to a variable Length block is returned.
        /// See www.id3.org for details of the block's structure.
        /// </summary>
        ID3v2 = 1,

        /// <summary>
        /// OGG comments : Only available when streaming an OGG file.
        /// A pointer to a series of null-terminated UTF-8 strings is returned, the final string ending with a double null.
        /// </summary>
        OGG = 2,

        /// <summary>
        /// HTTP headers : Only available when streaming from a HTTP server.
        /// A pointer to a series of null-terminated ANSI strings is returned, the final string ending with a double null.
        /// </summary>
        HTTP = 3,

        /// <summary>
        /// ICY headers : A pointer to a series of null-terminated ANSI strings is returned,
        /// the final string ending with a double null.
        /// </summary>
        ICY = 4,

        /// <summary>
        /// ICY (Shoutcast) metadata : A single null-terminated ANSI string containing
        /// the current stream title and url (usually omitted).
        /// The format of the string is: StreamTitle='xxx';StreamUrl='xxx';
        /// </summary>
        META = 5,

        /// <summary>
        /// APE (v1 or v2) tags : Only available when streaming an APE file.
        /// A pointer to a series of null-terminated UTF-8 strings is returned,
        /// the final string ending with a double null.
        /// </summary>
        APE = 6,

        /// <summary>
        /// iTunes/MP4 metadata : Only available when streaming a MP4 file.
        /// A pointer to a series of null-terminated UTF-8 strings is returned, the final string ending with a double null.
        /// </summary>
        MP4 = 7,

        /// <summary>
        /// WMA header tags: WMA tags : Only available when streaming a WMA file.
        /// A pointer to a series of null-terminated UTF-8 strings is returned, the final string ending with a double null.
        /// </summary>
        WMA = 8,

        /// <summary>
        /// OGG encoder : A single null-terminated UTF-8 string.
        /// </summary>
        OggEncoder = 9,

        /// <summary>
        /// Lyric3v2 tag : A single ANSI string is returned, containing the Lyrics3v2 information.
        /// See www.id3.org/Lyrics3v2 for details of its format.
        /// </summary>
        Lyrics3v2 = 10,

        /// <summary>
        /// WMA mid-stream tag: a single UTF-8 string.
        /// </summary>
        WmaMeta = 11,

        /// <summary>
        /// Apple CoreAudio codec info (see CACodecTag).
        /// </summary>
        CoreAudioCodec = 11,

        /// <summary>
        /// WMA codec: A description of the codec used by the file.
        /// 2 null-terminated UTF-8 strings are returned, with the 1st string being the name of the codec,
        /// and the 2nd containing additional information like what VBR setting was used.
        /// </summary>
        WmaCodec = 12,

        /// <summary>
        /// FLAC cuesheet.
        /// </summary>
        FlacCue = 12,

        /// <summary>
        /// Media Foundation tags : A pointer to a series of null-terminated UTF-8 strings
        /// is returned, the final string ending with a double null.
        /// </summary>
        MF = 13,

        /// <summary>
        /// WAVE format : A pointer to a <see cref="WaveFormat"/> structure is returned.
        /// </summary>
        WaveFormat = 14,

        /// <summary>
        /// ZXTune Sub Ogg.
        /// </summary>
        ZXTuneSuOgg = 0x00F10000,

        #region Riff
        /// <summary>
        /// RIFF/WAVE tags : array of null-terminated ANSI strings.
        /// </summary>
        RiffInfo = 0x100,

        /// <summary>
        /// BWF/RF64 tags (Broadcast Audio Extension) : A pointer to a variable Length block is returned.
        /// See the EBU specification for details of the block's structure.
        /// </summary>
        RiffBext = 0x101,

        /// <summary>
        /// RIFF/BWF Radio Traffic Extension tags : A pointer to a variable Length block is returned.
        /// See the EBU specifications for details of the block's structure.
        /// </summary>
        RiffCart = 0x102,

        /// <summary>
        /// RIFF DISP text chunk: a single ANSI string.
        /// </summary>
        RiffDISP = 0x103,
        #endregion

        /// <summary>
        /// + index# : ApeBinaryTag structure.
        /// </summary>
        ApeBinary = 0x1000,

        #region MOD
        /// <summary>
        /// MOD music name : a single ANSI string.
        /// </summary>
        MusicName = 0x10000,

        /// <summary>
        /// MOD message : a single ANSI string.
        /// </summary>
        MusicMessage = 0x10001,

        /// <summary>
        /// MOD music order list: BYTE array of pattern numbers played at that order position.
        /// Pattern number 254 is "+++" (skip order) and 255 is "---" (end song).
        /// You can use <see cref="Bass.ChannelGetLength"/> with <see cref="PositionFlags.MusicOrders"/> to get the Length of the array.
        /// </summary>
        MusicOrders = 0x10002,

        /// <summary>
        /// MOD author : UTF-8 string
        /// </summary>
        MusicAuth = 0x10003,

        /// <summary>
        /// + instrument#, MOD instrument name : ANSI string
        /// </summary>
        MusicInstrument = 0x10100,

        /// <summary>
        /// + sample#, MOD sample name : ANSI string
        /// </summary>
        MusicSample = 0x10300,

        /// <summary>
        /// + track#, track text : array of null-terminated ANSI strings
        /// </summary>
        MidiTrack = 0x11000,
        #endregion

        /// <summary>
        /// + index# : FLACPicture structure.
        /// </summary>
        FlacPicture = 0x12000,

        /// <summary>
        /// ADX tags: A pointer to the ADX loop structure.
        /// </summary>
        AdxLoop = 0x12000,

        #region DSD
        /// <summary>
        /// DSDIFF artist : ASCII string
        /// </summary>
        DSDArtist = 0x13000,

        /// <summary>
        /// DSDIFF title : ASCII string
        /// </summary>
        DSDTitle = 0x13001,

        /// <summary>
        /// + index, DSDIFF comment : A pointer to the DSDIFF comment tag structure.
        /// </summary>
        DSDComment = 0x13100,
        #endregion

        /// <summary>
        /// Segment's EXTINF tag: UTF-8 string.
        /// </summary>
        HlsExtInf = 0x14000,
        
        /// <summary>
        /// Segment's EXT-X-STREAM-INF tag: UTF-8 string.
        /// </summary>
        HlsStreamInf = 0x14001,

        /// <summary>
        /// EXT-X-PROGRAM-DATE-TIME tag: UTF-8 string.
        /// </summary>
        HlsDate = 0x14002
    }
}
