using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings;
#nullable enable
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DefaultValueAttribute : Attribute
{
    public object? Value
    {
        get => _value;
        init => _value = value;
    }
    object? _value;
}
