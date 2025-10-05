namespace ManagedBass.Enc
{
    /// <summary>
	/// Stats type to be used with <see cref="BassEnc.CastGetStats(int, EncodeStats, string)" /> to define the type of stats you want to get.
	/// </summary>
	public enum EncodeStats
    {
        /// <summary>
        /// Shoutcast stats.
        /// </summary>
        Shoutcast,

        /// <summary>
        /// Icecast mount-point stats.
        /// </summary>
        Icecast,

        /// <summary>
        /// Icecast server stats.
        /// </summary>
        IcecastServer
    }
}