using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private void Update()
        {
            if(IsPressed)
            {
                if (PressTime < 0.7f)
                    PressTime += Time.deltaTime;
            }
        }
        async UniTaskVoid InitializeAllMenu()
        {
            await UniTask.DelayFrame(1);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }
            await UniTask.DelayFrame(3);
            SwitchToDesiredIndex().Forget();
            BindArea();
        }

        async UniTaskVoid SwitchToDesiredIndex()
        {
            await UniTask.Yield();
            Index = MajInstances.GameManager.LastSettingPage;
            UpdateMenu(0, Index);
        }

        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsDown)
            {
                switch(e.Type)
                {
                    case SensorArea.A6:
                        if (Direction != -1)
                            return;
                        IsPressed = false;
                        PressTime = 0;
                        break;
                    case SensorArea.A3:
                        if (Direction != 1)
                            return;
                        IsPressed = false;
                        PressTime = 0;
                        break;
                }
                return;
            }
            else if (e.Type is SensorArea.A5 or SensorArea.A4)
            {
                //refresh some setting here
                MajInstances.AudioManager.ReadVolumeFromSettings();
                if(MajEnv.Mode == RunningMode.View)
                {
                    MajInstances.SceneSwitcher.SwitchScene("View");
                }
                else
                {
                    MajInstances.SceneSwitcher.SwitchScene("List", false);
                }
                return;
            }
            

            switch(e.Type)
            {
                case SensorArea.A6:
                    if (IsPressed)
                        return;
                    Direction = -1;
                    IsPressed = true;
                    PressTime = 0;
                    break;
                case SensorArea.A3:
                    if (IsPressed)
                        return;
                    Direction = 1;
                    IsPressed = true;
                    PressTime = 0;
                    break;
                case SensorArea.A1:
                    NextMenu();
                    break;
                case SensorArea.A8:
                    PreviousMenu();
                    break;
            }
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
                return;
            
            if (newIndex > oldIndex)
                menus[Index].ToFirst();
            else
                menus[Index].ToLast();
            menus[Index].gameObject.SetActive(true);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }
        }
        void BindArea()
        {
            InputManager.BindButton(OnAreaDown, SensorArea.A1);
            InputManager.BindButton(OnAreaDown, SensorArea.A8);
            InputManager.BindButton(OnAreaDown, SensorArea.A5);
            InputManager.BindButton(OnAreaDown, SensorArea.A4);
            InputManager.BindButton(OnAreaDown, SensorArea.A3);
            InputManager.BindButton(OnAreaDown, SensorArea.A6);
        }
        void UnbindArea()
        {
            InputManager.UnbindButton(OnAreaDown, SensorArea.A1);
            InputManager.UnbindButton(OnAreaDown, SensorArea.A8);
            InputManager.UnbindButton(OnAreaDown, SensorArea.A5);
            InputManager.UnbindButton(OnAreaDown, SensorArea.A4);
            InputManager.UnbindButton(OnAreaDown, SensorArea.A3);
            InputManager.UnbindButton(OnAreaDown, SensorArea.A6);
        }
        private void OnDestroy()
        {
            UnbindArea();
            GC.Collect();
        }
    }
}
