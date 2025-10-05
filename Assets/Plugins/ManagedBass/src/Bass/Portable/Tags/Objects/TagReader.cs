using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Reads tags from a File or a Channel depending on the <see cref="ChannelType"/>.
    /// </summary>
    public sealed class TagReader : TagProperties<string>
    {
        /// <summary>
        /// Provides tags that didn't fit into any Properties.
        /// </summary>
        public Dictionary<string, string> Other { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Provides the Pictures read from File/Channel.
        /// </summary>
        public List<PictureTag> Pictures { get; } = new List<PictureTag>();

        /// <summary>
        /// Gets the Lyrics.
        /// </summary>
        public string Lyrics { get; set; }
        
        /// <summary>
        /// Reads tags from a File.
        /// </summary>
        public static TagReader Read(string FileName)
        {
            Bass.Init();

            var h = Bass.CreateStream(FileName, Flags: BassFlags.Prescan);

            TagReader result = null;

            if (h != 0)
            {
                result = Read(h);

                Bass.StreamFree(h);
            }
            else
            {
                h = Bass.MusicLoad(FileName, Flags: BassFlags.Prescan);

                if (h != 0)
                {
                    result = Read(h);

                    Bass.MusicFree(h);
                }
            }

            if (!string.IsNullOrWhiteSpace(result?.Title))
                result.Title = System.IO.Path.GetFileNameWithoutExtension(FileName);

            return result;
        }

        /// <summary>
        /// Reads tags from a <paramref name="Channel"/>.
        /// </summary>
        public static TagReader Read(int Channel)
        {
            var result = new TagReader();
            var info = Bass.ChannelGetInfo(Channel);
            
            switch (info.ChannelType)
            {
                case ChannelType.MP1:
                case ChannelType.MP2:
                case ChannelType.MP3:
                    if (result.ReadID3v2(Channel)) { }

                    else if (result.ReadID3v1(Channel)) { }

                    else if (result.ReadApe(Channel)) { }

                    else if (result.ReadBWF(Channel)) { }
                    break;

                case ChannelType.OGG:
                    if (result.ReadOgg(Channel)) { }

                    else if (result.ReadApe(Channel)) { }
                    break;

                case ChannelType.MP4:
                case ChannelType.AAC:
                    if (result.ReadMp4(Channel)) { }

                    else if (result.ReadID3v2(Channel)) { }

                    else if (result.ReadApe(Channel)) { }

                    else if (result.ReadOgg(Channel)) { }
                    break;

                case ChannelType.Wave:
                case ChannelType.WavePCM:
                case ChannelType.WaveFloat:
                    if (result.ReadRiffInfo(Channel)) { }

                    else if (result.ReadBWF(Channel)) { }

                    else if (result.ReadID3v2(Channel)) { }
                    break;

                case ChannelType.DSD:
                    result.Title = Marshal.PtrToStringAnsi(Bass.ChannelGetTags(Channel, TagType.DSDTitle));
                    result.Artist = Marshal.PtrToStringAnsi(Bass.ChannelGetTags(Channel, TagType.DSDArtist));
                    break;

                case ChannelType.MOD:
                    result.Title = Marshal.PtrToStringAnsi(Bass.ChannelGetTags(Channel, TagType.MusicName));
                    result.Artist = Marshal.PtrToStringAnsi(Bass.ChannelGetTags(Channel, TagType.MusicAuth));
                    result.Comment = Marshal.PtrToStringAnsi(Bass.ChannelGetTags(Channel, TagType.MusicMessage));
                    break;

                default:
                    if (result.ReadApe(Channel)) { }

                    else if (result.ReadOgg(Channel)) { }

                    else if (result.ReadID3v2(Channel)) { }

                    else if (result.ReadID3v1(Channel)) { }
                    break;
            }

            if (string.IsNullOrWhiteSpace(result.Lyrics))
            {
                var ptr = Bass.ChannelGetTags(Channel, TagType.Lyrics3v2);

                if (ptr != IntPtr.Zero)
                    result.Lyrics = Marshal.PtrToStringAnsi(ptr);
            }

            return result;
        }

        IEnumerable<KeyValuePair<string, string>> ReadUsingLookupTable(IEnumerable<string> Tags, TagProperties<IEnumerable<string>> LookupTable, char Separator)
        {
            foreach (var tag in Tags)
            {
                var arrx = tag.Split(Separator);
                var key = arrx[0].ToLower();
                var value = arrx[1];

                if (!SetTagUsingLookupTable(key, value, LookupTable))
                    yield return new KeyValuePair<string, string>(arrx[0], value);
            }
        }

        bool SetTagUsingLookupTable(string Key, string Value, TagProperties<IEnumerable<string>> LookupTable)
        {
            if (LookupTable.Title != null && LookupTable.Title.Contains(Key))
                Title = Value;

            else if (LookupTable.Artist != null && LookupTable.Artist.Contains(Key))
                Artist = Value;

            else if (LookupTable.Album != null && LookupTable.Album.Contains(Key))
                Album = Value;

            else if (LookupTable.AlbumArtist != null && LookupTable.AlbumArtist.Contains(Key))
                AlbumArtist = Value;

            else if (LookupTable.Subtitle != null && LookupTable.Subtitle.Contains(Key))
                Subtitle = Value;

            else if (LookupTable.BPM != null && LookupTable.BPM.Contains(Key))
                BPM = Value;

            else if (LookupTable.Composer != null && LookupTable.Composer.Contains(Key))
                Composer = Value;

            else if (LookupTable.Copyright != null && LookupTable.Copyright.Contains(Key))
                Copyright = Value;

            else if (LookupTable.Genre != null && LookupTable.Genre.Contains(Key))
                Genre = Value;

            else if (LookupTable.Grouping != null && LookupTable.Grouping.Contains(Key))
                Grouping = Value;

            else if (LookupTable.Publisher != null && LookupTable.Publisher.Contains(Key))
                Publisher = Value;

            else if (LookupTable.Encoder != null && LookupTable.Encoder.Contains(Key))
                Encoder = Value;

            else if (LookupTable.Lyricist != null && LookupTable.Lyricist.Contains(Key))
                Lyricist = Value;

            else if (LookupTable.Year != null && LookupTable.Year.Contains(Key))
                Year = Value;

            else if (LookupTable.Conductor != null && LookupTable.Conductor.Contains(Key))
                Conductor = Value;

            else if (LookupTable.Track != null && LookupTable.Track.Contains(Key))
                Track = Value;

            else if (LookupTable.Producer != null && LookupTable.Producer.Contains(Key))
                Producer = Value;

            else if (LookupTable.Comment != null && LookupTable.Comment.Contains(Key))
                Comment = Value;

            else if (LookupTable.Mood != null && LookupTable.Mood.Contains(Key))
                Mood = Value;

            else if (LookupTable.Rating != null && LookupTable.Rating.Contains(Key))
                Rating = Value;

            else if (LookupTable.ISRC != null && LookupTable.ISRC.Contains(Key))
                ISRC = Value;

            else if (LookupTable.Remixer != null && LookupTable.Remixer.Contains(Key))
                Remixer = Value;
            
            else return false;

            return true;
        }

        #region Specific Tag Types
        /// <summary>
        /// Reads <see cref="TagType.APE"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadApe(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.APE);

            if (ptr == IntPtr.Zero)
                return false;

            foreach (var otherTag in ReadUsingLookupTable(Extensions.ExtractMultiStringUtf8(ptr), LookupTables.Ape, '='))
                Other.Add(otherTag.Key, otherTag.Value);

            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.OGG"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadOgg(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.OGG);

            if (ptr == IntPtr.Zero)
                return false;
            
            foreach (var otherTag in ReadUsingLookupTable(Extensions.ExtractMultiStringUtf8(ptr), LookupTables.Ogg, '='))
                Other.Add(otherTag.Key, otherTag.Value);

            if (string.IsNullOrWhiteSpace(Encoder))
            {
                var encoderPtr = Bass.ChannelGetTags(Channel, TagType.OggEncoder);
                if (encoderPtr != IntPtr.Zero)
                    Encoder = Extensions.PtrToStringUtf8(encoderPtr);
            }

            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.RiffInfo"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadRiffInfo(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.RiffInfo);

            if (ptr == IntPtr.Zero)
                return false;

            foreach (var otherTag in ReadUsingLookupTable(Extensions.ExtractMultiStringAnsi(ptr), LookupTables.RiffInfo, '='))
                Other.Add(otherTag.Key, otherTag.Value);
            
            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.MP4"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadMp4(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.MP4);

            if (ptr == IntPtr.Zero)
                return false;

            foreach (var otherTag in ReadUsingLookupTable(Extensions.ExtractMultiStringUtf8(ptr), LookupTables.Mp4, '='))
                Other.Add(otherTag.Key, otherTag.Value);
            
            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.ID3"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadID3v1(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.ID3);

            if (ptr == IntPtr.Zero)
                return false;
            
            var id3v1 = BassMarshal.PtrToStructure<ID3v1Tag>(ptr);

            Title = id3v1.Title;
            Artist = id3v1.Artist;
            Album = id3v1.Album;
            Year = id3v1.Year;
            Genre = id3v1.Genre;
            Track = id3v1.TrackNo.ToString();
            Comment = id3v1.Comment;

            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.ID3v2"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadID3v2(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.ID3v2);

            if (ptr == IntPtr.Zero)
                return false;

            var id3V2 = new ID3v2Tag(ptr);

            foreach (var frame in id3V2.TextFrames)
                if (!SetTagUsingLookupTable(frame.Key, frame.Value, LookupTables.Id3v2))
                    Other.Add(frame.Key, frame.Value);

            Pictures.AddRange(id3V2.PictureFrames);

            return true;
        }

        /// <summary>
        /// Reads <see cref="TagType.RiffBext"/> tags from a <paramref name="Channel"/>.
        /// </summary>
        public bool ReadBWF(int Channel)
        {
            var ptr = Bass.ChannelGetTags(Channel, TagType.RiffBext);

            if (ptr == IntPtr.Zero)
                return false;

            var tag = BassMarshal.PtrToStructure<BextTag>(ptr);

            Title = tag.Description;
            Artist = tag.Originator;
            Encoder = tag.OriginatorReference;
            Year = tag.OriginationDate.Split('-')[0];

            return true;
        }
        #endregion
    }
}