using MajdataPlay.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#nullable enable
namespace MajdataPlay.Collections
{
    public class OnlineSongCollection : SongCollection
    {
        public ApiEndpoint Source { get; init; }
        public OnlineSongCollection(ApiEndpoint source, string name, ISongDetail[] pArray) : base(null, name, pArray)
        {
            IsOnline = true;
            Source = source;
        }
        public OnlineSongCollection(ApiEndpoint source, string? dirPath, string name, ISongDetail[] pArray) : base(dirPath, name, pArray)
        {
            IsOnline = true;
            Source = source;
        }
    }
}
