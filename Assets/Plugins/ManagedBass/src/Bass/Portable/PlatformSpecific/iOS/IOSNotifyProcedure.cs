namespace ManagedBass
{
    /// <summary>
    /// iOS notification callback function.
    /// </summary>
    /// <param name="Status">The notification status.</param>
    /// <remarks>
    /// The callback function receives notification of audio session interruptions which can use that instead of AudioSessionInitialize.
    /// </remarks>
    public delegate void IOSNotifyProcedure(IOSNotify Status);
}
