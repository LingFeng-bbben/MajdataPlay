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

namespace MajdataPlay.Scenes.Setting
{
    public class SettingManager : MonoBehaviour
    {
        public int Index { get; private set; } = 0;
        public bool IsPressed { get; private set; } = false;
        public float PressTime { get; private set; } = 0f;
        public int Direction { get; private set; } = 1;
        public GameSetting Setting => MajInstances.Settings;

        public GameObject menuPrefab;

        Menu[] menus = Array.Empty<Menu>();
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

            if(!_settingConfig.IgnoreChartSettingPage)
            {
                menus = new Menu[properties.Length + 1];
                offset = 0;
                var selectedChart = SongStorage.WorkingCollection.Current;
                var chartSetting = ChartSettingStorage.GetSetting(selectedChart);
                var chartSettingType = chartSetting.GetType();
                var menuObj = Instantiate(menuPrefab, transform);
                menuObj.name = chartSettingType.Name;
                var menu = menuObj.GetComponent<Menu>();
                menus[properties.Length] = menu;
                menu.SubOptionObject = chartSetting;
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

                var menuObj = Instantiate(menuPrefab, transform);
                menuObj.name = _property.Name;
                var menu = menuObj.GetComponent<Menu>();
                menus[i + offset] = menu;
                menu.SubOptionObject = _property.GetValue(root);
                menu.Name = _property.Name;
            }
            foreach (var (i, menu) in menus.WithIndex())
            {
                menu.Init();
                menu.gameObject.SetActive(true);
            }

            MajInstances.AudioManager.PlaySFX("settings.wav");

            InitializeAllMenu().Forget();
        }
        void Update()
        {
            if(_isExited || !_isInited)
            {
                return;
            }
            if(IsPressed)
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
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A1))
                {
                    NextMenu();
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A8))
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
            LedRing.SetAllLight(Color.white);
            LedRing.SetButtonLight(Color.green, 3);
            LedRing.SetButtonLight(Color.red, 4);
            LedRing.SetButtonLight(Color.blue, 2);
            LedRing.SetButtonLight(Color.blue, 5);
            LedRing.SetButtonLight(Color.blue, 0);
            LedRing.SetButtonLight(Color.blue, 7);
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
        void UpdateMenu(int oldIndex,int newIndex)
        {
            if (oldIndex == newIndex)
            {
                return;
            }
            
            if (newIndex > oldIndex)
            {
                menus[Index].ToFirst();
            }
            else
            {
                menus[Index].ToLast();
            }
            menus[Index].gameObject.SetActive(true);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                {
                    menu.gameObject.SetActive(false);
                }
            }
        }
        private void OnDestroy()
        {
            _isExited = true;
            InputManager.TouchButtonRingEdge = 5.4f;
            GameManager.RequestSave(this);
            GC.Collect();
        }
    }
}
