namespace ManagedBass
{
    /// <summary>
    /// The Notification used by <see cref="IOSNotifyProcedure"/>.
    /// </summary>
    public enum IOSNotify
    {
        /// <summary>
        /// iOS Audio Session Interruption Started.
        /// </summary>
        Interrupt = 1,

        /// <summary>
        /// iOS Audio Session Interruption Ended.
        /// </summary>
        InterruptEnd
    }
}
