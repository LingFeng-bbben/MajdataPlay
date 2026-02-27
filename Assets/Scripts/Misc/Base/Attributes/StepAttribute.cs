using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay;
[AttributeUsage(AttributeTargets.Property| AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class StepAttribute : Attribute
{
    public decimal Value
    {
        get => _value;
    }

    readonly decimal _value;
    public StepAttribute(double value)
    {
        _value = (decimal)value;
    }
    public StepAttribute(float value)
    {
        _value = (decimal)value;
    }
    public StepAttribute(int value)
    {
        _value = (decimal)value;
    }
}
