namespace ManagedBass.Sfx
{
	/// <summary>
	/// BASS_SFX error codes as returned by <see cref="BassSfx.LastError" />.
	/// </summary>
	public enum BassSfxError
	{
		/// <summary>
		/// Some other mystery problem.
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// All is OK.
		/// </summary>
		OK = 0,
		
        /// <summary>
		/// Memory error.
		/// </summary>
		Memory = 1,

		/// <summary>
		/// Can't open the file.
		/// </summary>
		FileOpen = 2,

		/// <summary>
		/// Invalid handle.
		/// </summary>
		Handle = 3,

		/// <summary>
		/// Already initialized.
		/// </summary>
		Already = 4,

		/// <summary>
		/// Unsupported plugin format.
		/// </summary>
		Format = 5,

		/// <summary>
		/// <see cref="BassSfx.Init" /> has not been successfully called.
		/// </summary>
		Init = 6,
		
        /// <summary>
		/// Can't open WMP plugin using specified GUID.
		/// </summary>
		GUID = 7
	}
}