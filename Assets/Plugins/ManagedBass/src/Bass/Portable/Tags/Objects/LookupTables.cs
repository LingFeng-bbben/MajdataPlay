using System.Collections.Generic;

namespace ManagedBass
{
    static class LookupTables
    {
        public static readonly TagProperties<IEnumerable<string>> Ape = new TagProperties<IEnumerable<string>>
        {
            Title = new[] { "title" },
            Artist = new[] { "artist" },
            Album = new[] { "album" },
            AlbumArtist = new[] { "album artist" },
            Track = new[] { "track" },
            Year = new[] { "year" },
            Genre = new[] { "genre" },
            Copyright = new[] { "copyright" },
            Encoder = new[] { "encodedby" },
            Publisher = new[] { "label" },
            Composer = new[] { "composer" },
            Conductor = new[] { "conductor" },
            Lyricist = new[] { "lyricist" },
            Remixer = new[] { "remixer" },
            Producer = new[] { "producer" },
            Comment = new[] { "comment" },
            Grouping = new[] { "grouping" },
            Mood = new[] { "mood" },
            Rating = new[] { "rating" },
            ISRC = new[] { "isrc" },
            BPM = new[] { "bpm" }
        };

        public static readonly TagProperties<IEnumerable<string>> Ogg = new TagProperties<IEnumerable<string>>
        {
            Title = new[] { "title" },
            Artist = new[] { "artist" },
            Album = new[] { "album" },
            AlbumArtist = new[] { "albumartist" },
            Track = new[] { "tracknumber" },
            Year = new[] { "date" },
            Genre = new[] { "genre" },
            Copyright = new[] { "copyright" },
            Encoder = new[] { "encodedby" },
            Publisher = new[] { "label" },
            Composer = new[] { "composer" },
            Conductor = new[] { "conductor" },
            Lyricist = new[] { "lyricist" },
            Remixer = new[] { "remixer" },
            Producer = new[] { "producer" },
            Comment = new[] { "comment" },
            Grouping = new[] { "grouping" },
            Mood = new[] { "mood" },
            Rating = new[] { "rating" },
            ISRC = new[] { "isrc" },
            BPM = new[] { "bpm" }
        };

        public static readonly TagProperties<IEnumerable<string>> RiffInfo = new TagProperties<IEnumerable<string>>
        {
            Title = new[] { "inam" },
            Artist = new[] { "iart" },
            Album = new[] { "iprd" },
            AlbumArtist = new[] { "isbj" },
            Track = new[] { "itrk", "iprt" },
            Year = new[] { "icrd" },
            Genre = new[] { "ignr" },
            Copyright = new[] { "icop" },
            Encoder = new[] { "isft" },
            Publisher = new[] { "icms" },
            Composer = new[] { "ieng" },
            Conductor = new[] { "itch" },
            Lyricist = new[] { "iwri" },
            Remixer = new[] { "iedt" },
            Producer = new[] { "ipro" },
            Comment = new[] { "icmt" },
            Grouping = new[] { "isrf" },
            Mood = new[] { "ikey" },
            Rating = new[] { "ishp" },
            ISRC = new[] { "isrc" },
            BPM = new[] { "ibpm" }
        };

        public static readonly TagProperties<IEnumerable<string>> Mp4 = new TagProperties<IEnumerable<string>>
        {
            Title = new[] { "©nam" },
            Artist = new[] { "©art" },
            Album = new[] { "©alb" },
            AlbumArtist = new[] { "aart" },
            Track = new[] { "trkn" },
            Year = new[] { "©day" },
            Genre = new[] { "©gen" },
            Copyright = new[] { "cprt" },
            Encoder = new[] { "©too" },
            Composer = new[] { "©wrt" },
            Comment = new[] { "©cmt" },
            Grouping = new[] { "©grp" },
            Rating = new[] { "rtng" },
        };

        public static readonly TagProperties<IEnumerable<string>> Id3v2 = new TagProperties<IEnumerable<string>>
        {
            Title = new[] { "TIT2", "TT2" },
            Artist = new[] { "TPE1", "TP1" },
            Album = new[] { "TALB", "TAL" },
            AlbumArtist = new[] { "TPE2", "TP2" },
            Subtitle = new[] { "TIT3", "TT3" },
            Track = new[] { "TRK", "TRCK" },
            Year = new[] { "TYER", "TYE" },
            Genre = new[] { "TCON", "TCO" },
            Copyright = new[] { "TCOP", "TCR" },
            Encoder = new[] { "TENC", "TEN" },
            Publisher = new[] { "TPUB", "TPB" },
            Composer = new[] { "TCOM", "TCM" },
            Conductor = new[] { "TPE3", "TP3" },
            Lyricist = new[] { "TEXT", "TXT" },
            Remixer = new[] { "TPE4", "TP4" },
            Producer = new[] { "TIPL" },
            Comment = new[] { "COMM", "COM" },
            Grouping = new[] { "TIT1", "TT1" },
            Mood = new[] { "TMOO" },
            Rating = new[] { "POPM" },
            ISRC = new[] { "TSCR" },
            BPM = new[] { "TBPM", "TBP" }
        };
    }
}