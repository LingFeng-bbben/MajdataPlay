using System;
using System.Collections.Generic;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 设置菜单配置，包含一组相关的设置项
    /// </summary>
    public class SettingMenu
    {
        public string Name { get; }
        public IReadOnlyList<ISettingItem> Items { get; }

        public SettingMenu(string name, IReadOnlyList<ISettingItem> items)
        {
            Name = name;
            Items = items;
        }
    }

    /// <summary>
    /// 设置菜单配置，用于特殊的字符串类型菜单
    /// </summary>
    public class SpecialSettingMenu
    {
        public string Name { get; }
        public object SubOptionObject { get; }

        public SpecialSettingMenu(string name, object subOptionObject)
        {
            Name = name;
            SubOptionObject = subOptionObject;
        }
    }
}
