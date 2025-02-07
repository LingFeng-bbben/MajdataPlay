using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
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
        bool _isNum = false;
        bool _isBound = false;
        bool _isFloat = false;
        bool _isReadOnly = false;
        bool _isPressed = false;
        bool _isUp = false;
        float _pressTime = 0;
        float? _maxValue = null;
        float? _minValue = null;

        int _lastIndex = 0;

        AudioManager _audioManager = MajInstances.AudioManager;
        void Start()
        {
            Localization.OnLanguageChanged += OnLangChanged;
            nameText.text = Localization.GetLocalizedText(PropertyInfo.Name);
            //valueText.text = Localization.GetLocalizedText(PropertyInfo.GetValue(OptionObject).ToString());
            descriptionText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_DESC");
            InitOptions();
            UpdatePosition();
            UpdateOption();

            if (Parent.SelectedIndex == Index)
                BindArea();
            else
                UnbindArea();
        }
        void OnLangChanged(object? sender,Language newLanguage)
        {
            nameText.text = Localization.GetLocalizedText(PropertyInfo.Name);
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
                        _step = 0.1f;
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
                if (_isUp)
                    Up();
                else
                    Down();
            }
            else if (_isPressed)
                _pressTime += Time.deltaTime;
            var currentIndex = Parent.SelectedIndex;
            if (_lastIndex == currentIndex)
                return;

            if (currentIndex == Index)
                BindArea();
            else
                UnbindArea();
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
            var origin = PropertyInfo.GetValue(OptionObject).ToString();
            var localizedText = string.Empty;
            switch (PropertyInfo.Name)
            {
                case "OuterJudgeDistance":
                case "InnerJudgeDistance":
                    if((float)PropertyInfo.GetValue(OptionObject) == 0)
                        localizedText = Localization.GetLocalizedText("OFF");
                    else
                        localizedText = Localization.GetLocalizedText(origin);
                    break;
                default:
                    localizedText = Localization.GetLocalizedText(origin);
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
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (_isReadOnly)
                return;
            else if (e.IsButton)
                return;
            var on = e.Status == SensorStatus.On;
            var canTrigger = on && !_isPressed;
            switch (e.Type)
            {
                case SensorType.E4:
                case SensorType.B4:

                    if (canTrigger)
                    {
                        Up();
                        _isUp = true;
                        _isPressed = true;
                    }
                    else if(!on)
                    {
                        _isPressed = false;
                        _pressTime = 0;
                    }
                    break;
                case SensorType.E6:
                case SensorType.B5:
                    if (canTrigger)
                    {
                        Down();
                        _isUp = false;
                        _isPressed = true;
                    }
                    else if (!on)
                    {
                        _isPressed = false;
                        _pressTime = 0;
                    }
                    break;
            }
        }
        void OnDestroy()
        {
            UnbindArea();
            Localization.OnLanguageChanged -= OnLangChanged;
        }
        void OnDisable()
        {
            UnbindArea();
        }
        void OnEnable()
        {
            BindArea();
        }
        void BindArea()
        {
            if (_isBound)
                return;
            else if(_isReadOnly)
            {
                _isBound = true;
                return;
            }
            else if (Parent == null)
                return;
            else if (Parent.SelectedIndex != Index)
                return;
            _isBound = true;
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.E4);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.E6);
        }
        void UnbindArea()
        {
            if (!_isBound)
                return;
            _isPressed = false;
            _pressTime = 0;
            _isBound = false;
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E6);
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
