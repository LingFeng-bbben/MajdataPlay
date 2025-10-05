namespace ManagedBass.Enc
{
    /// <summary>
    /// To be used with <see cref="BassEnc.EncodeSetNotify" /> to receive notifications on an encoder's status.
    /// </summary>
    public enum EncodeNotifyStatus
    {
        /// <summary>
        /// Encoder died
        /// </summary>
        EncoderDied = 1,

        /// <summary>
        /// Cast server connection died
        /// </summary>
        CastServerConnectionDied = 2,

        /// <summary>
        /// Cast timeout
        /// </summary>
        CastTimeout = 0x10000,

        /// <summary>
        /// Queue is out of space
        /// </summary>
        QueueFull = 0x10001,

        /// <summary>
        /// Encoder has been freed
        /// </summary>
        Free = 0x10002
    }
}
