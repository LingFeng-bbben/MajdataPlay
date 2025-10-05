using System;

namespace ManagedBass
{
    /// <summary>
    /// Bass Error Codes returned by <see cref="Bass.LastError" /> and BassAsio.LastError.
    /// </summary>
    public enum Errors
    {
        /// <summary>
        /// Some other mystery error
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// No Error
        /// </summary>
        OK = 0,

        /// <summary>
        /// Memory Error
        /// </summary>
        Memory = 1,

        /// <summary>
        /// Can't open the file
        /// </summary>
        FileOpen = 2,

        /// <summary>
        /// Can't find a free/valid driver
        /// </summary>
        Driver = 3,

        /// <summary>
        /// The sample Buffer was lost
        /// </summary>
        BufferLost = 4,

        /// <summary>
        /// Invalid Handle
        /// </summary>
        Handle = 5,

        /// <summary>
        /// Unsupported sample format
        /// </summary>
        SampleFormat = 6,

        /// <summary>
        /// Invalid playback position
        /// </summary>
        Position = 7,

        /// <summary>
        /// <see cref="Bass.Init"/> has not been successfully called
        /// </summary>
        Init = 8,

        /// <summary>
        /// <see cref="Bass.Start"/> has not been successfully called
        /// </summary>
        Start = 9,

        /// <summary>
        /// SSL/HTTPS support isn't available.
        /// </summary>
        [Obsolete("Use SSL instead.")]
        SLL = 10,
        
        /// <summary>
        /// SSL/HTTPS support isn't available.
        /// </summary>
        SSL = 10,

        /// <summary>
        /// No CD in drive
        /// </summary>
        NoCD = 12,

        /// <summary>
        /// Invalid track number
        /// </summary>
        CDTrack = 13,

        /// <summary>
        /// Already initialized/paused/whatever
        /// </summary>
        Already = 14,

        /// <summary>
        /// Not paused
        /// </summary>
        NotPaused = 16,

        /// <summary>
        /// Not an audio track
        /// </summary>
        NotAudioTrack = 17,

        /// <summary>
        /// Can't get a free channel
        /// </summary>
        NoChannel = 18,

        /// <summary>
        /// An illegal Type was specified
        /// </summary>
        Type = 19,

        /// <summary>
        /// An illegal parameter was specified
        /// </summary>
        Parameter = 20,

        /// <summary>
        /// No 3D support
        /// </summary>
        No3D = 21,

        /// <summary>
        /// No EAX support
        /// </summary>
        NoEAX = 22,

        /// <summary>
        /// Illegal device number
        /// </summary>
        Device = 23,

        /// <summary>
        /// Not playing
        /// </summary>
        NotPlaying = 24,

        /// <summary>
        /// Illegal sample rate
        /// </summary>
        SampleRate = 25,

        /// <summary>
        /// The stream is not a file stream
        /// </summary>
        NotFile = 27,

        /// <summary>
        /// No hardware voices available
        /// </summary>
        NoHW = 29,

        /// <summary>
        /// The MOD music has no sequence data
        /// </summary>
        Empty = 31,

        /// <summary>
        /// No internet connection could be opened
        /// </summary>
        NoInternet = 32,

        /// <summary>
        /// Couldn't create the file
        /// </summary>
        Create = 33,

        /// <summary>
        /// Effects are not available
        /// </summary>
        NoFX = 34,

        /// <summary>
        /// The channel is playing
        /// </summary>
        Playing = 35,

        /// <summary>
        /// Requested data is not available
        /// </summary>
        NotAvailable = 37,

        /// <summary>
        /// The channel is a 'Decoding Channel'
        /// </summary>
        Decode = 38,

        /// <summary>
        /// A sufficient DirectX version is not installed
        /// </summary>
        DirectX = 39,

        /// <summary>
        /// Connection timedout
        /// </summary>
        Timeout = 40,

        /// <summary>
        /// Unsupported file format
        /// </summary>
        FileFormat = 41,

        /// <summary>
        /// Unavailable speaker
        /// </summary>
        Speaker = 42,

        /// <summary>
        /// Invalid BASS version (used by add-ons)
        /// </summary>
        Version = 43,

        /// <summary>
        /// Codec is not available/supported
        /// </summary>
        Codec = 44,

        /// <summary>
        /// The channel/file has ended
        /// </summary>
        Ended = 45,

        /// <summary>
        /// The device is busy (eg. in "exclusive" use by another process)
        /// </summary>
        Busy = 46,

        /// <summary>
        /// The file cannot be streamed using the buffered file system.
        /// This could be because an MP4 file's "mdat" atom comes before its "moov" atom. 
        /// </summary>
        Unstreamable = 47,
 
        #region BassWma
        /// <summary>
        /// BassWma: The file is protected
        /// </summary>
        WmaLicense = 1000,

        /// <summary>
        /// BassWma: WM9 is required
        /// </summary>
        WM9 = 1001,

        /// <summary>
        /// BassWma: Access denied (Username/Password is invalid)
        /// </summary>
        WmaAccesDenied = 1002,

        /// <summary>
        /// BassWma: No appropriate codec is installed
        /// </summary>
        WmaCodec = 1003,

        /// <summary>
        /// BassWma: individualization is needed
        /// </summary>
        WmaIndividual = 1004,
        #endregion

        /// <summary>
        /// BassWASAPI: WASAPI Not available
        /// </summary>
        Wasapi = 5000,

        /// <summary>
		/// BassEnc: ACM codec selection cancelled
		/// </summary>
		AcmCancel = 2000,

        /// <summary>
        /// BassEnc: Access denied (invalid password)
        /// </summary>
        CastDenied = 2100,

        /// <summary>
        /// BassAAC: Non-Streamable due to MP4 atom order ("mdat" before "moov")
        /// </summary>
        Mp4NoStream = 6000
    }
}
