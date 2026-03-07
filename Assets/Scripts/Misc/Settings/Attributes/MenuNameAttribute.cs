using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings;
#nullable enable
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class MenuNameAttribute : Attribute
{
    public string Name { get; }
    public MenuNameAttribute(string name)
    {
        Name = name ?? string.Empty;
    }
}
