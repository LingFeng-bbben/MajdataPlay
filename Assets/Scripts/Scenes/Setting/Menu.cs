using LitMotion;
using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Menu : MonoBehaviour
    {
        [field: SerializeField, ReadOnlyField]
        public string Name { get; set; } = string.Empty;

        [field: SerializeField, ReadOnlyField]
        public int SelectedIndex { get; private set; }
        /// <summary>
        /// Option对象<para>e.g. GameSetting.Game</para>
        /// </summary>
        public object Instance { get; set; }

        [SerializeField]
        [FormerlySerializedAs("optionPrefab")]
        GameObject _optionPrefab;

        SettingManager _manager;

        [SerializeField, ReadOnlyField]
        float _listCursorPos = 0;

        float _lastWaitTime = 0;
        Option[] _options = Array.Empty<Option>();

        MotionHandle _optionAnim;

        readonly SettingConfig _settingConfig = MajEnv.RuntimeConfig?.Setting ?? new();
        void Awake()
        {
            _manager = FindAnyObjectByType<SettingManager>();
        }
        public void Init()
        {
            var type = Instance.GetType();
            var properties = type.GetProperties()
                                 .Where(x => !x.GetCustomAttributes<HideInSettingUIAttribute>().Any())
                                 .ToArray();
            _options = new Option[properties.Length];
            foreach(var (i,property) in properties.WithIndex())
            {
                var optionObj = Instantiate(_optionPrefab, transform);
                var option = optionObj.GetComponent<Option>();
                _options[i] = option;
                option.PropertyInfo = property;
                option.MenuInstance = Instance;
                option.Parent = this;
                option.Index = i;
                option.Init();
            }

            UpdateDisplayerPosition();
        }
        void OnDisable()
        {
            SelectedIndex = 0;
        }
        void OnDestroy()
        {
            _optionAnim.TryCancel();
        }
        void Update()
        {
            if(_manager.IsPressed && _manager.PressTime != 0)
            {
                if (_manager.PressTime < 0.7f)
                {
                    return;
                }
                else if (_lastWaitTime < 0.2f)
                {
                    _lastWaitTime += Time.deltaTime;
                    return;
                }
                switch(_manager.Direction)
                {
                    case 1:
                        NextOption();
                        _lastWaitTime = 0;
                        break;
                    case -1:
                        PreviousOption();
                        _lastWaitTime = 0;
                        break;
                }
            }
            else
            {
                _lastWaitTime = 0;
                if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A6))
                {
                    PreviousOption();
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A3))
                {
                    NextOption();
                }
            }
        }
        void PreviousOption()
        {
            SelectedIndex--;
            if (SelectedIndex < 0)
            {
                _manager.PreviousMenu();
            }
            SelectedIndex = SelectedIndex.Clamp(0, _options.Length - 1);
            DisplayerMoveTo(SelectedIndex, 0.3f);
            _settingConfig.SelectedOption = _options[SelectedIndex].PropertyInfo.Name;
        }
        void NextOption()
        {
            SelectedIndex++;
            if (SelectedIndex > _options.Length - 1)
            {
                _manager.NextMenu();
            }
            SelectedIndex = SelectedIndex.Clamp(0, _options.Length - 1);
            DisplayerMoveTo(SelectedIndex, 0.3f);
            _settingConfig.SelectedOption = _options[SelectedIndex].PropertyInfo.Name;
        }

        void DisplayerMoveTo(float targetPos, float duration)
        {
            _optionAnim.TryCancel();
            UpdateOptionSelectionState();
            _optionAnim = LMotion.Create(_listCursorPos, targetPos, duration)
                                     .WithScheduler(MotionScheduler.PostLateUpdate)
                                     .WithEase(Ease.OutQuad)
                                     .Bind(x =>
                                     {
                                         _listCursorPos = x;
                                         UpdateDisplayerPosition();
                                     });
        }
        void UpdateDisplayerPosition()
        {
            for (var i = 0; i < _options.Length; i++)
            {
                var distance = i - _listCursorPos;
                var optionDisplayer = _options[i];

                optionDisplayer.transform.localPosition = GetOptionTransformPosition(distance);
                optionDisplayer.transform.localScale = GetOptionTransformScale(distance);
                optionDisplayer.SetTextColor(GetOptionTextColor(distance));
            }
        }
        void UpdateOptionSelectionState()
        {
            for (var i = 0; i < _options.Length; i++)
            {
                var optionDisplayer = _options[i];
                if(i == SelectedIndex)
                {
                    optionDisplayer.SetAsSelected();
                }
                else
                {
                    optionDisplayer.SetAsUnselected();
                }
            }
        }

        public void Hide()
        {
            for (var i = 0; i < _options.Length; i++)
            {
                var option = _options[i];
                option.gameObject.SetActive(false);
            }
        }
        public void Show()
        {
            for (var i = 0; i < _options.Length; i++)
            {
                var option = _options[i];
                option.gameObject.SetActive(true);
            }
        }
        public void ToTail()
        {
            SelectedIndex = _options.Length - 1;
            DisplayerMoveTo(SelectedIndex, 0f);
        }
        public void ToHead()
        {
            SelectedIndex = 0;
            DisplayerMoveTo(SelectedIndex, 0f);
        }

        public void ToIndex(int index)
        {
            SelectedIndex = index;
            SelectedIndex = SelectedIndex.Clamp(0, _options.Length - 1);
            DisplayerMoveTo(SelectedIndex, 0.3f);
        }
        public void ToOption(string optionName)
        {
            if (string.IsNullOrEmpty(optionName))
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex = Array.FindIndex(_options, x => x.PropertyInfo.Name == optionName);
            }
            SelectedIndex = SelectedIndex.Clamp(0, _options.Length - 1);
            DisplayerMoveTo(SelectedIndex, 0.3f);
        }

        Vector3 GetOptionTransformScale(float diff)
        {
            if (diff > 1)
            {
                return new Vector3(0.6f, 0.6f, 0.6f);
            }
            else
            {
                return Vector3.Lerp(Vector3.one, new Vector3(0.6f, 0.6f, 0.6f), Mathf.Abs(diff));
            }
        }
        Vector3 GetOptionTransformPosition(float diff)
        {
            return new Vector3(380 * diff, -220, 0);
        }
        Color GetOptionTextColor(float diff)
        {
            if (diff == 0)
            {
                return new Color(0.8823529f, 0.8078431f, 0.6392157f, 1f);
            }
            else if (diff.InRange(-1f, 1f))
            {
                return Color.Lerp(
                            new Color(0.8823529f, 0.8078431f, 0.6392157f, 1f), 
                            new Color(0.3607843f, 0.3098039f, 0.2862745f, 1f), 
                            Mathf.Abs(diff));
            }
            else
            {
                return new Color(0.3607843f, 0.3098039f, 0.2862745f, 1f);
            }
        }
    }
}
