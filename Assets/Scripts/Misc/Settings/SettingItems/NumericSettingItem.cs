using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 数值类型的设置项
    /// </summary>
    public class NumericSettingItem<T> : ISettingItem where T : struct, IComparable<T>, IConvertible
    {
        public string Name { get; }
        public bool IsNumeric => true;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<T> _getter;
        readonly Action<T> _setter;
        readonly Func<decimal>? _stepProvider;
        readonly decimal _fixedStep;
        readonly decimal? _minValue;
        readonly decimal? _maxValue;

        public NumericSettingItem(
            string name,
            Func<T> getter,
            Action<T> setter,
            decimal step,
            decimal? minValue = null,
            decimal? maxValue = null,
            bool isReadOnly = false,
            Func<decimal>? stepProvider = null)
        {
            Name = name;
            _getter = getter;
            _setter = setter;
            _fixedStep = step;
            _stepProvider = stepProvider;
            _minValue = minValue;
            _maxValue = maxValue;
            IsReadOnly = isReadOnly;
        }

        decimal GetStep() => _stepProvider?.Invoke() ?? _fixedStep;

        public object GetValue() => _getter()!;

        public string GetValueString() => _getter().ToString() ?? string.Empty;

        public void SetValue(object value)
        {
            if (IsReadOnly) return;
            var converted = (T)Convert.ChangeType(value, typeof(T));
            _setter(converted);
            OnValueChanged?.Invoke(converted);
        }

        public void ModifyValue(int direction)
        {
            if (IsReadOnly) return;

            var currentValue = Convert.ToDecimal(_getter());
            var step = GetStep();
            var newValue = Math.Round(currentValue + step * direction, 3);

            if (_maxValue.HasValue)
                newValue = Math.Min(newValue, _maxValue.Value);
            if (_minValue.HasValue)
                newValue = Math.Max(newValue, _minValue.Value);

            var converted = (T)Convert.ChangeType(newValue, typeof(T));
            _setter(converted);
            OnValueChanged?.Invoke(converted);
        }
    }
}
