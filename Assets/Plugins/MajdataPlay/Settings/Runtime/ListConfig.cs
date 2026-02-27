using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Settings.Runtime;
[Preserve]
internal class ListConfig
{
    [Preserve]
    public int SelectedSongIndex { get; set; } = 0;
    [Preserve]
    public int SelectedDir { get; set; } = 0;
    [Preserve]
    public Guid SelectedDirGuid { get; set; } = Guid.Empty;
    [Preserve]
    public string SelectedSongHash { get; set; } = string.Empty;
    [Preserve]
    public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
    [JsonIgnore, Preserve]
    public SongOrder OrderBy { get; } = new();
}
