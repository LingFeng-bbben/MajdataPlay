namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx BiQuad filter type. Defines within the <see cref="BQFParameters" /> structure which BiQuad filter should be used.
    /// </summary>
    public enum BQFType
    {
        /// <summary>
        /// BiQuad Lowpass filter.
        /// </summary>
        LowPass,
        
        /// <summary>
        /// BiQuad Highpass filter.
        /// </summary>
        HighPass,
        
        /// <summary>
        /// BiQuad Bandpass filter (constant 0 dB peak gain).
        /// </summary>
        BandPass,
        
        /// <summary>
        /// BiQuad Bandpass Q filter (constant skirt gain, peak gain = Q).
        /// </summary>
        BandPassQ,
        
        /// <summary>
        /// BiQuad Notch filter.
        /// </summary>
        Notch,
        
        /// <summary>
        /// BiQuad All-Pass filter.
        /// </summary>
        AllPass,
        
        /// <summary>
        /// BiQuad Peaking EQ filter.
        /// </summary>
        PeakingEQ,
        
        /// <summary>
        /// BiQuad Low-Shelf filter.
        /// </summary>
        LowShelf,
        
        /// <summary>
        /// BiQuad High-Shelf filter.
        /// </summary>
        HighShelf
    }
}