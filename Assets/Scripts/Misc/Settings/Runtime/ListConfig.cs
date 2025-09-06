using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.Runtime;
internal class ListConfig
{
    public int SelectedSongIndex { get; set; } = 0;
    public int SelectedDir { get; set; } = 0;
    public Guid SelectedDirGuid { get; set; } = Guid.Empty;
    public string SelectedSongHash { get; set; } = string.Empty;
    public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
    [JsonIgnore]
    public SongOrder OrderBy { get; } = new();
}
