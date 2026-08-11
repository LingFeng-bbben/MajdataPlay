using System;
using System.Reflection;
#nullable enable
namespace MajdataPlay.Settings
{
    public interface IOptionEnumerator : IDisposable
    {
        string Name { get; }
        object? Current { get; }
        string ValueText { get; }
        string LocalizedValueText { get; }

        void Init(FieldInfo fieldInfo, object field);
        void Init(PropertyInfo propertyInfo, object property);

        bool MoveNext();
        bool MovePrevious();
        void Refresh();
        void RefreshLocalization();
    }
}
