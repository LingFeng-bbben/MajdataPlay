namespace ManagedBass.Wasapi
{
    /// <summary>
    /// Wasapi Device Type to be used with <see cref="WasapiDeviceInfo"/>.
    /// </summary>
    public enum WasapiDeviceType
    {
        /// <summary>
        /// A network device.
        /// </summary>
        NetworkDevice,
        
        /// <summary>
        /// A speakers device.
        /// </summary>
        Speakers,
        
        /// <summary>
        /// A line level device.
        /// </summary>
        LineLevel,
        
        /// <summary>
        /// A headphone device.
        /// </summary>
        Headphones,
        
        /// <summary>
        /// A microphone device.
        /// </summary>
        Microphone,
        
        /// <summary>
        /// A headset device.
        /// </summary>
        Headset,
        
        /// <summary>
        /// A handset device.
        /// </summary>
        Handset,
        
        /// <summary>
        /// A digital device.
        /// </summary>
        Digital,
        
        /// <summary>
        /// A S/PDIF device.
        /// </summary>
        SPDIF,
        
        /// <summary>
        /// A HDMI device.
        /// </summary>
        HDMI,
        
        /// <summary>
        /// An unknown device.
        /// </summary>
        Unknown
    }
}