using Cysharp.Threading.Tasks;
using MajdataPlay.Settings;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using MajdataPlay.Settings.Runtime;
using UnityEngine.Serialization;
using TMPro;
using MajdataPlay.Numerics;
using LitMotion;

namespace MajdataPlay.Scenes.Setting
{
    public class SettingManager : MonoBehaviour
    {
        public int Index { get; private set; } = 0;
        public bool IsPressed { get; private set; } = false;
        public float PressTime { get; private set; } = 0f;
        public int Direction { get; private set; } = 1;
        public GameSetting Setting => MajEnv.Settings;

        public GameObject menuPrefab;

        [SerializeField]
        [FormerlySerializedAs("menuTitleDisplayerPrefab")]
        GameObject _menuTitleDisplayerPrefab;

        [SerializeField]
        [FormerlySerializedAs("menuListRoot")]
        GameObject _menuListRoot;

        [SerializeField]
        [FormerlySerializedAs("menuTitleDisplayerListRoot")]
        GameObject _menuTitleDisplayerListRoot;

        [SerializeField]
        [FormerlySerializedAs("currentMenuNameDisplayer")]
        TextMeshProUGUI _currentMenuNameDisplayer;

        [SerializeField]
        [FormerlySerializedAs("descriptionText")]
        TextMeshProUGUI _descriptionTextDisplayer;

        Menu[] menus = Array.Empty<Menu>();
        TextDisplayer[] _menuTitleDisplayers = Array.Empty<TextDisplayer>();

        float _listCursorPos = 0;
        MotionHandle _menuTitleDisplayerAnim;

        bool _isExited = false;
        bool _isInited = false;

