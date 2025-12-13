using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Collections;
using UnityEngine;
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
        decimal _step = 1;
        bool _isEnabled = false;
        bool _isNum = false;
        bool _isBound = false;
        bool _isFloat = false;
        bool _isReadOnly = false;
        bool _isPressed = false;
        bool _isUp = false;
        float _pressTime = 0;
        decimal? _maxValue = null;
        decimal? _minValue = null;

        float _iterationThrottle = 0;
        int _lastIndex = 0;

        AudioManager _audioManager = MajInstances.AudioManager;
        public void Init()
        {
            Localization.OnLanguageChanged += OnLangChanged;
            nameText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}".i18n();
            descriptionText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}_DESC".i18n();
            switch (PropertyInfo.Name)
            {
                case "SlideFadeInOffset":
                case "AudioOffset":
                case "JudgeOffset":
                case "AnswerOffset":
                case "TouchPanelOffset":
                case "DisplayOffset":
                    descriptionText.text += $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
                    break;
            }
            InitOptions();
            UpdatePosition();
            UpdateOption();
        }
        void OnLangChanged(object? sender,Language newLanguage)
        {
            nameText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}".i18n();
            descriptionText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}_DESC".i18n();
            switch (PropertyInfo.Name)
            {
                case "SlideFadeInOffset":
                case "AudioOffset":
                case "JudgeOffset":
                case "AnswerOffset":
                case "TouchPanelOffset":
                case "DisplayOffset":
                    descriptionText.text += $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
                    break;
            }
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
                {
                    _options[i] = values.GetValue(i);
                }
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
                        _step = 0.05m;
                        _minValue = 0;
                        break;
                    case "Answer":
                    case "BGM":
                    case "Track":
                    case "Tap":
                    case "Judge":
                    case "Slide":
                    case "Break":
                    case "Touch":
                    case "Voice":
                        _maxValue = 2;
                        _step = 0.05m;
                        _minValue = 0;
                        break;
                    case "TrackVolumeOffset":
                        _maxValue = 2;
                        _step = 0.05m;
                        _minValue = -2;
                        break;
                    case "OuterJudgeDistance":
                    case "InnerJudgeDistance":
                    case "BackgroundDim":
                        _maxValue = 1;
                        _step = 0.05m;
                        _minValue = 0;
                        break;
                    case "TouchSpeed":
                    case "TapSpeed":
                        _maxValue = null;
                        _minValue = null;
                        _step = 0.25m;
                        break;
                    case "Rotation":
                        _maxValue = 7;
                        _minValue = -7;
                        _step = 1;
                        break;
                    case "PlaybackSpeed":
                        _minValue = 0;
                        _step = 0.01m;
                        break;
                    case "FPSLimit":
                        _minValue = -1;
                        _step = 1;
                        break;
                    case "MenuOptionIterationSpeed":
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
                        _step = 0.01m;
                        break;
                    case "AudioOffset":
                    case "JudgeOffset":
                    case "AnswerOffset":
                    case "TouchPanelOffset":
                        {
                            if(MajEnv.Settings.Debug.OffsetUnit == Settings.OffsetUnitOption.Second)
                            {
                                goto default;
                            }
                            else
                            {
                                _maxValue = null;
                                _minValue = null;
                                _step = 0.1m;
                            }
                        }
                        break;
                    case "DisplayOffset":
                        {
                            if (MajEnv.Settings.Debug.OffsetUnit == Settings.OffsetUnitOption.Second)
                            {
                                _maxValue = null;
                                _minValue = 0;
                                _step = 0.001m;
                            }
                            else
                            {
                                _maxValue = null;
                                _minValue = 0;
                                _step = 0.1m;
                            }
                        }
                        break;
                    default:
                        _maxValue = null;
                        _minValue = null;
                        if(type.IsIntType())
                        {
                            _step = 1m;
                        }
                        else
                        {
                            _step = 0.001m;
                        }
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
            var currentIndex = Parent.SelectedIndex;
            

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
                            Up();
                        }
                        else
                        {
                            Down();
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
            var value = PropertyInfo.GetValue(OptionObject);
            var origin = value.ToString();
            string localizedText;
            switch (PropertyInfo.Name)
            {
                case "AudioOffset":
                case "JudgeOffset":
                case "AnswerOffset":
                case "TouchPanelOffset":
                    {
                        if (MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
                        {
                            _maxValue = null;
                            _minValue = null;
                            _step = 0.001m;
                        }
                        else
                        {
                            _maxValue = null;
                            _minValue = null;
                            _step = 0.1m;
                        }
                        goto default;
                    }
                case "DisplayOffset":
                    {
                        if (MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
                        {
                            _maxValue = null;
                            _minValue = 0;
                            _step = 0.001m;
                        }
                        else
                        {
                            _maxValue = null;
                            _minValue = 0;
                            _step = 0.1m;
                        }
                        goto default;
                    }
                case "OuterJudgeDistance":
                case "InnerJudgeDistance":
                    if((float)value == 0)
                    {
                        localizedText = "OFF".i18n();
                    }
                    else
                    {
                        localizedText = origin.i18n();
                    }
                    break;
                default:
                    if(!_isNum)
                    {
                        if (!$"MAJSETTING_PROPERTY_{PropertyInfo.Name}_OPTION_{origin}".Tryi18n(out localizedText))
                        {
                            localizedText = origin.i18n();
                        }
                    }
                    else
                    {
                        localizedText = origin;
                    }
                    break;
            }
            valueText.text = localizedText;
            nameText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}".i18n();
            descriptionText.text = $"MAJSETTING_PROPERTY_{PropertyInfo.Name}_DESC".i18n();
            switch (PropertyInfo.Name)
            {
                case "SlideFadeInOffset":
                case "AudioOffset":
                case "JudgeOffset":
                case "AnswerOffset":
                case "TouchPanelOffset":
                case "DisplayOffset":
                    descriptionText.text += $"\n{$"MAJTEXT_SETTING_OFFSETUNIT_{MajEnv.Settings.Debug.OffsetUnit}".i18n()}";
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
                var oldValue = Convert.ToDecimal(valueObj);
                var newValue = Math.Round(oldValue + _step * num, 3);
                
                if (_maxValue is not null) //有上限
                {
                    newValue = Math.Min(newValue, (decimal)_maxValue);
                }
                if(_minValue is not null)
                {
                    newValue = Math.Max(newValue, (decimal)_minValue);
                }
                OnValueChanged(oldValue, newValue);
                PropertyInfo.SetValue(OptionObject, Convert.ChangeType(newValue, valueType));
            }
            else //非数值类
            {
                _current += 1 * num;
                if (_current < 0)
                {
                    _current = _maxOptionIndex;
                }
                if (_current>_maxOptionIndex)
                {
                    _current = 0;
                }
                var oldValue = PropertyInfo.GetValue(OptionObject);
                var newValue = _options[_current];

                OnValueChanged(oldValue, newValue);
                PropertyInfo.SetValue(OptionObject, newValue);
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
        void OnValueChanged(object oldValue,object newValue)
        {
            switch(PropertyInfo.Name)
            {
                case "OffsetUnit":
                    {
                        var eOldValue = (OffsetUnitOption)oldValue;
                        var eNewValue = (OffsetUnitOption)newValue;
                        if (eOldValue == eNewValue)
                        {
                            return;
                        }
                        else if(eNewValue == OffsetUnitOption.Second)
                        {
                            MajEnv.Settings.Judge.AudioOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.AudioOffset, 3); 
                            MajEnv.Settings.Judge.JudgeOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.JudgeOffset, 3);
                            MajEnv.Settings.Judge.TouchPanelOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.TouchPanelOffset, 3); 
                            MajEnv.Settings.Judge.AnswerOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.AnswerOffset, 3); 
                            MajEnv.Settings.Game.SlideFadeInOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Game.SlideFadeInOffset, 3); 
                            MajEnv.Settings.Debug.DisplayOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Debug.DisplayOffset, 3);
                            ChartSettingStorage.ConvertUnitToSecond();
                        }
                        else
                        {
                            MajEnv.Settings.Judge.AudioOffset = MathF.Round(MajEnv.Settings.Judge.AudioOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            MajEnv.Settings.Judge.JudgeOffset = MathF.Round(MajEnv.Settings.Judge.JudgeOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            MajEnv.Settings.Judge.TouchPanelOffset = MathF.Round(MajEnv.Settings.Judge.TouchPanelOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            MajEnv.Settings.Judge.AnswerOffset = MathF.Round(MajEnv.Settings.Judge.AnswerOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            MajEnv.Settings.Game.SlideFadeInOffset = MathF.Round(MajEnv.Settings.Game.SlideFadeInOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            MajEnv.Settings.Debug.DisplayOffset = MathF.Round(MajEnv.Settings.Debug.DisplayOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                            ChartSettingStorage.ConvertUnitToFrame();
                        }
                    }
                    break;
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
                    QualitySettings.vSyncCount = Convert.ToBoolean(newValue) ? 1 : 0;
                    break;
                case "FPSLimit":
                    Application.targetFrameRate = Convert.ToInt32(newValue);
                    break;
                case "RenderQuality":
                    QualitySettings.SetQualityLevel(Convert.ToInt32(newValue), true);
                    break;
                case "Direct3DMaxQueuedFrames":
                    QualitySettings.maxQueuedFrames = Convert.ToInt32(newValue);
                    break;
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
