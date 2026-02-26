using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay;
[AttributeUsage(AttributeTargets.Property| AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class StepAttribute : Attribute
{
    public decimal Step
    {
        get => _step;
        init => _step = value;
    }

    readonly decimal _step;
}
