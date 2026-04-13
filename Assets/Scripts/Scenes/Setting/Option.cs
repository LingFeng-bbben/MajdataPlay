using Cysharp.Text;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Settings.OptionEnumerators;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using Topten.RichTextKit.Editor;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using RangeAttribute = MajdataPlay.Settings.RangeAttribute;
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Option : MonoBehaviour
    {
        public int Index { get; set; } 
        public Menu Parent { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object OptionObject { get; set; }

        [SerializeField]
        [FormerlySerializedAs("nameText")]
        TextMeshPro _nameText;
        [SerializeField]
        [FormerlySerializedAs("valueText")]
        TextMeshPro _valueText;
        [SerializeField]
        [FormerlySerializedAs("descriptionText")]
        TextMeshPro _descriptionText;

        bool _isEnabled = false;
        bool _isNum = false;
        bool _isFloat = false;
        bool _isReadOnly = false;
        bool _isPressed = false;
        bool _isUp = false;
        bool _isNoDescription = false;
        float _pressTime = 0;

        string _optionDescription = string.Empty;
        string _optionName = string.Empty;

        float _iterationThrottle = 0;
        int _lastIndex = 0;

        IOptionEnumerator _optionEnumerator;

        public void Init()
        {
            Localization.OnLanguageChanged += OnLangChanged;
            InitOptions();
            _nameText.text = _optionName.i18n();
            if(_isNoDescription)
            {
                _descriptionText.text = string.Empty;
            }
            else
            {
                _descriptionText.text = _optionDescription.i18n();
                switch (PropertyInfo.Name)
                {
                    case "SlideFadeInOffset":
                    case "AudioOffset":
                    case "JudgeOffset":
                    case "AnswerOffset":
                    case "TouchPanelOffset":
                    case "DisplayOffset":
                        _descriptionText.text = _optionDescription.i18n() + $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
                        break;
                }
            }
            UpdatePosition();
            UpdateOption();
        }
        void OnLangChanged(object? sender,Language newLanguage)
        {
            _nameText.text = _optionName.i18n();
            _descriptionText.text = _optionDescription.i18n();
            UpdateOption();
        }
        void InitOptions()
        {
            var type = PropertyInfo.PropertyType;
            _isFloat = type.IsFloatType();
            _isNum = type.IsIntType() || _isFloat;
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
                else if (_isNum)
                {
                    _optionEnumerator = new DefaultNumberEnumerator();
                }
                else // string
                {
                    _optionEnumerator = new DefaultReadOnlyEnumerator();
                }
            }
            _optionEnumerator.Init(PropertyInfo, OptionObject);
        }
        void Update()
        {
            var currentIndex = Parent.SelectedIndex;

            _optionEnumerator.OnUpdate();
            if (currentIndex == Index && _isEnabled && !_isReadOnly)
            {
                var isE4OrB4On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E4, SwitchStatus.On) ||
                                 InputManager.CheckSensorStatusInThisFrame(SensorArea.B4, SwitchStatus.On);
                var isE6OrB5On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E6, SwitchStatus.On) ||
                                 InputManager.CheckSensorStatusInThisFrame(SensorArea.B5, SwitchStatus.On);

                if (_pressTime >= 0.4f)
                {
                    var iterationSpeed = MajEnv.Settings.Debug.MenuOptionIterationSpeed;
                    if (_iterationThrottle <= 1f / (iterationSpeed is 0 ? 15 : iterationSpeed))
                    {
                        _iterationThrottle += MajTimeline.DeltaTime;
                    }
                    else
                    {
                        if (_isUp)
                        {
                            _optionEnumerator.MoveNext();
                        }
                        else
                        {
                            _optionEnumerator.MovePrevious();
                        }
                        _iterationThrottle = 0;
                    }
                }

                if (_isPressed)
                {
                    if(_pressTime < 0.4f)
                    {
                        _pressTime += MajTimeline.DeltaTime;
                    }
                    if(isE4OrB4On)
                    {
                        _isUp = true;
                    }
                    else if(isE6OrB5On)
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
                    if (isE4OrB4On)
                    {
                        _optionEnumerator.MoveNext();
                        _isUp = true;
                        _isPressed = true;
                    }
                    else if (isE6OrB5On)
                    {
                        _optionEnumerator.MovePrevious();
                        _isUp = false;
                        _isPressed = true;
                    }
                }
            }
            UpdateOption();
            if (_lastIndex == currentIndex)
            {
                return;
            }
            _lastIndex = currentIndex;
            UpdatePosition();
        }
        void UpdatePosition()
        {
            var diff = _lastIndex - Index;
            var scale = GetScale(diff);
            var pos = GetPosition(diff);
            transform.localPosition = pos;
            transform.localScale = scale;
        }
        void UpdateOption()
        {
            _valueText.text = _optionEnumerator.LocalizedValueText;
            switch (PropertyInfo.Name)
            {
                case "SlideFadeInOffset":
                case "AudioOffset":
                case "JudgeOffset":
                case "AnswerOffset":
                case "TouchPanelOffset":
                case "DisplayOffset":
                    _descriptionText.text = _optionDescription.i18n() + $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
                    break;
            }
        }
        void OnDestroy()
        {
            _isEnabled = false;
            Localization.OnLanguageChanged -= OnLangChanged;
            _optionEnumerator.Dispose();
        }
        void OnDisable()
        {
            _isEnabled = false;
        }
        void OnEnable()
        {
            _isEnabled = true;
        }
        Vector3 GetScale(int diff)
        {
            switch(diff)
            {
                case 1:
                case -1:
                    return new Vector3(0.6f, 0.6f, 0.6f);
                case 0:
                    return new Vector3(1, 1, 1);
                default:
                    return Vector3.zero;
            }
        }
        Vector3 GetPosition(int diff)
        {
            switch (diff)
            {
                case 1:
                    return new Vector3(-330, 0, 0);
                case -1:
                    return new Vector3(330, 0, 0);
                case 0:
                    return new Vector3(0, 0, 0);
                default:
                    return new Vector3(1000,0,0);
            }
        }
    }
}
