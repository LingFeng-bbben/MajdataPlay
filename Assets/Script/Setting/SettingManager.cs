using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MajdataPlay.Scenes
{
    public class SettingManager : MonoBehaviour
    {
        public int Index { get; private set; } = 0;
        public GameSetting Setting => GameManager.Instance.Setting;

        public GameObject menuPrefab;

        Menu[] menus = Array.Empty<Menu>();
        void Start()
        {
            var type = Setting.GetType();
            var properties = type.GetProperties()
                             .SkipLast(3)
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

            LightManager.Instance.SetAllLight(Color.white);
            LightManager.Instance.SetButtonLight(Color.green, 3);
            LightManager.Instance.SetButtonLight(Color.red, 4);
            LightManager.Instance.SetButtonLight(Color.blue, 2);
            LightManager.Instance.SetButtonLight(Color.blue, 5);
            LightManager.Instance.SetButtonLight(Color.blue, 0);
            LightManager.Instance.SetButtonLight(Color.blue, 7);

            BindArea();
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            else if (e.Type is SensorType.A5 or SensorType.A4)
            {
                SceneManager.LoadScene(1);
                return;
            }
            

            switch(e.Type)
            {
                case SensorType.A1:
                    Index = (++Index).Clamp(0, menus.Length - 1);
                    UpdateMenu();
                    break;
                case SensorType.A8:
                    Index = (--Index).Clamp(0, menus.Length - 1);
                    UpdateMenu();
                    break;
            }
        }
        void UpdateMenu()
        {
            menus[Index].gameObject.SetActive(true);
            foreach (var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }
        }
        void BindArea()
        {
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A1);
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A8);
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A5);
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A4);
        }
        void UnbindArea()
        {
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A1);
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A8);
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A5);
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A4);
        }
        private void OnDestroy()
        {
            UnbindArea();
            GC.Collect();
        }
    }
}
