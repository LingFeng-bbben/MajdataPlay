using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Settings;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class OptionEnumeratorAttribute : Attribute
{
    public Type EnumeratorType
    {
        get;
    }
    public OptionEnumeratorAttribute(Type enumeratorType)
    {
        EnumeratorType = enumeratorType;
    }
    public IOptionEnumerator Instance()
    {
        return (IOptionEnumerator)Activator.CreateInstance(EnumeratorType);
    }
}
