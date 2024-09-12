using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Linq;
using UnityEngine;

namespace MajdataPlay.Scenes
{
    public class SettingManager : MonoBehaviour
    {
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
                var menuObj = Instantiate(menuPrefab, transform);
                var menu = menuObj.GetComponent<Menu>();
                menus[i] = menu;
                menu.SubOptionObject = property.GetValue(Setting);
            }
        }
    }
}
