using LitMotion;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
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
        const float OPTION_MOVE_DURATION = 0.18f;
        const int VISIBLE_OPTION_RADIUS = 2;
        static readonly Vector3 UNSELECTED_OPTION_SCALE = Vector3.one * 0.6f;
        static readonly Color SELECTED_OPTION_COLOR = new(0.8823529f, 0.8078431f, 0.6392157f, 1f);
        static readonly Color UNSELECTED_OPTION_COLOR = new(0.3607843f, 0.3098039f, 0.2862745f, 1f);

        [field: SerializeField, ReadOnlyField]
        public string Name { get; set; } = string.Empty;

        [field: SerializeField, ReadOnlyField]
        public int SelectedIndex { get; private set; }
        /// <summary>
        /// Option对象<para>e.g. GameSetting.Game</para>
        /// </summary>
        public object Instance { get; set; } = null!;

        [SerializeField]
        [FormerlySerializedAs("optionPrefab")]
        GameObject _optionPrefab = null!;

        SettingManager _manager = null!;

        [SerializeField, ReadOnlyField]
        float _listCursorPos = 0;

        PropertyInfo[] _properties = Array.Empty<PropertyInfo>();
        Option?[] _options = Array.Empty<Option?>();

        MotionHandle _optionAnim;
        int _visibleStart = -1;
        int _visibleEnd = -1;
        int _appliedSelectedIndex = -1;
        bool _isInitialized = false;

        readonly SettingConfig _settingConfig = MajEnv.RuntimeConfig?.Setting ?? new();
        void Awake()
        {
            _manager = FindAnyObjectByType<SettingManager>();
        }
        public void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            var type = Instance.GetType();
            _properties = type.GetProperties()
                              .Where(x => !x.GetCustomAttributes<HideInSettingUIAttribute>().Any())
                              .ToArray();
            _options = new Option?[_properties.Length];
            _isInitialized = true;
        }
        void OnDisable()
        {
            _optionAnim.TryCancel();
            SelectedIndex = 0;
        }
        void OnDestroy()
        {
            _optionAnim.TryCancel();
        }
        internal void SwitchOption(int direction)
        {
            if (!_isInitialized)
            {
                return;
            }
            MoveOption(direction);
        }
        internal void HandleInput()
        {
            if (!_isInitialized || _options.Length == 0)
            {
                return;
            }

            EnsureOption(SelectedIndex).HandleInput();
        }
        internal void RefreshVisibleEnumerators()
        {
            if (!_isInitialized)
            {
                return;
            }

            for (var i = _visibleStart; i <= _visibleEnd; i++)
            {
                var option = i >= 0 ? _options[i] : null;
                if (option is not null)
                {
                    option.RefreshEnumerator();
                }
            }
        }
        void MoveOption(int direction)
        {
            var targetIndex = SelectedIndex + direction;
            if (targetIndex < 0)
            {
                _manager.PreviousMenu();
                return;
            }
            if (targetIndex >= _options.Length)
            {
                _manager.NextMenu();
                return;
            }

            SelectOption(targetIndex, true);
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
            if (_options.Length == 0)
            {
                return;
            }

            var visibleStart = Mathf.Max(0, Mathf.FloorToInt(_listCursorPos) - VISIBLE_OPTION_RADIUS);
            var visibleEnd = Mathf.Min(_options.Length - 1, Mathf.CeilToInt(_listCursorPos) + VISIBLE_OPTION_RADIUS);
            if (visibleStart != _visibleStart || visibleEnd != _visibleEnd)
            {
                for (var i = _visibleStart; i <= _visibleEnd; i++)
                {
                    if (i >= 0 && (i < visibleStart || i > visibleEnd))
                    {
                        _options[i]?.gameObject.SetActive(false);
                    }
                }
                _visibleStart = visibleStart;
                _visibleEnd = visibleEnd;
            }

            for (var i = visibleStart; i <= visibleEnd; i++)
            {
                var distance = i - _listCursorPos;
                var optionDisplayer = EnsureOption(i);
                optionDisplayer.transform.localPosition = GetOptionTransformPosition(distance);
                optionDisplayer.transform.localScale = GetOptionTransformScale(distance);
                optionDisplayer.SetTextColor(GetOptionTextColor(distance));
                if (!optionDisplayer.gameObject.activeSelf)
                {
                    optionDisplayer.gameObject.SetActive(true);
                }
            }
        }
        void UpdateOptionSelectionState()
        {
            if (_appliedSelectedIndex == SelectedIndex)
            {
                return;
            }

            if (_appliedSelectedIndex >= 0 && _appliedSelectedIndex < _options.Length)
            {
                _options[_appliedSelectedIndex]?.SetSelected(false);
            }
            EnsureOption(SelectedIndex).SetSelected(true);
            _appliedSelectedIndex = SelectedIndex;
        }

        internal void ToTail()
        {
            if (!_isInitialized || _options.Length == 0)
            {
                return;
            }
            SelectOption(_options.Length - 1, false);
        }
        internal void ToHead()
        {
            if (!_isInitialized || _options.Length == 0)
            {
                return;
            }
            SelectOption(0, false);
        }

        internal void ToOption(string optionName)
        {
            if (!_isInitialized || _options.Length == 0)
            {
                return;
            }
            var index = string.IsNullOrEmpty(optionName)
                ? 0
                : Array.FindIndex(_properties, x => x.Name == optionName);
            SelectOption(index, false);
        }
        void SelectOption(int index, bool useAnimation)
        {
            SelectedIndex = index.Clamp(0, _options.Length - 1);
            _settingConfig.SelectedOption = _properties[SelectedIndex].Name;
            if (useAnimation)
            {
                DisplayerMoveTo(SelectedIndex, OPTION_MOVE_DURATION);
            }
            else
            {
                SnapDisplayerTo(SelectedIndex);
            }
        }
        Option EnsureOption(int index)
        {
            var option = _options[index];
            if (option is not null)
            {
                return option;
            }

            var optionObj = Instantiate(_optionPrefab, transform);
            optionObj.SetActive(false);
            option = optionObj.GetComponent<Option>();
            _options[index] = option;
            option.Init(_manager, _properties[index], Instance);
            return option;
        }
        void SnapDisplayerTo(float targetPos)
        {
            _optionAnim.TryCancel();
            _listCursorPos = targetPos;
            UpdateOptionSelectionState();
            UpdateDisplayerPosition();
        }

        static Vector3 GetOptionTransformScale(float diff)
        {
            return Vector3.Lerp(Vector3.one, UNSELECTED_OPTION_SCALE, Mathf.Clamp01(Mathf.Abs(diff)));
        }
        static Vector3 GetOptionTransformPosition(float diff)
        {
            return new Vector3(380 * diff, -220, 0);
        }
        static Color GetOptionTextColor(float diff)
        {
            return Color.Lerp(SELECTED_OPTION_COLOR, UNSELECTED_OPTION_COLOR, Mathf.Clamp01(Mathf.Abs(diff)));
        }
    }
}
