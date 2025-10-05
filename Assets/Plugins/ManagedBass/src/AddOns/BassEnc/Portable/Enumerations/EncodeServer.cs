namespace ManagedBass.Enc
{
    /// <summary>
    /// To be used with <see cref="BassEnc.ServerInit" /> to define optional server flags.
    /// </summary>
    public enum EncodeServer
    {
        /// <summary>
        /// Default (no options).
        /// </summary>
        Default,

        /// <summary>
        /// No HTTP headers.
        /// </summary>
        NoHTTP,

        /// <summary>
        /// Shoutcast metadata
        /// </summary>
        Meta
    }
}