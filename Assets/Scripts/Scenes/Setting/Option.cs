using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using MajdataPlay.Settings.SettingItems;
using System;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Option : MonoBehaviour
    {
        public int Index { get; set; }
        public Menu Parent { get; set; }
        public ISettingItem SettingItem { get; set; }

        [SerializeField]
        TextMeshPro nameText;
        [SerializeField]
        TextMeshPro valueText;
        [SerializeField]
        TextMeshPro descriptionText;

        bool _isEnabled = false;
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
            UpdateText();
            UpdatePosition();
            UpdateOption();

            // 注册值改变回调
            SettingItem.OnValueChanged += OnSettingValueChanged;
        }

        void OnSettingValueChanged(object newValue)
        {
            HandleSpecialCases();
        }

        void HandleSpecialCases()
        {
            // 处理特殊的设置项逻辑
            switch (SettingItem.Name)
            {
                case "OffsetUnit":
                    HandleOffsetUnitChanged();
                    break;
            }
        }

        void HandleOffsetUnitChanged()
        {
            var oldValue = MajEnv.Settings.Debug.OffsetUnit;
            if (oldValue == OffsetUnitOption.Second)
            {
                // 转换为秒
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
                // 转换为帧
                MajEnv.Settings.Judge.AudioOffset = MathF.Round(MajEnv.Settings.Judge.AudioOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                MajEnv.Settings.Judge.JudgeOffset = MathF.Round(MajEnv.Settings.Judge.JudgeOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                MajEnv.Settings.Judge.TouchPanelOffset = MathF.Round(MajEnv.Settings.Judge.TouchPanelOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                MajEnv.Settings.Judge.AnswerOffset = MathF.Round(MajEnv.Settings.Judge.AnswerOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                MajEnv.Settings.Game.SlideFadeInOffset = MathF.Round(MajEnv.Settings.Game.SlideFadeInOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                MajEnv.Settings.Debug.DisplayOffset = MathF.Round(MajEnv.Settings.Debug.DisplayOffset / MajEnv.FRAME_LENGTH_SEC, 1);
                ChartSettingStorage.ConvertUnitToFrame();
            }
        }

        void OnLangChanged(object? sender, Language newLanguage)
        {
            UpdateText();
            UpdateOption();
        }

        void UpdateText()
        {
            nameText.text = $"MAJSETTING_PROPERTY_{SettingItem.Name}".i18n();
            descriptionText.text = $"MAJSETTING_PROPERTY_{SettingItem.Name}_DESC".i18n();

            // 添加偏移单位提示
            switch (SettingItem.Name)
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

        void Update()
        {
            var currentIndex = Parent.SelectedIndex;

            if (currentIndex == Index && _isEnabled && !SettingItem.IsReadOnly)
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
            var valueString = SettingItem.GetValueString();
            string localizedText;

            // 特殊显示处理
            switch (SettingItem.Name)
            {
                case "OuterJudgeDistance":
                case "InnerJudgeDistance":
                    if (SettingItem.GetValue() is float f && f == 0)
                    {
                        localizedText = "OFF".i18n();
                    }
                    else
                    {
                        localizedText = valueString.i18n();
                    }
                    break;
                default:
                    if (!SettingItem.IsNumeric)
                    {
                        if (!$"MAJSETTING_PROPERTY_{SettingItem.Name}_OPTION_{valueString}".Tryi18n(out localizedText))
                        {
                            localizedText = valueString.i18n();
                        }
                    }
                    else
                    {
                        localizedText = valueString;
                    }
                    break;
            }
            valueText.text = localizedText;
            UpdateText();
        }

        void Up()
        {
            SettingItem.ModifyValue(1);
            UpdateOption();
        }

        void Down()
        {
            SettingItem.ModifyValue(-1);
            UpdateOption();
        }

        void OnDestroy()
        {
            _isEnabled = false;
            Localization.OnLanguageChanged -= OnLangChanged;
            if (SettingItem != null)
                SettingItem.OnValueChanged -= OnSettingValueChanged;
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
