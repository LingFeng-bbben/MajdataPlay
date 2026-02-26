using System;
using System.Linq;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 枚举类型的设置项
    /// </summary>
    public class EnumSettingItem<T> : ISettingItem where T : struct, Enum
    {
        public string Name { get; }
        public bool IsNumeric => false;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<T> _getter;
        readonly Action<T> _setter;
        readonly T[] _values;
        readonly int _maxIndex;

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
            _values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            _maxIndex = _values.Length - 1;
        }

        public object GetValue() => _getter();

        public string GetValueString() => _getter().ToString();

        public void SetValue(object value)
        {
            if (IsReadOnly) return;
            var converted = (T)value;
            _setter(converted);
            OnValueChanged?.Invoke(converted);
        }

        public void ModifyValue(int direction)
        {
            if (IsReadOnly) return;

            var currentValue = _getter();
            var currentIndex = Array.IndexOf(_values, currentValue);
            
            currentIndex += direction;
            if (currentIndex < 0)
                currentIndex = _maxIndex;
            else if (currentIndex > _maxIndex)
                currentIndex = 0;

            var newValue = _values[currentIndex];
            _setter(newValue);
            OnValueChanged?.Invoke(newValue);
        }
    }
}
