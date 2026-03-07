using System;

#nullable enable
namespace MajdataPlay.Settings.OptionEnumerators;
public class DefaultNumberEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    decimal _step = 0;
    decimal _currentValue = 0;

    decimal? _maxValue = null;
    decimal? _minValue = null;

    public override bool MoveNext()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextValue = _currentValue + _step;
        if (_maxValue is decimal maxValue)
        {
            if(_currentValue >= maxValue)
            {
                return false;
            }
            nextValue = Math.Min(nextValue, maxValue);
        }
        var valueToSet = (object)nextValue;
        OptionValues[0] = valueToSet;
        Value = valueToSet;
        return true;
    }
    public override bool MovePrevious()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextValue = _currentValue - _step;
        if (_minValue is decimal minValue)
        {
            if (_currentValue <= minValue)
            {
                return false;
            }
            nextValue = Math.Max(nextValue, minValue);
        }
        var valueToSet = (object)nextValue;
        OptionValues[0] = valueToSet;
        Value = valueToSet;
        return true;
    }

    protected override void InitInternal()
    {
        var isNum = IsIntType || IsFloatType;
        if (isNum)
        {
            throw new InvalidOperationException("Type provided must be an Number");
        }
        var rangeAttribute = GetCustomAttribute<RangeAttribute>();
        var stepAttribute = GetCustomAttribute<StepAttribute>();

        if (rangeAttribute is not null)
        {
            _minValue = rangeAttribute.Min;
            _maxValue = rangeAttribute.Max;
            if (!rangeAttribute.HasMin)
            {
                _minValue = null;
            }
            if (!rangeAttribute.HasMax)
            {
                _maxValue = null;
            }
        }
        if (stepAttribute is not null)
        {
            _step = stepAttribute.Value;
        }

        if (stepAttribute is null && rangeAttribute is null)
        {
            _maxValue = null;
            _minValue = null;
            if (IsIntType)
            {
                _step = 1m;
            }
            else
            {
                _step = 0.001m;
            }
        }

        OptionValues = new object[1];
        ValueIndex = 0;

        var currentValue = Value;
        OptionValues[0] = currentValue;
        _currentValue = (decimal)currentValue;
    }
}
