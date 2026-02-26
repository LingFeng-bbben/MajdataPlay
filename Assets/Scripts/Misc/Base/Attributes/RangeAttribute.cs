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
    public decimal Min { get; }
    public decimal Max { get; }
    public bool IsStartInclusive
    {
        get => Range.IsStartInclusive;
    }
    public bool IsEndInclusive
    {
        get => Range.IsEndInclusive;
    }
    public Range<decimal> Range { get; }
    public RangeAttribute(decimal min, decimal max, ContainsType containsType = ContainsType.Closed)
    {
        Min = min;
        Max = max;
        Range = new Range<decimal>(min, max, containsType);
    }
    public RangeAttribute(Range<decimal> range)
    {
        Min = range.Start;
        Max = range.End;
        Range = range;
    }
}
