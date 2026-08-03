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
        MenuTitleDisplayer _menuTitleDisplayerPrefab;

        [SerializeField]
        [FormerlySerializedAs("menuListRoot")]
        GameObject _menuListRoot;

        [SerializeField]
        [FormerlySerializedAs("menuTitleDisplayerListRoot")]
        GameObject _menuTitleDisplayerListRoot;

        [SerializeField]
        [FormerlySerializedAs("descriptionText")]
        TextMeshProUGUI _descriptionTextDisplayer;

        Menu[] menus = Array.Empty<Menu>();
        MenuTitleDisplayer[] _menuTitleDisplayers = Array.Empty<MenuTitleDisplayer>();

        float _listCursorPos = 0;
        int _listCursorTarget = 0;
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
                                 .Where(x => !x.GetCustomAttributes<HideInSettingUIAttribute>().Any())
                                 .ToArray();
            var offset = 0;
            var listRoot = _menuListRoot.transform;
            var currentCollection = SongStorage.WorkingCollection;
            if (!_settingConfig.IgnoreChartSettingPage && currentCollection.Count != 0)
            {
                menus = new Menu[properties.Length + 1];
                offset = 1;
                var chartSetting = ChartSettingStorage.GetSetting(MajEnv.RuntimeConfig.List.SelectedSongHash);
                var chartSettingType = chartSetting.GetType();
                var menuObj = Instantiate(menuPrefab, listRoot);
                menuObj.name = chartSettingType.Name;
                var menu = menuObj.GetComponent<Menu>();
                menus[0] = menu;
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
            foreach (var menu in menus)
            {
                menu.Init();
                menu.gameObject.SetActive(true);
            }

            MajInstances.AudioManager.PlaySFX("settings.wav");

            _menuTitleDisplayers = new MenuTitleDisplayer[menus.Length];
            var menuTitleDisplayerListRoot = _menuTitleDisplayerListRoot.transform;
            for (var i = 0; i < menus.Length; i++)
            {
                var menu = menus[i];
                var displayer = Instantiate(_menuTitleDisplayerPrefab, menuTitleDisplayerListRoot);
                displayer.Initialize($"MAJSETTING_CATEGORY_{menu.Name}");
                _menuTitleDisplayers[i] = displayer;
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
            _listCursorTarget = index;
            if (Index != 0)
            {
                UpdateMenu(1);
            }
            menus[Index].ToOption(_settingConfig.SelectedOption);
        }

        public void PreviousMenu()
        {
            if (menus.Length <= 1)
            {
                return;
            }

            Index = (Index - 1 + menus.Length) % menus.Length;
            _listCursorTarget--;
            _settingConfig.SelectedMenu = menus[Index].Name;
            UpdateMenu(-1);
        }
        public void NextMenu()
        {
            if (menus.Length <= 1)
            {
                return;
            }

            Index = (Index + 1) % menus.Length;
            _listCursorTarget++;
            _settingConfig.SelectedMenu = menus[Index].Name;
            UpdateMenu(1);
        }
        public void SetDescriptionText(string text)
        {
            _descriptionTextDisplayer.text = text.Replace('\n', ',');
        }
        void UpdateMenu(int direction)
        {
            if (direction > 0)
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
            _menuTitleDisplayerAnim = LMotion.Create(_listCursorPos, _listCursorTarget, 0.25f)
                                             .WithEase(Ease.OutQuad)
                                             .WithOnComplete(NormalizeMenuTitleCursor)
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
                var distance = GetCircularMenuTitleDistance(i);
                displayer.SetDistance(distance, _menuTitleDisplayers.Length);
            }
        }
        void NormalizeMenuTitleCursor()
        {
            _listCursorPos = Index;
            _listCursorTarget = Index;
            UpdateMenuTitleDisplayerPosition();
        }
        float GetCircularMenuTitleDistance(int displayerIndex)
        {
            var itemCount = _menuTitleDisplayers.Length;
            if (itemCount <= 1)
            {
                return 0;
            }

            var wrappedCursorPos = Mathf.Repeat(_listCursorPos, itemCount);
            var distance = displayerIndex - wrappedCursorPos;
            var halfItemCount = itemCount / 2f;

            if (distance > halfItemCount)
            {
                distance -= itemCount;
            }
            else if (distance < -halfItemCount)
            {
                distance += itemCount;
            }

            return distance;
        }
        private void OnDestroy()
        {
            _isExited = true;
            _menuTitleDisplayerAnim.TryCancel();
            InputManager.TouchButtonRingEdge = 5.4f;
            GameManager.RequestSave(this);
        }
    }
}
