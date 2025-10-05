namespace ManagedBass.Cd
{
	/// <summary>
	/// The identification to retrieve, used with <see cref="BassCd.GetID" />.
	/// </summary>
	public enum CDID
	{
		/// <summary>
		/// Returns the catalog number of the CD.
		/// The number uses UPC/EAN-code (BAR coding).
		/// This might not be available for all CDs.
		/// </summary>
		UPC = 1,
		
        /// <summary>
		/// Produces a CDDB identifier.
		/// This can be used to get details on the CD's contents from a CDDB server.
		/// </summary>
		CDDB = 2,
		
        /// <summary>
		/// Produces a CDDB2 identifier.
		/// This can be used to get details on the CD's contents from a CDDB2 server.
		/// </summary>
		CDDB2 = 3,
		
        /// <summary>
		/// Retrieves the CD-TEXT information from the CD.
		/// CD-TEXT is not available on the majority of CDs.
		/// </summary>
		Text = 4,
		
        /// <summary>
		/// Produces an identifier that can be used to lookup CD details in the CDPLAYER.INI file, located in the Windows directory.
		/// </summary>
		CDPlayer = 5,

		/// <summary>
		/// Produces an identifier that can be used to get details on the CD's contents from <a href="http://www.musicbrainz.org">www.musicbrainz.org</a>.
		/// </summary>
		MusicBrainz = 6,

		/// <summary>
		/// Use: + track#. Returns the International Standard Recording Code of the track... 0 = first track.
		/// This might not be available for all CDs.
		/// </summary>
		ISRC = 0x100,

		/// <summary>
		/// Sends a "query" command to the configured CDDB server (see <see cref="BassCd.CDDBServer" />) to get a list of matching entries for the CD's CDDB identifier.
		/// The contents of each entry can be retrieved via the <see cref="Read"/> option.
		/// </summary>
		Query = 0x200,

		/// <summary>
		/// Use: + entry#.
		/// Sends a "read" command to the configured CDDB server (see <see cref="BassCd.CDDBServer" />) to get a database entry for the CD's CDDB identifier... 0 = first entry.
		/// </summary>
		Read = 0x201,

		/// <summary>
		/// Returns the cached CDDB "read" command response, if there is one.
		/// </summary>
		ReadCache = 0x2ff
	}
}