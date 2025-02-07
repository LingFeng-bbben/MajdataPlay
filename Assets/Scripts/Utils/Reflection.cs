using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class Reflection<T>
    {
        public static ReadOnlyMemory<MethodInfo> Methods { get; }
        public static ReadOnlyMemory<PropertyInfo> Properties { get; }
        public static ReadOnlyMemory<FieldInfo> Fields { get; }
        public static ReadOnlyMemory<ConstructorInfo> Constructors { get; }
        public static ReadOnlyMemory<MemberInfo> Members { get; }
        static Reflection()
        {
            var type = typeof(T);
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Methods = type.GetMethods(flags);
            Properties = type.GetProperties(flags);
            Fields = type.GetFields(flags);
            Constructors = type.GetConstructors();
            Members = type.GetMembers(flags);
        }
    }
}
