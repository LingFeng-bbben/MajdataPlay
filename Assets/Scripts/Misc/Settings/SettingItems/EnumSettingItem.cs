using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 枚举类型的设置项
    /// </summary>
    public class EnumSettingItem<T> : ISettingItem where T : Enum
    {
        public string Name { get; }
        public bool IsNumeric => false;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<T> _getter;
        readonly Action<T> _setter;
        readonly T[] _values;

        public EnumSettingItem(
            string name,
            Func<T> getter,
            Action<T> setter,
            bool isReadOnly = false)
        {
            Name = name;
            _getter = getter;
            _setter = setter;
            IsReadOnly = isReadOnly;
            _values = (T[])Enum.GetValues(typeof(T));
        }

        public object GetValue() => _getter();

        public string GetValueString() => _getter().ToString() ?? string.Empty;

        public void SetValue(object value)
        {
            if (IsReadOnly) return;
            var converted = (T)Enum.Parse(typeof(T), value.ToString() ?? string.Empty);
            _setter(converted);
            OnValueChanged?.Invoke(converted);
        }

        public void ModifyValue(int direction)
        {
            if (IsReadOnly) return;

            var current = _getter();
            var currentIndex = Array.IndexOf(_values, current);
            var newIndex = currentIndex + direction;

            if (newIndex < 0)
                newIndex = _values.Length - 1;
            else if (newIndex >= _values.Length)
                newIndex = 0;

            var newValue = _values[newIndex];
            _setter(newValue);
            OnValueChanged?.Invoke(newValue);
        }
    }
}
