using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 布尔类型的设置项
    /// </summary>
    public class BoolSettingItem : ISettingItem
    {
        public string Name { get; }
        public bool IsNumeric => false;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<bool> _getter;
        readonly Action<bool> _setter;

        public BoolSettingItem(
            string name,
            Func<bool> getter,
            Action<bool> setter,
            bool isReadOnly = false)
        {
            Name = name;
            _getter = getter;
            _setter = setter;
            IsReadOnly = isReadOnly;
        }

        public object GetValue() => _getter();

        public string GetValueString() => _getter().ToString();

        public void SetValue(object value)
        {
            if (IsReadOnly) return;
            var converted = Convert.ToBoolean(value);
            _setter(converted);
            OnValueChanged?.Invoke(converted);
        }

        public void ModifyValue(int direction)
        {
            if (IsReadOnly) return;

            var newValue = !_getter();
            _setter(newValue);
            OnValueChanged?.Invoke(newValue);
        }
    }
}
