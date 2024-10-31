using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MajdataPlay.Setting
{
    public class SettingManager : MonoBehaviour
    {
        public int Index { get; private set; } = 0;
        public GameSetting Setting => MajInstances.Setting;

        public GameObject menuPrefab;

        Menu[] menus = Array.Empty<Menu>();
        void Start()
        {
            var type = Setting.GetType();
            var properties = type.GetProperties()
                             .SkipLast(2)
                             .ToArray();
            menus = new Menu[properties.Length];
            foreach(var (i, property) in properties.WithIndex())
            {
                object root = Setting;
                var _property = property;
                if(property.Name == "Audio")
                {
                    root = property.GetValue(Setting);
                    _property = property.PropertyType.GetProperty("Volume");
                }

                var menuObj = Instantiate(menuPrefab, transform);
                var menu = menuObj.GetComponent<Menu>();
                menus[i] = menu;
                menu.SubOptionObject = _property.GetValue(root);
                menu.Name = _property.Name;
            }
            foreach(var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }

            MajInstances.LightManager.SetAllLight(Color.white);
            MajInstances.LightManager.SetButtonLight(Color.green, 3);
            MajInstances.LightManager.SetButtonLight(Color.red, 4);
            MajInstances.LightManager.SetButtonLight(Color.blue, 2);
            MajInstances.LightManager.SetButtonLight(Color.blue, 5);
            MajInstances.LightManager.SetButtonLight(Color.blue, 0);
            MajInstances.LightManager.SetButtonLight(Color.blue, 7);

            BindArea();
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            else if (e.Type is SensorType.A5 or SensorType.A4)
            {
                //refresh some setting here
                MajInstances.AudioManager.ReadVolumeFromSettings();
                MajInstances.SceneSwitcher.SwitchScene("List");
                return;
            }
            

            switch(e.Type)
            {
                case SensorType.A1:
                    NextMenu();
                    break;
                case SensorType.A8:
                    PreviousMenu();
                    break;
            }
        }
        public void PreviousMenu()
        {
            var oldIndex = Index;
            Index = (--Index).Clamp(0, menus.Length - 1);
            UpdateMenu(oldIndex,Index);
        }
        public void NextMenu()
        {
            var oldIndex = Index;
            Index = (++Index).Clamp(0, menus.Length - 1);
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
            MajInstances.InputManager.BindButton(OnAreaDown, SensorType.A1);
            MajInstances.InputManager.BindButton(OnAreaDown, SensorType.A8);
            MajInstances.InputManager.BindButton(OnAreaDown, SensorType.A5);
            MajInstances.InputManager.BindButton(OnAreaDown, SensorType.A4);
        }
        void UnbindArea()
        {
            MajInstances.InputManager.UnbindButton(OnAreaDown, SensorType.A1);
            MajInstances.InputManager.UnbindButton(OnAreaDown, SensorType.A8);
            MajInstances.InputManager.UnbindButton(OnAreaDown, SensorType.A5);
            MajInstances.InputManager.UnbindButton(OnAreaDown, SensorType.A4);
        }
        private void OnDestroy()
        {
            UnbindArea();
            GC.Collect();
        }
    }
}
