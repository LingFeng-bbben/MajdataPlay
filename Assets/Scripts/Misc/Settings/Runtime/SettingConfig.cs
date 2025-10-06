using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Settings.Runtime;
[Preserve]
internal class SettingConfig
{
    [JsonIgnore, Preserve]
    public int SelectedPage { get; set; } = 0;
    [JsonIgnore, Preserve]
    public int SelectedMenuIndex { get; set; } = 0;
}
