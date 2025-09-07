using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.Runtime;
internal class RuntimeConfig
{
    public ListConfig List { get; init; } = new();
    public SettingConfig Setting { get; init; } = new();
}
