namespace ManagedBass.Asio
{
    /// <summary>
    /// BassAsio notify values as used in the <see cref="AsioNotifyProcedure" />.
    /// </summary>
    public enum AsioNotify
    {
        /// <summary>
        /// The device's sample rate has changed.
        /// The new rate is available from <see cref="BassAsio.Rate" />.
        /// </summary>
        Rate = 1,

        /// <summary>
        /// The driver has requested a reset/reinitialization;
        /// for example, following a change of the default Buffer size.
        /// This request can be ignored, but if a reinitialization is performed, it should not be done within the callback.
        /// </summary>
        Reset
    }
}