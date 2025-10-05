namespace ManagedBass
{
    /// <summary>
    /// Direct Sound interface flags for use with <see cref="Bass.GetDSoundObject(DSInterface)" /> (Windows only).
    /// </summary>
    public enum DSInterface
    {
        /// <summary>
        /// Retrieve the IDirectSound interface.
        /// </summary>
        IDirectSound = 1,

        /// <summary>
        /// Retrieve the IDirectSound3DListener interface.
        /// </summary>
        IDirectSound3DListener
    }
}