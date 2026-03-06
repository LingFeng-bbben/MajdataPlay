using MajdataPlay.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class RangeAttribute : Attribute
{
    public decimal Min
    {
        get => _min;
    }
    public decimal Max
    {
        get => _max;
    }
    public bool IsStartInclusive
    {
        get => _isStartInclusive;
    }
    public bool IsEndInclusive
    {
        get => _isEndInclusive;
    }

    readonly decimal _min;
    readonly decimal _max;
    readonly bool _isStartInclusive;
    readonly bool _isEndInclusive;
}
