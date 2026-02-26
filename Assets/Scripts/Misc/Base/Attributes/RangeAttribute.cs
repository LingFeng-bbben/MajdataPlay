using MajdataPlay.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class RangeAttribute : Attribute
{
    public decimal Min
    {
        get => _min;
        init => _min = value;
    }
    public decimal Max
    {
        get => _max;
        init => _max = value;
    }
    public bool IsStartInclusive
    {
        get => _isStartInclusive;
        set => _isStartInclusive = value;
    }
    public bool IsEndInclusive
    {
        get => _isEndInclusive;
        set => _isEndInclusive = value;
    }

    decimal _min;
    decimal _max;
    bool _isStartInclusive;
    bool _isEndInclusive;
}
