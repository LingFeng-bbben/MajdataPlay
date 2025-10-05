using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Wraps an ID3v1 tag
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ID3v1Tag
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        char[] id;

        /// <summary>
        /// Returns the Title (Max 30 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string Title;

        /// <summary>
        /// Returns the Artist (Max 30 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string Artist;

        /// <summary>
        /// Returns the Album (Max 30 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string Album;

        /// <summary>
        /// Returns the Year.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Year;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        char[] comment;

        /// <summary>
        /// Genre ID.
        /// </summary>
        public byte genre = 147;

        /// <summary>
        /// An array of Genres arranged by ID.
        /// </summary>
        public static string[] Genres = 
            {
                "Blues",
                "Classic Rock",
                "Country",
                "Dance",
                "Disco",
                "Funk",
                "Grunge",
                "Hip-Hop",
                "Jazz",
                "Metal",
                "New Age",
                "Oldies",
                "Other",
                "Pop",
                "R&B",
                "Rap",
                "Reggae",
                "Rock",
                "Techno",
                "Industrial",
                "Alternative",
                "Ska",
                "Death Metal",
                "Pranks",
                "Soundtrack",
                "Euro-Techno",
                "Ambient",
                "Trip-Hop",
                "Vocal",
                "Jazz+Funk",
                "Fusion",
                "Trance",
                "Classical",
                "Instrumental",
                "Acid",
                "House",
                "Game",
                "Sound Clip",
                "Gospel",
                "Noise",
                "Alternative Rock",
                "Bass",
                "Soul",
                "Punk",
                "Space",
                "Meditative",
                "Instrumental Pop",
                "Instrumental Rock",
                "Ethnic",
                "Gothic",
                "Darkwave",
                "Techno-Industrial",
                "Electronic",
                "Pop-Folk",
                "Eurodance",
                "Dream",
                "Southern Rock",
                "Comedy",
                "Cult",
                "Gangsta",
                "Top Christian Rap",
                "Pop/Funk",
                "Jungle",
                "Native US",
                "Cabaret",
                "New Wave",
                "Psychadelic",
                "Rave",
                "Showtunes",
                "Trailer",
                "Lo-Fi",
                "Tribal",
                "Acid Punk",
                "Acid Jazz",
                "Polka",
                "Retro",
                "Musical",
                "Rock & Roll",
                "Hard Rock",
                "Folk",
                "Folk-Rock",
                "National Folk",
                "Swing",
                "Fast Fusion",
                "Bebob",
                "Latin",
                "Revival",
                "Celtic",
                "Bluegrass",
                "Avantgarde",
                "Gothic Rock",
                "Progressive Rock",
                "Psychedelic Rock",
                "Symphonic Rock",
                "Slow Rock",
                "Big Band",
                "Chorus",
                "Easy Listening",
                "Acoustic",
                "Humour",
                "Speech",
                "Chanson",
                "Opera",
                "Chamber Music",
                "Sonata",
                "Symphony",
                "Booty Bass",
                "Primus",
                "Porn Groove",
                "Satire",
                "Slow Jam",
                "Club",
                "Tango",
                "Samba",
                "Folklore",
                "Ballad",
                "Power Ballad",
                "Rhythmic Soul",
                "Freestyle",
                "Duet",
                "Punk Rock",
                "Drum Solo",
                "Acapella",
                "Euro-House",
                "Dance Hall",
                "Goa",
                "Drum & Bass",
                "Club - House",
                "Hardcore",
                "Terror",
                "Indie",
                "BritPop",
                "Negerpunk",
                "Polsk Punk",
                "Beat",
                "Christian Gangsta Rap",
                "Heavy Metal",
                "Black Metal",
                "Crossover",
                "Contemporary Christian",
                "Christian Rock",
                "Merengue",
                "Salsa",
                "Thrash Metal",
                "Anime",
                "JPop",
                "Synthpop",
                "Unknown"
            };

        /// <summary>
        /// Reads an <see cref="ID3v1Tag"/>.
        /// </summary>
        /// <param name="Channel">The Channel to read from.</param>
        public static ID3v1Tag Read(int Channel)
        {
            return BassMarshal.PtrToStructure<ID3v1Tag>(Bass.ChannelGetTags(Channel, TagType.ID3));
        }

        /// <summary>
        /// Gets the Comment.
        /// </summary>
        public string Comment => comment == null ? "" : new string(comment).Replace("\0", "").Trim();

        /// <summary>
        /// Gets the Genre.
        /// </summary>
        public string Genre => Genres[genre > 147 ? 147 : genre];

        // If 29th byte of comment[] is null ('\0'), then 30th byte is track number
        /// <summary>
        /// Gets the Track Number.
        /// </summary>
        public int TrackNo => comment[28] == '\0' ? comment[29] : -1;
    }
}