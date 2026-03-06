using System;
#nullable enable
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

    readonly bool _hasMin;
    readonly bool _hasMax;

    readonly decimal _min;
    readonly decimal _max;

    public RangeAttribute()
    {
        
    }
    public RangeAttribute(string? min, string? max)
    {
        if(!string.IsNullOrEmpty(min))
        {
            _min = decimal.Parse(min);
        }
        if (!string.IsNullOrEmpty(max))
        {
            _max = decimal.Parse(max);
        }
    }
}
