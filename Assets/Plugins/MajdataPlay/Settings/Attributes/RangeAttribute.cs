using System;

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
    public bool HasMin 
    {
        get => _hasMin;
        init => _hasMin = value;
    }
    public bool HasMax
    {
        get => _hasMax;
        init => _hasMax = value;
    }
    public bool IsStartInclusive
    {
        get => _isStartInclusive;
        init => _isStartInclusive = value;
    }
    public bool IsEndInclusive
    {
        get => _isEndInclusive;
        init => _isEndInclusive = value;
    }

    readonly bool _isStartInclusive = true;
    readonly bool _isEndInclusive = true;

    readonly bool _hasMin;
    readonly bool _hasMax;

    readonly decimal _min;
    readonly decimal _max;

    public RangeAttribute()
    {
        
    }
    public RangeAttribute(string min, string max)
    {
        _min = decimal.Parse(min);
        _max = decimal.Parse(max);
    }
}
