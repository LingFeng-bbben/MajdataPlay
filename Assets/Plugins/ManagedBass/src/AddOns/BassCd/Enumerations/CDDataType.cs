namespace ManagedBass.Cd
{
    /// <summary>
    /// The Type of data received, used with <see cref="CDDataProcedure" />.
    /// </summary>
    public enum CDDataType
    {
        /// <summary>
        /// Sub-channel data.
        /// </summary>
        SubChannel,

        /// <summary>
        /// C2 error info.
        /// </summary>
        C2
    }
}