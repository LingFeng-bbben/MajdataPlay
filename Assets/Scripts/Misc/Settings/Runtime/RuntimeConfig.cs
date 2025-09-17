using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Settings.Runtime;
[Preserve]
internal class RuntimeConfig
{
    [Preserve]
    public ListConfig List { get; init; } = new();
    [Preserve]
    public SettingConfig Setting { get; init; } = new();
}
