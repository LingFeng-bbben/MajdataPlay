using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.Runtime;
internal class SettingConfig
{
    [JsonIgnore]
    public int SelectedPage { get; set; } = 0;
    [JsonIgnore]
    public int SelectedMenuIndex { get; set; } = 0;
}
