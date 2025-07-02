using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MajdataPlay.Setting
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
        void Start()
        {
            var type = Setting.GetType();
            var properties = type.GetProperties()
                             .Where(x => x.GetCustomAttributes<SettingVisualizationIgnoreAttribute>().Count() == 0)
                             .ToArray();
            menus = new Menu[properties.Length];
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
                menus[i] = menu;
                menu.SubOptionObject = _property.GetValue(root);
                menu.Name = _property.Name;
            }
            foreach (var (i, menu) in menus.WithIndex())
            {
                menu.gameObject.SetActive(true);
            }

            LedRing.SetAllLight(Color.white);
            LedRing.SetButtonLight(Color.green, 3);
            LedRing.SetButtonLight(Color.red, 4);
            LedRing.SetButtonLight(Color.blue, 2);
            LedRing.SetButtonLight(Color.blue, 5);
            LedRing.SetButtonLight(Color.blue, 0);
            LedRing.SetButtonLight(Color.blue, 7);

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
            await UniTask.DelayFrame(1);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                {
                    menu.gameObject.SetActive(false);
                }
            }
            await UniTask.DelayFrame(3);
            await SwitchToDesiredIndex();
            _isInited = true;
        }

        async UniTask SwitchToDesiredIndex()
        {
            await UniTask.Yield();
            Index = MajInstances.GameManager.LastSettingPage;
            UpdateMenu(0, Index);
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
            MajInstances.GameManager.Save();
            GC.Collect();
        }
    }
}
