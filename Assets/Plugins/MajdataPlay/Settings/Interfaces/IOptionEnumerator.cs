using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Settings;
public interface IOptionEnumerator
{
    object? Current { get; }

    void Init(FieldInfo fieldInfo, object field);
    void Init(PropertyInfo propertyInfo, object property);

    bool MoveNext();
    bool MovePrevious();
}
