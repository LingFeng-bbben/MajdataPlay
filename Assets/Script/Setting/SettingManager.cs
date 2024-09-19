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
            }
            foreach(var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }
            InputManager.Instance.BindButton(OnAreaDown, SensorType.A1);
            InputManager.Instance.BindButton(OnAreaDown, SensorType.A8);
            InputManager.Instance.BindButton(OnAreaDown, SensorType.A5);
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            else if (!e.IsButton)
                return;
            else if (e.Type == SensorType.A5)
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
        private void OnDestroy()
        {
            InputManager.Instance.UnbindButton(OnAreaDown, SensorType.A1);
            InputManager.Instance.UnbindButton(OnAreaDown, SensorType.A8);
            InputManager.Instance.UnbindButton(OnAreaDown, SensorType.A5);
            GC.Collect();
        }
    }
}
