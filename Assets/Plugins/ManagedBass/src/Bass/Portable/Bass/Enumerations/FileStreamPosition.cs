namespace ManagedBass
{
    /// <summary>
    /// Stream File Position modes to be used with <see cref="Bass.StreamGetFilePosition" />
    /// </summary>
    public enum FileStreamPosition
    {
        /// <summary>
        /// Position that is to be decoded for playback next.
        /// This will be a bit ahead of the position actually being heard due to buffering.
        /// </summary>
        Current,

        /// <summary>
        /// Download progress of an internet file stream or "buffered" User file stream.
        /// </summary>
        Download,

        /// <summary>
        /// End of the file, in other words the file Length.
        /// When streaming in blocks, the file Length is unknown, so the download Buffer Length is returned instead.
        /// </summary>
        End,

        /// <summary>
        /// Start of stream data in the file.
        /// </summary>
        Start,

        /// <summary>
        /// Internet file stream or "buffered" User file stream is still connected? 0 = no, 1 = yes.
        /// </summary>
        Connected,

        /// <summary>
        /// The amount of data in the Buffer of an internet file stream or "buffered" User file stream.
        /// Unless streaming in blocks, this is the same as <see cref="Download"/>.
        /// </summary>
        Buffer,

        /// <summary>
        /// Returns the socket hanlde used for streaming.
        /// </summary>
        Socket,

        /// <summary>
        /// The amount of data in the asynchronous file reading Buffer.
        /// This requires that the <see cref="BassFlags.AsyncFile"/> flag was used at the stream's creation.
        /// </summary>
        AsyncBuffer,

        /// <summary>
        /// WMA add-on: internet buffering progress (0-100%)
        /// </summary>
        WmaBuffer = 1000,

        /// <summary>
        /// Segment Sequence number.
        /// </summary>
        HlsSegment = 0x10000
    }
}
