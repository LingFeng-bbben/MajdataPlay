using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 数值类型的设置项（支持 int, float 等）
    /// </summary>
    public class NumericSettingItem<T> : ISettingItem where T : struct
    {
        public string Name { get; }
        public bool IsNumeric => true;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<T> _getter;
        readonly Action<T> _setter;
        readonly decimal _step;
        readonly Func<decimal>? _stepProvider;
        readonly decimal? _minValue;
        readonly decimal? _maxValue;

        public NumericSettingItem(
            string name,
            Func<T> getter,
            Action<T> setter,
            decimal step = 1,
            Func<decimal>? stepProvider = null,
            decimal? minValue = null,
            decimal? maxValue = null,
            bool isReadOnly = false)
        {
            Name = name;
            _getter = getter;
            _setter = setter;
            _step = step;
            _stepProvider = stepProvider;
            _minValue = minValue;
            _maxValue = maxValue;
            IsReadOnly = isReadOnly;
        }

        public object GetValue() => _getter();

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
            var step = _stepProvider?.Invoke() ?? _step;
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
