using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay
{
    public class SongOrder
    {
        [JsonIgnore]
        public string Keyword { get; set; } = string.Empty;
        public SortType SortBy { get; set; } = SortType.Default;
    }
}
