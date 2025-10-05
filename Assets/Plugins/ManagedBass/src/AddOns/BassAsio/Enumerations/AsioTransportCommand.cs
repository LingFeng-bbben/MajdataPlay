namespace ManagedBass.Asio
{
    /// <summary>
    /// Asio Transport Command to be used with <see cref="AsioTransportParameters"/>.
    /// </summary>
    public enum AsioTransportCommand
    {
        /// <summary>
        /// Start
        /// </summary>
        Start = 1,

        /// <summary>
        /// Stop
        /// </summary>
        Stop,

        /// <summary>
        /// Locate
        /// </summary>
        Locate,

        /// <summary>
        /// Punch in
        /// </summary>
        PunchIn,

        /// <summary>
        /// Punch out
        /// </summary>
        PunchOut,

        /// <summary>
        /// Arm on
        /// </summary>
        ArmOn,

        /// <summary>
        /// Arm off
        /// </summary>
        ArmOff,

        /// <summary>
        /// Monitor on
        /// </summary>
        MonitorOn,
        
        /// <summary>
        /// Monitor off
        /// </summary>
        MonitorOff,

        /// <summary>
        /// Arm
        /// </summary>
        Arm,

        /// <summary>
        /// Monitor
        /// </summary>
        Monitor
    }
}