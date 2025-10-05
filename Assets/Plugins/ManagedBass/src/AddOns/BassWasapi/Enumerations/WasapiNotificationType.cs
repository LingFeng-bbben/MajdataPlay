namespace ManagedBass.Wasapi
{
    /// <summary>
    /// BassWasapi Notification type to be used with <see cref="BassWasapi.SetNotify"/>.
    /// </summary>
    public enum WasapiNotificationType
    {
        /// <summary>
        /// The device has been enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// The device has been disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The device is now the default Input device.
        /// </summary>
        DefaultOutput,

        /// <summary>
        /// The device is now the default output device.
        /// </summary>
        DefaultInput,

        /// <summary>
        /// The device has failed and been stopped.
        /// If the device is still enabled and shared mode was being used, then it may be that the device's sample format has changed.
        /// It can be freed and reinitialized, with <see cref="BassWasapi.Free"/> and <see cref="BassWasapi.Init"/>, to resume in that case.
        /// </summary>
        Fail = 0x100
    }
}