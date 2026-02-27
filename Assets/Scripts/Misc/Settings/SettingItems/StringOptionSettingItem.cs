using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 字符串选项类型的设置项（从预定义的选项中选择）
    /// </summary>
    public class StringOptionSettingItem : ISettingItem
    {
        public string Name { get; }
        public bool IsNumeric => false;
        public bool IsReadOnly { get; }

        public event Action<object>? OnValueChanged;

        readonly Func<string> _getter;
        readonly Action<string> _setter;
        readonly Func<string[]> _optionsProvider;

        int _currentIndex = 0;

        public StringOptionSettingItem(
            string name,
            Func<string> getter,
            Action<string> setter,
            Func<string[]> optionsProvider,
            bool isReadOnly = false)
        {
            Name = name;
            _getter = getter;
            _setter = setter;
            _optionsProvider = optionsProvider;
            IsReadOnly = isReadOnly;
            UpdateCurrentIndex();
        }

        void UpdateCurrentIndex()
        {
            var options = _optionsProvider();
            var current = _getter();
            _currentIndex = Array.IndexOf(options, current);
            if (_currentIndex < 0)
                _currentIndex = 0;
        }

        public object GetValue() => _getter();

        public string GetValueString() => _getter();

        public void SetValue(object value)
        {
            if (IsReadOnly) return;
            var converted = value.ToString() ?? string.Empty;
            _setter(converted);
            OnValueChanged?.Invoke(converted);
            UpdateCurrentIndex();
        }

        public void ModifyValue(int direction)
        {
            if (IsReadOnly) return;

            var options = _optionsProvider();
            if (options.Length == 0) return;

            _currentIndex += direction;
            if (_currentIndex < 0)
                _currentIndex = options.Length - 1;
            else if (_currentIndex >= options.Length)
                _currentIndex = 0;

            var newValue = options[_currentIndex];
            _setter(newValue);
            OnValueChanged?.Invoke(newValue);
        }
    }
}
