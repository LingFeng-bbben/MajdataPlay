using LitMotion;
using MajdataPlay.Collections;
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
                option.Init(_manager, property, Instance);
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
        internal void SwitchOption(int direction)
        {
            MoveOption(direction);
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
                optionDisplayer.SetSelected(i == SelectedIndex);
            }
        }

        internal void ToTail()
        {
            SelectOption(_options.Length - 1, false);
        }
        internal void ToHead()
        {
            SelectOption(0, false);
        }

        internal void ToOption(string optionName)
        {
            var index = string.IsNullOrEmpty(optionName)
                ? 0
                : Array.FindIndex(_options, x => x.PropertyInfo.Name == optionName);
            SelectOption(index, false);
        }
        void SelectOption(int index, bool useAnimation)
        {
            SelectedIndex = index.Clamp(0, _options.Length - 1);
            _settingConfig.SelectedOption = _options[SelectedIndex].PropertyInfo.Name;
            if (useAnimation)
            {
                DisplayerMoveTo(SelectedIndex, OPTION_MOVE_DURATION);
            }
            else
            {
                SnapDisplayerTo(SelectedIndex);
            }
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
