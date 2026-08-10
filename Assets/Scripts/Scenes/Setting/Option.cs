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
        bool _isNoDescription = false;
        bool _isInitialized = false;
        bool _isLocalizationSubscribed = false;

        string _optionDescription = string.Empty;
        string _optionName = string.Empty;

        IOptionEnumerator _optionEnumerator = null!;
        SettingManager _manager = null!;
        InputRepeatState _inputRepeat;

        internal void Init(SettingManager manager, PropertyInfo propertyInfo, object menuInstance)
        {
            _manager = manager;
            PropertyInfo = propertyInfo;
            InitOptions(menuInstance);
            _isInitialized = true;
            SubscribeLocalization();
            _nameTextDisplayer.text = _optionName.i18n();
            RefreshValueText();
        }

        internal void SetSelected(bool isSelected)
        {
            if (_isSelected != isSelected)
            {
                _inputRepeat.SuppressUntilRelease();
            }
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
            _optionEnumerator.RefreshLocalization();
            _nameTextDisplayer.text = _optionName.i18n();
            RefreshValueText();
            if (_isSelected && isActiveAndEnabled)
            {
                SetDescriptionText();
            }
        }
        void InitOptions(object menuInstance)
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
            _optionEnumerator.Init(PropertyInfo, menuInstance);
        }
        internal void HandleInput()
        {
            if (!_isSelected)
            {
                return;
            }

            var moveNextPressed =
                InputManager.CheckSensorStatusInThisFrame(SensorArea.E4, SwitchStatus.On) ||
                InputManager.CheckSensorStatusInThisFrame(SensorArea.B4, SwitchStatus.On) ||
                InputManager.CheckSensorStatusInThisFrame(SensorArea.B3, SwitchStatus.On);
            var movePreviousPressed =
                InputManager.CheckSensorStatusInThisFrame(SensorArea.E6, SwitchStatus.On) ||
                InputManager.CheckSensorStatusInThisFrame(SensorArea.B5, SwitchStatus.On) ||
                InputManager.CheckSensorStatusInThisFrame(SensorArea.B6, SwitchStatus.On);

            if (_inputRepeat.Update(
                moveNextPressed,
                movePreviousPressed,
                MajTimeline.DeltaTime,
                0.4f,
                GetRepeatInterval(),
                out var direction))
            {
                MoveOption(direction > 0);
            }
        }

        static float GetRepeatInterval()
        {
            var iterationSpeed = MajEnv.Settings.Debug.MenuOptionIterationSpeed;
            return 1f / (iterationSpeed is 0 ? 15 : iterationSpeed);
        }

        internal void RefreshEnumerator()
        {
            _optionEnumerator.Refresh();
            RefreshValueText();
        }

        void MoveOption(bool moveNext)
        {
            var hasChanged = moveNext ? _optionEnumerator.MoveNext() : _optionEnumerator.MovePrevious();
            if (hasChanged)
            {
                RefreshEnumerator();
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
                var description = _optionDescription.i18n();
                var isOffsetOption = PropertyInfo.Name is
                    "SlideFadeInOffset" or
                    "AudioOffset" or
                    "JudgeOffset" or
                    "AnswerOffset" or
                    "TouchPanelOffset" or
                    "DisplayOffset";
                if (isOffsetOption)
                {
                    description += $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
                }
                _manager.SetDescriptionText(description);
            }
        }
        void RefreshValueText()
        {
            _valueTextDisplayer.text = _optionEnumerator.LocalizedValueText;
        }
        void OnDestroy()
        {
            UnsubscribeLocalization();
            if (_isInitialized)
            {
                _optionEnumerator.Dispose();
            }
        }
        void OnEnable()
        {
            if (!_isInitialized)
            {
                return;
            }

            SubscribeLocalization();
            _optionEnumerator.Refresh();
            OnLangChanged(null, Localization.Current);
        }
        void OnDisable()
        {
            UnsubscribeLocalization();
            if (_isInitialized)
            {
                _inputRepeat.SuppressUntilRelease();
            }
        }
        void SubscribeLocalization()
        {
            if (_isLocalizationSubscribed || !isActiveAndEnabled)
            {
                return;
            }

            Localization.OnLanguageChanged += OnLangChanged;
            _isLocalizationSubscribed = true;
        }
        void UnsubscribeLocalization()
        {
            if (!_isLocalizationSubscribed)
            {
                return;
            }

            Localization.OnLanguageChanged -= OnLangChanged;
            _isLocalizationSubscribed = false;
        }
    }
}
