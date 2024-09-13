using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Linq;
using UnityEngine;

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
                var menuObj = Instantiate(menuPrefab, transform);
                var menu = menuObj.GetComponent<Menu>();
                menus[i] = menu;
                menu.SubOptionObject = property.GetValue(Setting);
            }
            foreach(var (i, menu) in menus.WithIndex())
            {
                if (i != Index)
                    menu.gameObject.SetActive(false);
            }
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {

        }
    }
}
