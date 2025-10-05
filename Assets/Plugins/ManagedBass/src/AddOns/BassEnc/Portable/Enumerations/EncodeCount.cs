namespace ManagedBass.Enc
{
    /// <summary>
    /// Used with <see cref="BassEnc.EncodeGetCount" /> to define the type of count you want to get.
    /// </summary>
    public enum EncodeCount
    {
        /// <summary>
        /// Get the bytes sent to the encoder.
        /// </summary>
        In,

        /// <summary>
        /// Get the bytes received from the encoder.
        /// </summary>
        Out,

        /// <summary>
        /// Get the bytes sent to the cast server.
        /// </summary>
        Cast,

        /// <summary>
        /// Data currently in the queue, waiting to be sent to the encoder (if async encoding is enabled).
        /// </summary>
        Queue,

        /// <summary>
        /// The queue's size limit (if async encoding is enabled).
        /// </summary>
        QueueLimit,

        /// <summary>
        /// Data not queued due to the queue being full or out of memory (if async encoding is enabled).
        /// </summary>
        QueueFail
    }
}