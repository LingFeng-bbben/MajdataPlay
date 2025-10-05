namespace ManagedBass
{
    /// <summary>
    /// Software 3D mixing algorithm modes to be used with <see cref="Bass.Algorithm3D"/>.
    /// <remarks>
    /// On Windows, DirectX 7 or above is required for this option to have effect.
    /// On other platforms, only the <see cref="Default"/> and <see cref="Off"/> options are available.
    /// </remarks>
    /// </summary>
    public enum Algorithm3D
    {
        /// <summary>
        /// Default algorithm (currently translates to <see cref="Off"/>)
        /// </summary>
        Default,

        /// <summary>
        /// Uses normal left and right panning.
        /// The vertical axis is ignored except for scaling of volume due to distance.
        /// <para>
        /// Doppler shift and volume scaling are still applied, but the 3D filtering is not performed.
        /// This is the most CPU efficient software implementation, but provides no virtual 3D audio effect.
        /// Head Related Transfer Function processing will not be done.
        /// Since only normal stereo panning is used, a channel using this algorithm may be accelerated by a 2D hardware voice if no free 3D hardware voices are available.
        /// </para>
        /// </summary>
        Off,

        /// <summary>
        /// This algorithm gives the highest quality 3D audio effect, but uses more CPU (Windows Only).
        /// <para>
        /// Requires Windows 98 2nd Edition or Windows 2000 that uses WDM drivers,
        /// if this mode is not available then <see cref="Off"/> will be used instead.
        /// </para>
        /// </summary>
        Full,

        /// <summary>
        /// This algorithm gives a good 3D audio effect, and uses less CPU than the FULL mode (Windows Only).
        /// <para>
        /// Requires Windows 98 2nd Edition or Windows 2000 that uses WDM drivers,
        /// if this mode is not available then <see cref="Off"/> will be used instead.
        /// </para>
        /// </summary>
        Light
    }
}
