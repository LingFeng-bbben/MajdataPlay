using System.Collections.Generic;

#nullable enable
namespace MajdataPlay.Types
{
    public class Language
    {
        public string Code { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public Dictionary<MajText, string> MappingTable { get; init; } = new();

        public static Language Default => new();
    }
}
