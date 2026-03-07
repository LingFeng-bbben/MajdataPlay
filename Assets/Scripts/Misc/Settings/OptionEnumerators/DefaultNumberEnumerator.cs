using System;

#nullable enable
namespace MajdataPlay.Settings.OptionEnumerators;
public class DefaultNumberEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    protected decimal Step = 0;
    protected decimal CurrentValue = 0;

    protected decimal? MaxValue = null;
    protected decimal? MinValue = null;

    public override bool MoveNext()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextValue = CurrentValue + Step;
        if (MaxValue is decimal maxValue)
        {
            if(CurrentValue >= maxValue)
            {
                return false;
            }
            nextValue = Math.Min(nextValue, maxValue);
        }
        CurrentValue = nextValue;
        var valueToSet = (object)nextValue;
        OptionValues[0] = valueToSet;
        Value = Convert.ChangeType(valueToSet, Type);
        return true;
    }
    public override bool MovePrevious()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextValue = CurrentValue - Step;
        if (MinValue is decimal minValue)
        {
            if (CurrentValue <= minValue)
            {
                return false;
            }
            nextValue = Math.Max(nextValue, minValue);
        }
        CurrentValue = nextValue;
        var valueToSet = (object)nextValue;
        OptionValues[0] = valueToSet;
        Value = Convert.ChangeType(valueToSet, Type);
        return true;
    }

    protected override void InitInternal()
    {
        var isNum = IsIntType || IsFloatType;
        if (!isNum)
        {
            throw new InvalidOperationException("Type provided must be an Number");
        }
        var rangeAttribute = GetCustomAttribute<RangeAttribute>();
        var stepAttribute = GetCustomAttribute<StepAttribute>();

        if (rangeAttribute is not null)
        {
            MinValue = rangeAttribute.Min;
            MaxValue = rangeAttribute.Max;
            if (!rangeAttribute.HasMin)
            {
                MinValue = null;
            }
            if (!rangeAttribute.HasMax)
            {
                MaxValue = null;
            }
        }
        if (stepAttribute is not null)
        {
            Step = stepAttribute.Value;
        }

        if (stepAttribute is null && rangeAttribute is null)
        {
            MaxValue = null;
            MinValue = null;
            if (IsIntType)
            {
                Step = 1m;
            }
            else
            {
                Step = 0.001m;
            }
        }

        OptionValues = new object[1];
        ValueIndex = 0;

        var currentValue = Value;
        CurrentValue = Convert.ToDecimal(currentValue);
        OptionValues[0] = CurrentValue;
    }
}
