using System;
using System.Collections.Generic;

#nullable enable
namespace MajdataPlay.Types
{
    public class Language
    {
        public string Code { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public LangTable[] MappingTable { get; init; } = Array.Empty<LangTable>();
        public override string ToString()
        {
            return $"{Code} - {Author}";
        }
        public static Language Default => new();
    }
}