        readonly SettingConfig _settingConfig = MajEnv.RuntimeConfig?.Setting ?? new();
        void Awake()
        {
            InputManager.TouchButtonRingEdge = 4.8f;
        }
        void Start()
        {
            var type = Setting.GetType();
            var properties = type.GetProperties()
                                 .Where(x => x.GetCustomAttributes<HideInSettingUIAttribute>().Count() == 0)
                                 .ToArray();
            var offset = 0;
            var listRoot = _menuListRoot.transform;
            var currentCollection = SongStorage.WorkingCollection;
            if (!_settingConfig.IgnoreChartSettingPage && currentCollection.Count != 0)
            {
                menus = new Menu[properties.Length + 1];
                offset = 0;
                var chartSetting = ChartSettingStorage.GetSetting(MajEnv.RuntimeConfig.List.SelectedSongHash);
                var chartSettingType = chartSetting.GetType();
                var menuObj = Instantiate(menuPrefab, listRoot);
                menuObj.name = chartSettingType.Name;
                var menu = menuObj.GetComponent<Menu>();
                menus[properties.Length] = menu;
                menu.Instance = chartSetting;
                menu.Name = chartSettingType.Name;
            }
            else
            {
                menus = new Menu[properties.Length];
            }
            foreach (var (i, property) in properties.WithIndex())
            {
                object root = Setting;
                var _property = property;

                if (property.Name == "Audio")
                {
                    root = property.GetValue(Setting);
                    _property = property.PropertyType.GetProperty("Volume");
                }

                var menuObj = Instantiate(menuPrefab, listRoot);
                menuObj.name = _property.Name;
                var menu = menuObj.GetComponent<Menu>();
                menus[i + offset] = menu;
                menu.Instance = _property.GetValue(root);
                menu.Name = _property.Name;
            }
            foreach (var (i, menu) in menus.WithIndex())
            {
                menu.Init();
                menu.gameObject.SetActive(true);
            }

            MajInstances.AudioManager.PlaySFX("settings.wav");

            _menuTitleDisplayers = new TextDisplayer[menus.Length];
            var menuTitleDisplayerListRoot = _menuTitleDisplayerListRoot.transform;
            for (var i = 0; i < menus.Length; i++)
            {
                var menu = menus[i];
                var displayerObject = Instantiate(_menuTitleDisplayerPrefab, menuTitleDisplayerListRoot);
                var displayerTransform = displayerObject.transform;
                var displayerRectTransform = displayerObject.GetComponent<RectTransform>();
                var titleDisplayer = displayerObject.GetComponentInChildren<TextMeshProUGUI>();

                _menuTitleDisplayers[i] = new()
                {
                    Name = $"MAJSETTING_CATEGORY_{menu.Name}",
                    GameObject = displayerObject,
                    RectTransform = displayerRectTransform,
                    Transform = displayerTransform,
                    TitleDisplayer = titleDisplayer
                };
            }
            UpdateMenuTitleDisplayerPosition();
            InitializeAllMenu().Forget();
        }
        void Update()
        {
            if(_isExited || !_isInited)
            {
                return;
            }
            for (var i = 0; i < _menuTitleDisplayers.Length; i++)
            {
                var displayer = _menuTitleDisplayers[i];
                displayer.TitleDisplayer.text = displayer.Name.i18n();
            }
            var isCalibratorRequested = InputManager.IsButtonClickedInThisFrame(ButtonZone.A1) ||
                                        InputManager.IsSensorClickedInThisFrame(SensorArea.A1);
            if (isCalibratorRequested)
            {
                MajInstances.AudioManager.ReadVolumeFromSettings();
                _isExited = true;
                MajInstances.SceneSwitcher.SwitchScene("Calibrator");
                return;
            }
            if (IsPressed)
            {
                if (PressTime < 0.7f)
                {
                    PressTime += Time.deltaTime;
                }
                if (InputManager.CheckButtonStatus(ButtonZone.A6, SwitchStatus.Off) && Direction == -1)
                {
                    IsPressed = false;
                    PressTime = 0;
                }
                else if (InputManager.CheckButtonStatus(ButtonZone.A3, SwitchStatus.Off) && Direction == 1)
                {
                    IsPressed = false;
                    PressTime = 0;
                }
            }
            else
            {
                var isExitRequested = InputManager.IsButtonClickedInThisFrame(ButtonZone.A4) ||
                                      InputManager.IsButtonClickedInThisFrame(ButtonZone.A5);
                if (isExitRequested)
                {
                    MajInstances.AudioManager.ReadVolumeFromSettings();
                    _isExited = true;
                    if (MajEnv.Mode == RunningMode.View)
                    {
                        MajInstances.SceneSwitcher.SwitchScene("View");
                    }
                    else
                    {
                        MajInstances.SceneSwitcher.SwitchScene("List", false);
                    }
                    return;
                }
                if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A3))
                {
                    Direction = 1;
                    IsPressed = true;
                    PressTime = 0;
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A6))
                {
                    Direction = -1;
                    IsPressed = true;
                    PressTime = 0;
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A2))
                {
                    NextMenu();
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A7))
                {
                    PreviousMenu();
                }
            }
        }
        async UniTaskVoid InitializeAllMenu()
        {
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                {
                    menu.gameObject.SetActive(false);
                }
            }
            await UniTask.DelayFrame(3);
            await SwitchToDesiredIndex();
            MajInstances.SceneSwitcher.FadeOut();
            SetSettingLights();
            _isInited = true;
        }

        void SetSettingLights()
        {
            CabinetLed.SetAllLight(Color.white);
            CabinetLed.SetButtonLight(Color.green, 3);
            CabinetLed.SetButtonLight(Color.red, 4);
            CabinetLed.SetButtonLight(Color.blue, 2);
            CabinetLed.SetButtonLight(Color.blue, 5);
            CabinetLed.SetButtonLight(Color.blue, 0);
            CabinetLed.SetButtonLight(Color.blue, 7);
        }

        async UniTask SwitchToDesiredIndex()
        {
            await UniTask.Yield();
            var index = 0;
            index = Array.FindIndex(menus, x => x.Name == _settingConfig.SelectedMenu);
            if(index == -1)
            {
                index = Array.FindIndex(menus, x => x.Name == nameof(GameSetting.Game));
                _settingConfig.SelectedOption = string.Empty;
            }
            Index = index;
            UpdateMenu(0, Index);
            menus[Index].ToOption(_settingConfig.SelectedOption);
        }

        public void PreviousMenu()
        {
            var oldIndex = Index;
            Index--;
            if (Index < 0)
            {
                oldIndex = menus.Length;
                Index = menus.Length - 1;
            }
            _settingConfig.SelectedMenu = menus[Index].Name;
            UpdateMenu(oldIndex,Index);
        }
        public void NextMenu()
        {
            var oldIndex = Index;
            Index++;
            if (Index >= menus.Length)
            {
                oldIndex = -1;
                Index = 0;
            }
            _settingConfig.SelectedMenu = menus[Index].Name;
            UpdateMenu(oldIndex, Index);
        }
        public void SetDescriptionText(string text)
        {
            _descriptionTextDisplayer.text = text.Replace('\n', ',');
        }
        void UpdateMenu(int oldIndex,int newIndex)
        {
            if (oldIndex == newIndex)
            {
                return;
            }
            
            if (newIndex > oldIndex)
            {
                menus[Index].ToHead();
            }
            else
            {
                menus[Index].ToTail();
            }
            menus[Index].gameObject.SetActive(true);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                {
                    menu.gameObject.SetActive(false);
                }
            }
            _menuTitleDisplayerAnim.TryCancel();
            _menuTitleDisplayerAnim = LMotion.Create(_listCursorPos, newIndex, 0.25f)
                                             .WithEase(Ease.OutQuad)
                                             .Bind(x =>
                                             {
                                                 _listCursorPos = x;
                                                 UpdateMenuTitleDisplayerPosition();
                                             });
        }
        void UpdateMenuTitleDisplayerPosition()
        {
            for (var i = 0; i < _menuTitleDisplayers.Length; i++)
            {
                var displayer = _menuTitleDisplayers[i];
                var distance = i - _listCursorPos;
                displayer.RectTransform.anchoredPosition = GetMenuTitleDisplayerPositionFromDelta(distance);
            }
        }
        private void OnDestroy()
        {
            _isExited = true;
            InputManager.TouchButtonRingEdge = 5.4f;
            GameManager.RequestSave(this);
            GC.Collect();
        }

        Vector2 GetMenuTitleDisplayerPositionFromDelta(float delta)
        {
            const int X_POS_STEP = 164;
            const int X_POS_WITH_DELTA_1 = 218;

            var absDelta = Mathf.Abs(delta);
            if (delta == 0)
            {
                return Vector2.zero;
            }
            else if (absDelta.InRange(0, 1))
            {
                return new Vector2(X_POS_WITH_DELTA_1 * absDelta * Mathf.Sign(delta), 0);
            }
            else
            {
                var index = (int)absDelta;
                var posStartAt = X_POS_WITH_DELTA_1 + (X_POS_STEP * (index - 1));
                var middle = X_POS_STEP * (absDelta - Mathf.Floor(absDelta));

                return new Vector2((posStartAt + middle) * Mathf.Sign(delta), 0);
            }
        }

        struct TextDisplayer
        {
            public required string Name { get; init; }
            public required GameObject GameObject { get; init; }
            public required Transform Transform { get; init; }
            public required RectTransform RectTransform { get; init; }
            public required TextMeshProUGUI TitleDisplayer { get; init; }
        }
    }
}
