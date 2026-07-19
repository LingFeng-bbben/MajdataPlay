using Cysharp.Text;
using LitMotion;
using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.i18n;
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
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Option : MonoBehaviour
    {
        [field: SerializeField, ReadOnlyField]
        public int Index { get; set; } 
        public Menu Parent { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object MenuInstance { get; set; }

        [SerializeField]
        [FormerlySerializedAs("nameText")]
        TextMeshProUGUI _nameTextDisplayer;

        [SerializeField]
        [FormerlySerializedAs("valueText")]
        TextMeshProUGUI _valueTextDisplayer;


        bool _isEnabled = false;
        bool _isSelected = false;
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

        [SerializeField]
        [ReadOnlyField]
        int _currentIndex = 0;

        [SerializeField]
        [ReadOnlyField]
        float _indexProgress = 0f;

        IOptionEnumerator _optionEnumerator;

        SettingManager _manager;

        MotionHandle _optionAnim;

        void Awake()
        {
            _manager = FindAnyObjectByType<SettingManager>();
        }
        public void Init()
        {
            Localization.OnLanguageChanged += OnLangChanged;
            InitOptions();
            _nameTextDisplayer.text = _optionName.i18n();
            SetDescriptionText();
            UpdateOption();
        }

        internal void SetAsSelected()
        {
            _isSelected = true;
            SetDescriptionText();
        }
        internal void SetAsUnselected()
        {
            _isSelected = false;
        }
        internal void SetTextColor(Color newColor)
        {
            _nameTextDisplayer.color = newColor;
            _valueTextDisplayer.color = newColor;
        }

        void OnLangChanged(object? sender,Language newLanguage)
        {
            _nameTextDisplayer.text = _optionName.i18n();
            _manager.SetDescriptionText(_optionDescription.i18n());
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
            _optionEnumerator.Init(PropertyInfo, MenuInstance);
        }
        void Update()
        {
            _optionEnumerator.OnUpdate();
            if (_isSelected && _isEnabled && !_isReadOnly)
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
        void UpdateOption()
        {
            _valueTextDisplayer.text = _optionEnumerator.LocalizedValueText;
            if(_isSelected)
            {
                SetDescriptionText();
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
            _optionAnim.TryCancel();
        }
        void OnEnable()
        {
            _isEnabled = true;
        }
    }
}
