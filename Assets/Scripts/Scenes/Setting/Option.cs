using MajdataPlay.Diagnostics;
using MajdataPlay.Extensions;
using MajdataPlay.i18n;
using MajdataPlay.IO;
using MajdataPlay.Settings;
using MajdataPlay.Settings.OptionEnumerators;
using MajdataPlay.Utils;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Option : MonoBehaviour
    {
        internal PropertyInfo PropertyInfo { get; private set; } = null!;

        [SerializeField]
        [FormerlySerializedAs("nameText")]
        TextMeshProUGUI _nameTextDisplayer = null!;

        [SerializeField]
        [FormerlySerializedAs("valueText")]
        TextMeshProUGUI _valueTextDisplayer = null!;


        bool _isSelected = false;
        bool _isPressed = false;
        bool _isUp = false;
        bool _isNoDescription = false;
        float _pressTime = 0;

        string _optionDescription = string.Empty;
        string _optionName = string.Empty;

        float _iterationThrottle = 0;

        IOptionEnumerator _optionEnumerator = null!;
        SettingManager _manager = null!;
        object _menuInstance = null!;

        internal void Init(SettingManager manager, PropertyInfo propertyInfo, object menuInstance)
        {
            _manager = manager;
            PropertyInfo = propertyInfo;
            _menuInstance = menuInstance;
            InitOptions();
            Localization.OnLanguageChanged += OnLangChanged;
            _nameTextDisplayer.text = _optionName.i18n();
            RefreshValueText();
        }

        internal void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            if (isSelected)
            {
                SetDescriptionText();
            }
        }
        internal void SetTextColor(Color newColor)
        {
            _nameTextDisplayer.color = newColor;
            _valueTextDisplayer.color = newColor;
        }

        void OnLangChanged(object? sender,Language newLanguage)
        {
            _nameTextDisplayer.text = _optionName.i18n();
            RefreshValueText();
            if (_isSelected && isActiveAndEnabled)
            {
                SetDescriptionText();
            }
        }
        void InitOptions()
        {
            var type = PropertyInfo.PropertyType;
            var isNum = type.IsIntType() || type.IsFloatType();
            _isNoDescription = PropertyInfo.GetCustomAttribute<NoDescriptionAttribute>() is not null;
            var optionNameAttr = PropertyInfo.GetCustomAttribute<OptionNameAttribute>();
            var optionDescriptionAttr = PropertyInfo.GetCustomAttribute<DescriptionAttribute>();
            var optionEnumeratorAttr = PropertyInfo.GetCustomAttribute<OptionEnumeratorAttribute>();
            _optionName = optionNameAttr?.Name ?? $"MAJSETTING_PROPERTY_{PropertyInfo.Name}";

            if(optionDescriptionAttr is not null)
            {
                _optionDescription = optionDescriptionAttr.Text;
            }
            else
            {
                _optionDescription = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}_DESC";
            }

            if(optionEnumeratorAttr is not null)
            {
                var enumerator = default(IOptionEnumerator?);
                try
                {
                    enumerator = optionEnumeratorAttr.Instance();
                }
                catch(Exception e)
                {
                    MajDebug.LogWarning($"[SettingUI]Failed to instantiate IOptionEnumerator specified by Attribute\nType: {optionEnumeratorAttr.EnumeratorType}\nException: {e}");
                }
                if(enumerator is null)
                {
                    MajDebug.LogWarning($"[SettingUI]Failed to instantiate IOptionEnumerator specified by Attribute\nType: {optionEnumeratorAttr.EnumeratorType}");
                    _optionEnumerator = new DefaultReadOnlyEnumerator();
                }
                else
                {
                    _optionEnumerator = enumerator;
                }
            }
            else
            {
                if (type.IsEnum)
                {
                    _optionEnumerator = new DefaultEnumEnumerator();
                }
                else if (type == typeof(bool) || type == typeof(bool?))
                {
                    _optionEnumerator = new DefaultBooleanEnumerator();
                }
                else if (isNum)
                {
                    _optionEnumerator = new DefaultNumberEnumerator();
                }
                else // string
                {
                    _optionEnumerator = new DefaultReadOnlyEnumerator();
                }
            }
            _optionEnumerator.Init(PropertyInfo, _menuInstance);
        }
        void Update()
        {
            _optionEnumerator.OnUpdate();
            if (_isSelected)
            {
                var isE4OrB4OrB3On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E4, SwitchStatus.On) ||
                                     InputManager.CheckSensorStatusInThisFrame(SensorArea.B4, SwitchStatus.On) ||
                                     InputManager.CheckSensorStatusInThisFrame(SensorArea.B3, SwitchStatus.On);
                var isE6OrB5OrB6On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E6, SwitchStatus.On) ||
                                     InputManager.CheckSensorStatusInThisFrame(SensorArea.B5, SwitchStatus.On) ||
                                     InputManager.CheckSensorStatusInThisFrame(SensorArea.B6, SwitchStatus.On);

                if (_pressTime >= 0.4f)
                {
                    var iterationSpeed = MajEnv.Settings.Debug.MenuOptionIterationSpeed;
                    if (_iterationThrottle <= 1f / (iterationSpeed is 0 ? 15 : iterationSpeed))
                    {
                        _iterationThrottle += MajTimeline.DeltaTime;
                    }
                    else
                    {
                        MoveOption(_isUp);
                        _iterationThrottle = 0;
                    }
                }

                if (_isPressed)
                {
                    if(_pressTime < 0.4f)
                    {
                        _pressTime += MajTimeline.DeltaTime;
                    }
                    if(isE4OrB4OrB3On)
                    {
                        _isUp = true;
                    }
                    else if(isE6OrB5OrB6On)
                    {
                        _isUp = false;
                    }
                    else
                    {
                        _isPressed = false;
                        _pressTime = 0;
                    }
                }
                else
                {
                    if (isE4OrB4OrB3On)
                    {
                        MoveOption(true);
                        _isUp = true;
                        _isPressed = true;
                    }
                    else if (isE6OrB5OrB6On)
                    {
                        MoveOption(false);
                        _isUp = false;
                        _isPressed = true;
                    }
                }
            }
        }

        void MoveOption(bool moveNext)
        {
            var hasChanged = moveNext ? _optionEnumerator.MoveNext() : _optionEnumerator.MovePrevious();
            if (hasChanged)
            {
                RefreshValueText();
            }
        }
        void SetDescriptionText()
        {
            if (_isNoDescription)
            {
                _manager.SetDescriptionText(string.Empty);
            }
            else
            {
                _manager.SetDescriptionText(_optionDescription.i18n());
                switch (PropertyInfo.Name)
                {
                    case "SlideFadeInOffset":
                    case "AudioOffset":
                    case "JudgeOffset":
                    case "AnswerOffset":
                    case "TouchPanelOffset":
                    case "DisplayOffset":
                        _manager.SetDescriptionText(_optionDescription.i18n() + $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}");
                        break;
                }
            }
        }
        void RefreshValueText()
        {
            _valueTextDisplayer.text = _optionEnumerator.LocalizedValueText;
        }
        void OnDestroy()
        {
            Localization.OnLanguageChanged -= OnLangChanged;
            _optionEnumerator.Dispose();
        }
    }
}
