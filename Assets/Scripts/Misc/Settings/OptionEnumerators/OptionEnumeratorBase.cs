using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Settings.OptionEnumerators;
public abstract class OptionEnumeratorBase
{
    protected const int FLAG_FIELD_MODE = 1;
    protected const int FLAG_PROPERTY_MODE = 2;

    public object? Current 
    { 
        get
        {
            if(OptionValues.Length == 0)
            {
                return default;
            }
            return OptionValues[ValueIndex];
        }
    }

    protected int ModeFlag = 0;

    protected object Value
    {
        get
        {
            switch (ModeFlag)
            {
                case FLAG_FIELD_MODE:
                    return FieldInfo!.GetValue(Target);
                case FLAG_PROPERTY_MODE:
                    return PropertyInfo!.GetValue(Target);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch (ModeFlag)
            {
                case FLAG_FIELD_MODE:
                    FieldInfo!.SetValue(Target, value);
                    break;
                case FLAG_PROPERTY_MODE:
                    PropertyInfo!.SetValue(Target, value);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    protected object Target = null!;

    protected Type Type
    {
        get
        {
            return _memberType;
        }
    }
    protected bool IsIntType
    {
        get
        {
            return _memberType == typeof(int) || _memberType == typeof(long) ||
                   _memberType == typeof(short) || _memberType == typeof(byte) ||
                   _memberType == typeof(uint) || _memberType == typeof(ulong) ||
                   _memberType == typeof(ushort) || _memberType == typeof(sbyte);
        }
    }
    protected bool IsFloatType
    {
        get
        {
            return _memberType == typeof(float) || _memberType == typeof(double) ||
                   _memberType == typeof(decimal);
        }
    }
    protected bool IsReadOnly;

    protected object[] OptionValues = Array.Empty<object>();
    protected int ValueIndex = 0;

    protected FieldInfo? FieldInfo;
    protected PropertyInfo? PropertyInfo;


    Type _memberType = null!;

    public void Init(FieldInfo fieldInfo, object field)
    {
        if (fieldInfo is null)
        {
            throw new ArgumentNullException(nameof(fieldInfo));
        }
        if (field is null)
        {
            throw new ArgumentNullException(nameof(field));
        }
        FieldInfo = fieldInfo;
        Target = field;
        ModeFlag = FLAG_FIELD_MODE;
        _memberType = fieldInfo.FieldType;
        IsReadOnly = GetCustomAttribute<ReadOnlyOptionAttribute>() != null;
        InitInternal();
    }
    public void Init(PropertyInfo propertyInfo, object property)
    {
        if (propertyInfo is null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }
        if (property is null)
        {
            throw new ArgumentNullException(nameof(property));
        }
        PropertyInfo = propertyInfo;
        Target = property;
        ModeFlag = FLAG_PROPERTY_MODE;
        _memberType = propertyInfo.PropertyType;
        IsReadOnly = GetCustomAttribute<ReadOnlyOptionAttribute>() != null;
        InitInternal();
    }

    public virtual bool MoveNext()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextIndex = ValueIndex + 1;
        if (nextIndex >= OptionValues.Length)
        {
            nextIndex = 0;
        }
        ValueIndex = nextIndex;
        Value = OptionValues[ValueIndex];
        return true;
    }
    public virtual bool MovePrevious()
    {
        if (IsReadOnly)
        {
            return false;
        }
        var nextIndex = ValueIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = OptionValues.Length - 1;
        }
        ValueIndex = nextIndex;
        Value = OptionValues[ValueIndex];
        return true;
    }

    protected abstract void InitInternal();
    protected T? GetCustomAttribute<T>() where T : Attribute
    {
        switch(ModeFlag)
        {
            case FLAG_FIELD_MODE:
                return FieldInfo.GetCustomAttribute<T>();
            case FLAG_PROPERTY_MODE:
                return PropertyInfo.GetCustomAttribute<T>();
            default:
                return null;
        }
    }
    protected IEnumerable<T> GetCustomAttributes<T>() where T : Attribute
    {
        switch (ModeFlag)
        {
            case FLAG_FIELD_MODE:
                return FieldInfo.GetCustomAttributes<T>();
            case FLAG_PROPERTY_MODE:
                return PropertyInfo.GetCustomAttributes<T>();
            default:
                return Array.Empty<T>();
        }
    }
}
