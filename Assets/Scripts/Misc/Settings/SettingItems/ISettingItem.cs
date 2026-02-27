using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 设置项的基础接口，用于替代反射机制
    /// </summary>
    public interface ISettingItem
    {
        /// <summary>
        /// 设置项的名称（用于本地化键）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 当前值（以对象形式返回）
        /// </summary>
        object GetValue();

        /// <summary>
        /// 设置值（接受对象）
        /// </summary>
        void SetValue(object value);

        /// <summary>
        /// 获取当前值的字符串表示
        /// </summary>
        string GetValueString();

        /// <summary>
        /// 是否为数值类型
        /// </summary>
        bool IsNumeric { get; }

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 数值增加（direction为1增加，-1减少）
        /// </summary>
        void ModifyValue(int direction);

        /// <summary>
        /// 值改变时的回调
        /// </summary>
        event Action<object>? OnValueChanged;
    }
}
