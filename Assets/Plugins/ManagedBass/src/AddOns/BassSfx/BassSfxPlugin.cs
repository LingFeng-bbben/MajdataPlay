namespace ManagedBass.Sfx
{
	/// <summary>
	/// Flag which identifies the type/kind of the visual plugin.
	/// </summary>
	public enum BassSfxPlugin
	{
		/// <summary>
		/// An unknown plugin type (not supported).
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// Sonique visual plugin.
		/// </summary>
		Sonique = 0,
		
        /// <summary>
		/// Winamp visual plugin.
		/// </summary>
		Winamp = 1,
		
        /// <summary>
		/// Windows Media Player visual plugin.
		/// </summary>
		WMP = 2,
		
        /// <summary>
		/// BassBox visual plugin.
		/// </summary>
		BBP = 3
	}
}