using System;
using System.Reflection;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deep copy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T? Clone<T>(this T? source) where T: new()
        {
            if (source is null)
                return default;
            else if (source is string)
                return source;

            var type = source.GetType();

            if (type.IsValueType)
                return source;

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = source as Array;
                if (array is null)
                    return default;
                var copiedArray = Array.CreateInstance(elementType, array.Length);

                for (int i = 0; i < array.Length; i++)
                    copiedArray.SetValue(array.GetValue(i).Clone(), i);

                return (T)(object)copiedArray;
            }

            var result = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(source);
                if (value != null)
                    field.SetValue(result, value.Clone());
            }

            return (T)result;
        }
    }
}