﻿using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Collections;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Setting
{
    public class Option : MonoBehaviour
    {
        public int Index { get; set; } 
        public Menu Parent { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object OptionObject { get; set; }

        [SerializeField]
        TextMeshPro nameText;
        [SerializeField]
        TextMeshPro valueText;
        [SerializeField]
        TextMeshPro descriptionText;
        [ReadOnly]
        [SerializeField]
        int _current = 0; // 当前选项的位置
        [ReadOnly]
        [SerializeField]
        object[] _options = Array.Empty<object>(); // 可用的选项

        int _maxOptionIndex = 0;
        float _step = 1;
        bool _isEnabled = false;
        bool _isNum = false;
        bool _isBound = false;
        bool _isFloat = false;
        bool _isReadOnly = false;
        bool _isPressed = false;
        bool _isUp = false;
        float _pressTime = 0;
        float? _maxValue = null;
        float? _minValue = null;

        float _iterationThrottle = 0;
        int _lastIndex = 0;

        AudioManager _audioManager = MajInstances.AudioManager;
        void Start()
        {
            Localization.OnLanguageChanged += OnLangChanged;
            nameText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_TITLE");
            //valueText.text = Localization.GetLocalizedText(PropertyInfo.GetValue(OptionObject).ToString());
            descriptionText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_DESC");
            InitOptions();
            UpdatePosition();
            UpdateOption();
        }
        void OnLangChanged(object? sender,Language newLanguage)
        {
            nameText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_TITLE");
            descriptionText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_DESC");
            UpdateOption();
        }
        void InitOptions()
        {
            var type = PropertyInfo.PropertyType;
            _isFloat = type.IsFloatType();
            _isNum = type.IsIntType() || _isFloat;
            
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                _maxOptionIndex = values.Length - 1;
                _options = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                    _options[i] = values.GetValue(i);
                var obj = PropertyInfo.GetValue(OptionObject);
                _current = _options.FindIndex(x => (int)x == (int)obj);
            }
            else if(type == typeof(bool))
            {
                _options = new object[2] { true,false };
                var obj = PropertyInfo.GetValue(OptionObject);
                _maxOptionIndex = 1;
                _current = (bool)obj ? 0 : 1;
            }
            else if (_isNum)
            {
                switch (PropertyInfo.Name)
                {
                    case "Global":
                        _maxValue = 1;
                        _step = 0.05f;
                        _minValue = 0;
                        break;
                    case "Answer":
                    case "BGM":
                    case "Tap":
                    case "Judge":
                    case "Slide":
                    case "Break":
                    case "Touch":
                    case "Voice":
                        _maxValue = 2;
                        _step = 0.05f;
                        _minValue = 0;
                        break;
                    case "OuterJudgeDistance":
                    case "InnerJudgeDistance":
                    case "BackgroundDim":
                        _maxValue = 1;
                        _step = 0.05f;
                        _minValue = 0;
                        break;
                    case "TouchSpeed":
                    case "TapSpeed":
                        _maxValue = null;
                        _minValue = null;
                        _step = 0.25f;
                        break;
                    case "Rotation":
                        _maxValue = 7;
                        _minValue = -7;
                        _step = 1;
                        break;
                    case "PlaybackSpeed":
                        _minValue = 0;
                        _step = 0.01f;
                        break;
                    case "FPSLimit":
                        _minValue = -1;
                        _step = 1;
                        break;
                    case "Direct3DMaxQueuedFrames":
                        _minValue = 0;
                        _step = 1;
                        break;
                    case "TapScale":
                    case "HoldScale":
                    case "TouchScale":
                    case "SlideScale":
                        _maxValue = 2;
                        _minValue = 0;
                        _step = 0.01f;
                        break;
                    default:
                        _maxValue = null;
                        _minValue = null;
                        _step = 0.001f;
                        break;
                }
            }
            else // string
            {
                switch (PropertyInfo.Name)
                {
                    case "Resolution":
                        _isReadOnly = true;
                        break;
                    case "Skin":
                        var skinManager = MajInstances.SkinManager;
                        var skinNames = skinManager.LoadedSkins.Select(x => x.Name)
                                                               .ToArray();
                        var currentSkin = skinManager.SelectedSkin;
                        _options = skinNames;
                        _maxOptionIndex = _options.Length - 1;
                        _current = skinNames.FindIndex(x => x == currentSkin.Name);
                        break;
                    case "Language":
                        var availableLangs = Localization.Available;
                        if (availableLangs.IsEmpty())
                        {
                            _current = 0;
                            _options = new object[] { "Unavailable" };
                            _maxOptionIndex = 0;
                            _isReadOnly = true;
                            PropertyInfo.SetValue(OptionObject, "Unavailable");
                            return;
                        }
                        var langNames = availableLangs.Select(x => x.ToString())
                                                      .ToArray();
                        var currentLang = Localization.Current;
                        _options = langNames;
                        _maxOptionIndex = _options.Length - 1;
                        _current = availableLangs.FindIndex(x => x == currentLang);
                        break;
                    case "NoteMask":
                        _options = new string[3]
                        {
                            "Disable",
                            "Inner",
                            "Outer"
                        };
                        var current = PropertyInfo.GetValue(OptionObject);
                        _maxOptionIndex = _options.Length - 1;
                        _current = _options.FindIndex(x => x == current);
                        break;
                }
            }
        }
        void Update()
        {
            if (_pressTime >= 0.4f)
            {
                if(_iterationThrottle <= 1 / 15f)
                {
                    _iterationThrottle += MajTimeline.DeltaTime;
                }
                else
                {
                    if (_isUp)
                    {
                        Up();
                    }
                    else
                    {
                        Down();
                    }
                    _iterationThrottle = 0;
                }
            }
            else if (_isPressed)
            {
                _pressTime += MajTimeline.DeltaTime;
            }
            var currentIndex = Parent.SelectedIndex;
            

            if (currentIndex == Index && _isEnabled && !_isReadOnly)
            {
                var isE4OrB4On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E4, SensorStatus.On) ||
                                 InputManager.CheckSensorStatusInThisFrame(SensorArea.B4, SensorStatus.On);
                var isE6OrB5On = InputManager.CheckSensorStatusInThisFrame(SensorArea.E6, SensorStatus.On) ||
                                 InputManager.CheckSensorStatusInThisFrame(SensorArea.B5, SensorStatus.On);

                if(_isPressed)
                {
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
                        Up();
                        _isUp = true;
                        _isPressed = true;
                    }
                    else if (isE6OrB5On)
                    {
                        Down();
                        _isUp = false;
                        _isPressed = true;
                    }
                }
            }
            
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
            var value = PropertyInfo.GetValue(OptionObject);
            var origin = value.ToString();
            string localizedText;
            switch (PropertyInfo.Name)
            {
                case "OuterJudgeDistance":
                case "InnerJudgeDistance":
                    if((float)value == 0)
                        localizedText = Localization.GetLocalizedText("OFF");
                    else
                        localizedText = Localization.GetLocalizedText(origin);
                    break;
                default:
                    if(!_isNum)
                    {
                        if (!$"{PropertyInfo.Name}_MAJSETTING_OPTIONS_{origin}".Tryi18n(out localizedText))
                        {
                            localizedText = origin.i18n();
                        }
                    }
                    else
                    {
                        localizedText = origin;
                    }
                    //localizedText = Localization.GetLocalizedText(origin);
                    break;
            }
            valueText.text = localizedText;
            switch (PropertyInfo.Name)
            {
                case "Global":
                case "Answer":
                case "BGM":
                case "Tap":
                case "Judge":
                case "Slide":
                case "Break":
                case "Touch":
                case "Voice":
                    UpdateVolume();
                    break;
                case "VSync":
                    QualitySettings.vSyncCount = (bool)value ? 1 : 0;
                    break;
                case "FPSLimit":
                    Application.targetFrameRate = (int)value;
                    break;
                case "RenderQuality":
                    QualitySettings.SetQualityLevel((int)value, true);
                    break;
                case "Direct3DMaxQueuedFrames":
                    QualitySettings.maxQueuedFrames = (int)value;
                    break;
            }
        }
        void UpdateVolume()
        {
            _audioManager.ReadVolumeFromSettings();
        }
        void Up()
        {
            Diff(1);
            UpdateOption();
        }
        void Down()
        {
            Diff(-1);
            UpdateOption();
        }
        void Diff(int num)
        {
            num = num.Clamp(-1, 1);
            if (_isNum) // 数值类
            {
                var valueObj = PropertyInfo.GetValue(OptionObject);
                var valueType = valueObj.GetType();
                var value = Convert.ToSingle(valueObj);
                value += _step * num;
                value = MathF.Round(value, 3);
                if (_maxValue is not null) //有上限
                    value = Math.Min(value, (float)_maxValue);
                if(_minValue is not null)
                    value = Math.Max(value, (float)_minValue);
                PropertyInfo.SetValue(OptionObject,Convert.ChangeType(value,valueType));
            }
            else //非数值类
            {
                _current += 1 * num;
                if (_current < 0) _current = _maxOptionIndex;
                if (_current>_maxOptionIndex) _current = 0;
                PropertyInfo.SetValue(OptionObject, _options[_current]);
                switch (PropertyInfo.Name)
                {
                    case "Skin":
                        var skins = MajInstances.SkinManager.LoadedSkins;
                        var newSkin = skins.Find(x => x.Name == _options[_current].ToString());
                        MajInstances.SkinManager.SelectedSkin = newSkin;
                        break;
                    case "Language":
                        Localization.SetLang((string)_options[_current]);
                        break;
                }
            }
        }
        void OnDestroy()
        {
            _isEnabled = false;
            Localization.OnLanguageChanged -= OnLangChanged;
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
