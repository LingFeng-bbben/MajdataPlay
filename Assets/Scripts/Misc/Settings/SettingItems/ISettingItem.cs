using System;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 设置项接口，所有类型的设置项都实现此接口
    /// </summary>
    public interface ISettingItem
    {
        /// <summary>
        /// 设置项名称，对应本地化键
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否为数值类型
        /// </summary>
        bool IsNumeric { get; }

        /// <summary>
        /// 是否只读
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 当设置项的值发生改变时触发
        /// </summary>
        event Action<object>? OnValueChanged;

        /// <summary>
        /// 获取当前值
        /// </summary>
        object GetValue();

        /// <summary>
        /// 获取当前值的字符串表示
        /// </summary>
        string GetValueString();

        /// <summary>
        /// 设置新值
        /// </summary>
        void SetValue(object value);

        /// <summary>
        /// 修改值（上下调整）
        /// </summary>
        /// <param name="direction">方向：1 表示增加，-1 表示减少</param>
        void ModifyValue(int direction);
    }
}
