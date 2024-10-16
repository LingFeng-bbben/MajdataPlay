using MajdataPlay.Collections;
using MajdataPlay.Net;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class HttpRequestExceptionExtensions
    {
        public static HttpRequestError GetErrorCode(this HttpRequestException source)
        {
            switch (source.InnerException)
            {
                case SocketException socketEx:
                    return socketEx.SocketErrorCode switch
                    {
                        SocketError.HostNotFound or SocketError.NoData => HttpRequestError.NameResolutionError,
                        _ => HttpRequestError.ConnectionError,
                    };
                case AuthenticationException:
                    return HttpRequestError.SecureConnectionError;
                default:
                    break;
            }

            switch (source.Message)
            {
                case string message when message.Contains("HTTP/2") || message.Contains("HTTP/3"):
                    return HttpRequestError.HttpProtocolError;
                case string message when message.Contains("CONNECT") && message.Contains("WebSocket"):
                    return HttpRequestError.ExtendedConnectNotSupported;
                case string message when message.Contains("authentication"):
                    return HttpRequestError.UserAuthenticationError;
                case string message when message.Contains("version negotiation"):
                    return HttpRequestError.VersionNegotiationError;
                case string message when message.Contains("proxy tunnel"):
                    return HttpRequestError.ProxyTunnelError;
                case string message when message.Contains("malformed") || message.Contains("invalid"):
                    return HttpRequestError.InvalidResponse;
                case string message when message.Contains("response ended prematurely"):
                    return HttpRequestError.ResponseEnded;
                case string message when message.Contains("exceeded the limit"):
                    return HttpRequestError.ConfigurationLimitExceeded;
                default:
                    return HttpRequestError.Unknown;
            }
        }
    }
    
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws an exception if the System.Net.Http.HttpResponseMessage.IsSuccessStatusCode property for the HTTP response is false.
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="HttpTransmitException"></exception>
        public static void ThrowIfTransmitFailure(this HttpResponseMessage source)
        {
            try
            {
                source.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException e)
            {
                var errorCode = e.GetErrorCode();
                throw new HttpTransmitException(e)
                {
                    RequestError = errorCode,
                    StatusCode = source.StatusCode,
                    ResponseMessage = source
                };
            }
        }
    }
    public static class ArrayExtensions
    {
        //public static bool IsEmpty(this Array source) => source.Length == 0;
        public static Heap<T> AsHeap<T>(this T[] source) where T : unmanaged
        {
            return new Heap<T>(source);
        }
    }
    public static class Vector3Extensions
    {
        /// <summary>
        /// 获取一个在原点与当前坐标连线上并与当前坐标相隔<paramref name="distance"/>的坐标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetPoint(this Vector3 source,float distance)
        {
            var d = source.magnitude;
            var ratio = (d + distance) / d;
            return source * ratio;
        }
        /// <summary>
        /// 传入一个坐标作为原点，获取一个在原点与当前坐标连线上并与当前坐标相隔<paramref name="distance"/>的坐标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetPoint(this Vector3 source, Vector3 origin, float distance)
        {
            var v = source - origin;
            var d = v.magnitude;
            var ratio = (d + distance) / d;
            return v * ratio;
        }
    }
    public static class ListExtensions
    {
        public static bool IsEmpty<T>(this List<T> source) => source.Count == 0;
        public static bool TryGetElement<T>(this List<T> source,int index,out T? element)
        {
            element = default;
            if (index >= source.Count)
                return false;

            element = source[index];
            return true;
        }
    }
    public static class TypeExtensions
    {
        public static bool IsIntType(this Type source)
        {
            return source == typeof(int) || source == typeof(long) ||
                   source == typeof(short) || source == typeof(byte) ||
                   source == typeof(uint) || source == typeof(ulong) ||
                   source == typeof(ushort) || source == typeof(sbyte);
        }
        public static bool IsFloatType(this Type source)
        {
            return source == typeof(float) || source == typeof(double) ||
                   source == typeof(decimal);
        }
    }
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
    public static class IEnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source) => source.Count() == 0;
        public static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> source)
        {
            int index = 0;
            foreach (var item in source)
                yield return (index++, item);
        }
        public static T? Find<T>(this IEnumerable<T> source,Predicate<T> matcher)
        {
            foreach(var item in source)
                if(matcher(item))
                    return item;
            return default;
        }
        public static T[] FindAll<T>(this IEnumerable<T> source, Predicate<T> matcher)
        {
            List<T> items = new();
            foreach (var item in source)
                if (matcher(item))
                    items.Add(item);
            return items.ToArray();
        }
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> matcher)
        {
            foreach(var (index,item) in source.WithIndex())
                if (matcher(item))
                    return index;
            return -1;
        }
    }
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> ToEnumerable(this Transform source)
        {
            for(int i = 0; i < source.childCount;i++)
                yield return source.GetChild(i);
        }
        public static IEnumerable<Transform> GetChildren(this Transform source) => source.ToEnumerable();
        public static T? GetComponentInChildren<T>(this Transform source,int index)
        {
            if (index >= source.childCount)
                throw new IndexOutOfRangeException("Cannot get child at this object,because the index is out of range");
            return source.GetChild(index).GetComponent<T?>();
        }
    }
    public static class SensorTypeExtensions
    {
        /// <summary>
        /// Gets a touch panel area with a specified difference from the current touch panel area
        /// </summary>
        /// <param name="source">current touch panel area</param>
        /// <param name="diff">specified difference</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorType Diff(this SensorType source,int diff)
        {
            if(source > SensorType.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            diff = diff % 8;
            if (diff == 0)
                return source;
            else if (diff < 0)
                diff = 8 + diff;
            //var isReverse = diff < 0;
            var result = (source.GetIndex() - 1 + diff) % 8 ;
            var group = source.GetGroup();
            switch(group)
            {
                case SensorGroup.A:
                    return (SensorType)result;
                case SensorGroup.B:
                    result += 8;
                    return (SensorType)result;
                case SensorGroup.C:
                    return source;
                case SensorGroup.D:
                    result += 17;
                    return (SensorType)result;
                // SensorGroup.E
                default:
                    result += 25;
                    return (SensorType)result;
            }
        }
        /// <summary>
        /// Get the group where the touch panel area is located
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static SensorGroup GetGroup(this SensorType source)
        {
            if (source > SensorType.E8)
                throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area");
            var i = (int)source;
            if (i <= 7)
                return SensorGroup.A;
            else if (i <= 15)
                return SensorGroup.B;
            else if (i <= 16)
                return SensorGroup.C;
            else if (i <= 24)
                return SensorGroup.D;
            else
                return SensorGroup.E;
        }
        /// <summary>
        /// Get the index of the touch panel area within the group
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">throw exception when the touch panel is not in A1-E8</exception>
        public static int GetIndex(this SensorType source)
        {
            var group = source.GetGroup();
            return group switch
            {
                SensorGroup.A => (int)source + 1,
                SensorGroup.B => (int)source - 7,
                SensorGroup.C => 1,
                SensorGroup.D => (int)source - 16,
                SensorGroup.E => (int)source - 24,
                _ => throw new InvalidOperationException($"\"{source}\" is not a valid touch panel area")
            };
        }
        public static SensorType Mirror(this SensorType source,SensorType baseLine)
        {
            if (source == SensorType.C || source.IsCollinearWith(baseLine))
                return source;

            var thisIndex = source.GetIndex();
            var baseIndex = baseLine.GetIndex();
            var thisGroup = source.GetGroup();
            var baseGroup = baseLine.GetGroup();

            var AorB = thisGroup is SensorGroup.A or SensorGroup.B && baseGroup is SensorGroup.A or SensorGroup.B;
            var DorE = thisGroup is SensorGroup.D or SensorGroup.E && baseGroup is SensorGroup.D or SensorGroup.E;

            if(AorB || DorE)
            {
                var diff = baseIndex - thisIndex;

                if(thisGroup != baseGroup)
                {
                    var _baseLine = thisGroup switch
                    {
                        SensorGroup.A => (SensorType)(baseIndex - 1),
                        SensorGroup.B => (SensorType)(baseIndex - 1 + 8),
                        SensorGroup.D => (SensorType)(baseIndex - 1 + 17),
                        SensorGroup.E => (SensorType)(baseIndex - 1 + 25),
                        _ => throw new NotSupportedException("cnm")
                    };
                    return _baseLine.Diff(diff);
                }
                else
                    return baseLine.Diff(diff);
            }
            else
            {
                switch(baseLine)
                {
                    case SensorType.D1:
                    case SensorType.E1:
                        return source switch
                        {
                            SensorType.A1 => SensorType.A8,
                            SensorType.A2 => SensorType.A7,
                            SensorType.A3 => SensorType.A6,
                            SensorType.A4 => SensorType.A5,
                            SensorType.A5 => SensorType.A4,
                            SensorType.A6 => SensorType.A3,
                            SensorType.A7 => SensorType.A2,
                            SensorType.A8 => SensorType.A1,
                            SensorType.B1 => SensorType.B8,
                            SensorType.B2 => SensorType.B7,
                            SensorType.B3 => SensorType.B6,
                            SensorType.B4 => SensorType.B5,
                            SensorType.B5 => SensorType.B4,
                            SensorType.B6 => SensorType.B3,
                            SensorType.B7 => SensorType.B2,
                            SensorType.B8 => SensorType.B1,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D2:
                    case SensorType.E2:
                        return source switch
                        {
                            SensorType.A2 => SensorType.A1,
                            SensorType.A3 => SensorType.A8,
                            SensorType.A4 => SensorType.A7,
                            SensorType.A5 => SensorType.A6,
                            SensorType.A6 => SensorType.A5,
                            SensorType.A7 => SensorType.A4,
                            SensorType.A8 => SensorType.A3,
                            SensorType.A1 => SensorType.A2,
                            SensorType.B2 => SensorType.B1,
                            SensorType.B3 => SensorType.B8,
                            SensorType.B4 => SensorType.B7,
                            SensorType.B5 => SensorType.B6,
                            SensorType.B6 => SensorType.B5,
                            SensorType.B7 => SensorType.B4,
                            SensorType.B8 => SensorType.B3,
                            SensorType.B1 => SensorType.B2,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D3:
                    case SensorType.E3:
                        return source switch
                        {
                            SensorType.A3 => SensorType.A2,
                            SensorType.A4 => SensorType.A1,
                            SensorType.A5 => SensorType.A8,
                            SensorType.A6 => SensorType.A7,
                            SensorType.A7 => SensorType.A6,
                            SensorType.A8 => SensorType.A5,
                            SensorType.A1 => SensorType.A4,
                            SensorType.A2 => SensorType.A3,
                            SensorType.B3 => SensorType.B2,
                            SensorType.B4 => SensorType.B1,
                            SensorType.B5 => SensorType.B8,
                            SensorType.B6 => SensorType.B7,
                            SensorType.B7 => SensorType.B6,
                            SensorType.B8 => SensorType.B5,
                            SensorType.B1 => SensorType.B4,
                            SensorType.B2 => SensorType.B3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D4:
                    case SensorType.E4:
                        return source switch
                        {
                            SensorType.A4 => SensorType.A3,
                            SensorType.A5 => SensorType.A2,
                            SensorType.A6 => SensorType.A1,
                            SensorType.A7 => SensorType.A8,
                            SensorType.A8 => SensorType.A7,
                            SensorType.A1 => SensorType.A6,
                            SensorType.A2 => SensorType.A5,
                            SensorType.A3 => SensorType.A4,
                            SensorType.B4 => SensorType.B3,
                            SensorType.B5 => SensorType.B2,
                            SensorType.B6 => SensorType.B1,
                            SensorType.B7 => SensorType.B8,
                            SensorType.B8 => SensorType.B7,
                            SensorType.B1 => SensorType.B6,
                            SensorType.B2 => SensorType.B5,
                            SensorType.B3 => SensorType.B4,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D5:
                    case SensorType.E5:
                        return source switch
                        {
                            SensorType.A5 => SensorType.A4,
                            SensorType.A6 => SensorType.A3,
                            SensorType.A7 => SensorType.A2,
                            SensorType.A8 => SensorType.A1,
                            SensorType.A1 => SensorType.A8,
                            SensorType.A2 => SensorType.A7,
                            SensorType.A3 => SensorType.A6,
                            SensorType.A4 => SensorType.A5,
                            SensorType.B5 => SensorType.B4,
                            SensorType.B6 => SensorType.B3,
                            SensorType.B7 => SensorType.B2,
                            SensorType.B8 => SensorType.B1,
                            SensorType.B1 => SensorType.B8,
                            SensorType.B2 => SensorType.B7,
                            SensorType.B3 => SensorType.B6,
                            SensorType.B4 => SensorType.B5,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D6:
                    case SensorType.E6:
                        return source switch
                        {
                            SensorType.A6 => SensorType.A5,
                            SensorType.A7 => SensorType.A4,
                            SensorType.A8 => SensorType.A3,
                            SensorType.A1 => SensorType.A2,
                            SensorType.A2 => SensorType.A1,
                            SensorType.A3 => SensorType.A8,
                            SensorType.A4 => SensorType.A7,
                            SensorType.A5 => SensorType.A6,
                            SensorType.B6 => SensorType.B5,
                            SensorType.B7 => SensorType.B4,
                            SensorType.B8 => SensorType.B3,
                            SensorType.B1 => SensorType.B2,
                            SensorType.B2 => SensorType.B1,
                            SensorType.B3 => SensorType.B8,
                            SensorType.B4 => SensorType.B7,
                            SensorType.B5 => SensorType.B6,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D7:
                    case SensorType.E7:
                        return source switch
                        {
                            SensorType.A7 => SensorType.A6,
                            SensorType.A8 => SensorType.A5,
                            SensorType.A1 => SensorType.A4,
                            SensorType.A2 => SensorType.A3,
                            SensorType.A3 => SensorType.A2,
                            SensorType.A4 => SensorType.A1,
                            SensorType.A5 => SensorType.A8,
                            SensorType.A6 => SensorType.A7,
                            SensorType.B7 => SensorType.B6,
                            SensorType.B8 => SensorType.B5,
                            SensorType.B1 => SensorType.B4,
                            SensorType.B2 => SensorType.B3,
                            SensorType.B3 => SensorType.B2,
                            SensorType.B4 => SensorType.B1,
                            SensorType.B5 => SensorType.B8,
                            SensorType.B6 => SensorType.B7,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.D8:
                    case SensorType.E8:
                        return source switch
                        {
                            SensorType.A8 => SensorType.A7,
                            SensorType.A1 => SensorType.A6,
                            SensorType.A2 => SensorType.A5,
                            SensorType.A3 => SensorType.A4,
                            SensorType.A4 => SensorType.A3,
                            SensorType.A5 => SensorType.A2,
                            SensorType.A6 => SensorType.A1,
                            SensorType.A7 => SensorType.A8,
                            SensorType.B8 => SensorType.B7,
                            SensorType.B1 => SensorType.B6,
                            SensorType.B2 => SensorType.B5,
                            SensorType.B3 => SensorType.B4,
                            SensorType.B4 => SensorType.B3,
                            SensorType.B5 => SensorType.B2,
                            SensorType.B6 => SensorType.B1,
                            SensorType.B7 => SensorType.B8,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A1:
                    case SensorType.B1:
                        return source switch
                        {
                            SensorType.D1 => SensorType.D2,
                            SensorType.D2 => SensorType.D1,
                            SensorType.D3 => SensorType.D8,
                            SensorType.D4 => SensorType.D7,
                            SensorType.D5 => SensorType.D6,
                            SensorType.D6 => SensorType.D5,
                            SensorType.D7 => SensorType.D4,
                            SensorType.D8 => SensorType.D3,
                            SensorType.E1 => SensorType.E2,
                            SensorType.E2 => SensorType.E1,
                            SensorType.E3 => SensorType.E8,
                            SensorType.E4 => SensorType.E7,
                            SensorType.E5 => SensorType.E6,
                            SensorType.E6 => SensorType.E5,
                            SensorType.E7 => SensorType.E4,
                            SensorType.E8 => SensorType.E3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A2:
                    case SensorType.B2:
                        return source switch
                        {
                            SensorType.D3 => SensorType.D2,
                            SensorType.D4 => SensorType.D1,
                            SensorType.D5 => SensorType.D8,
                            SensorType.D6 => SensorType.D7,
                            SensorType.D7 => SensorType.D6,
                            SensorType.D8 => SensorType.D5,
                            SensorType.D1 => SensorType.D4,
                            SensorType.D2 => SensorType.D3,
                            SensorType.E3 => SensorType.E2,
                            SensorType.E4 => SensorType.E1,
                            SensorType.E5 => SensorType.E8,
                            SensorType.E6 => SensorType.E7,
                            SensorType.E7 => SensorType.E6,
                            SensorType.E8 => SensorType.E5,
                            SensorType.E1 => SensorType.E4,
                            SensorType.E2 => SensorType.E3,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A3:
                    case SensorType.B3:
                        return source switch
                        {
                            SensorType.D4 => SensorType.D3,
                            SensorType.D5 => SensorType.D2,
                            SensorType.D6 => SensorType.D1,
                            SensorType.D7 => SensorType.D8,
                            SensorType.D8 => SensorType.D7,
                            SensorType.D1 => SensorType.D6,
                            SensorType.D2 => SensorType.D5,
                            SensorType.D3 => SensorType.D4,
                            SensorType.E4 => SensorType.E3,
                            SensorType.E5 => SensorType.E2,
                            SensorType.E6 => SensorType.E1,
                            SensorType.E7 => SensorType.E8,
                            SensorType.E8 => SensorType.E7,
                            SensorType.E1 => SensorType.E6,
                            SensorType.E2 => SensorType.E5,
                            SensorType.E3 => SensorType.E4,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A4:
                    case SensorType.B4:
                        return source switch
                        { 
                            SensorType.D5 => SensorType.D4,
                            SensorType.D6 => SensorType.D3,
                            SensorType.D7 => SensorType.D2,
                            SensorType.D8 => SensorType.D1,
                            SensorType.D1 => SensorType.D8,
                            SensorType.D2 => SensorType.D7,
                            SensorType.D3 => SensorType.D6,
                            SensorType.D4 => SensorType.D5,
                            SensorType.E5 => SensorType.E4,
                            SensorType.E6 => SensorType.E3,
                            SensorType.E7 => SensorType.E2,
                            SensorType.E8 => SensorType.E1,
                            SensorType.E1 => SensorType.E8,
                            SensorType.E2 => SensorType.E7,
                            SensorType.E3 => SensorType.E6,
                            SensorType.E4 => SensorType.E5,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A5:
                    case SensorType.B5:
                        return source switch
                        {
                            SensorType.D6 => SensorType.D5,
                            SensorType.D7 => SensorType.D4,
                            SensorType.D8 => SensorType.D3,
                            SensorType.D1 => SensorType.D2,
                            SensorType.D2 => SensorType.D1,
                            SensorType.D3 => SensorType.D8,
                            SensorType.D4 => SensorType.D7,
                            SensorType.D5 => SensorType.D6,
                            SensorType.E6 => SensorType.E5,
                            SensorType.E7 => SensorType.E4,
                            SensorType.E8 => SensorType.E3,
                            SensorType.E1 => SensorType.E2,
                            SensorType.E2 => SensorType.E1,
                            SensorType.E3 => SensorType.E8,
                            SensorType.E4 => SensorType.E7,
                            SensorType.E5 => SensorType.E6,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A6:
                    case SensorType.B6:
                        return source switch
                        {
                            SensorType.D7 => SensorType.D6,
                            SensorType.D8 => SensorType.D5,
                            SensorType.D1 => SensorType.D4,
                            SensorType.D2 => SensorType.D3,
                            SensorType.D3 => SensorType.D2,
                            SensorType.D4 => SensorType.D1,
                            SensorType.D5 => SensorType.D8,
                            SensorType.D6 => SensorType.D7,
                            SensorType.E7 => SensorType.E6,
                            SensorType.E8 => SensorType.E5,
                            SensorType.E1 => SensorType.E4,
                            SensorType.E2 => SensorType.E3,
                            SensorType.E3 => SensorType.E2,
                            SensorType.E4 => SensorType.E1,
                            SensorType.E5 => SensorType.E8,
                            SensorType.E6 => SensorType.E7,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A7:
                    case SensorType.B7:
                        return source switch
                        {
                            SensorType.D8 => SensorType.D7,
                            SensorType.D1 => SensorType.D6,
                            SensorType.D2 => SensorType.D5,
                            SensorType.D3 => SensorType.D4,
                            SensorType.D4 => SensorType.D3,
                            SensorType.D5 => SensorType.D2,
                            SensorType.D6 => SensorType.D1,
                            SensorType.D7 => SensorType.D8,
                            SensorType.E8 => SensorType.E7,
                            SensorType.E1 => SensorType.E6,
                            SensorType.E2 => SensorType.E5,
                            SensorType.E3 => SensorType.E4,
                            SensorType.E4 => SensorType.E3,
                            SensorType.E5 => SensorType.E2,
                            SensorType.E6 => SensorType.E1,
                            SensorType.E7 => SensorType.E8,
                            _ => throw new NotSupportedException()
                        };
                    case SensorType.A8:
                    case SensorType.B8:
                        return source switch
                        {
                            SensorType.D1 => SensorType.D8,
                            SensorType.D2 => SensorType.D7,
                            SensorType.D3 => SensorType.D6,
                            SensorType.D4 => SensorType.D5,
                            SensorType.D5 => SensorType.D4,
                            SensorType.D6 => SensorType.D3,
                            SensorType.D7 => SensorType.D2,
                            SensorType.D8 => SensorType.D1,
                            SensorType.E1 => SensorType.E8,
                            SensorType.E2 => SensorType.E7,
                            SensorType.E3 => SensorType.E6,
                            SensorType.E4 => SensorType.E5,
                            SensorType.E5 => SensorType.E4,
                            SensorType.E6 => SensorType.E3,
                            SensorType.E7 => SensorType.E2,
                            SensorType.E8 => SensorType.E1,
                            _ => throw new NotSupportedException()
                        };
                    default:
                        throw new NotSupportedException();
                }
            }

        }
        public static bool IsCollinearWith(this SensorType source, SensorType target)
        {
            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();
            if (thisGroup is SensorGroup.C || targetGroup is SensorGroup.C)
                return true;

            var thisIndex = source.GetIndex();
            var targetIndex = target.GetIndex();
            
            if (thisGroup is (SensorGroup.A or SensorGroup.B) && targetGroup is (SensorGroup.A or SensorGroup.B))
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else if (thisGroup is (SensorGroup.D or SensorGroup.E) && targetGroup is (SensorGroup.D or SensorGroup.E))
                return thisIndex == targetIndex || Math.Abs(thisIndex - targetIndex) == 4;
            else
                return false;
        }
        public static bool IsLeftOf(this SensorType source, SensorType target)
        {
            if (source == SensorType.C || target == SensorType.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;

            var opposite = target.Diff(4);
            var thisIndex = source.GetIndex();
            var aIndex = target.GetIndex();
            var bIndex = opposite.GetIndex();
            var min = Math.Min(aIndex, bIndex);
            var max = Math.Max(aIndex, bIndex);

            var thisGroup = source.GetGroup();
            var targetGroup = target.GetGroup();

            var AorB = thisGroup is SensorGroup.A or SensorGroup.B && targetGroup is SensorGroup.A or SensorGroup.B;
            var DorE = thisGroup is SensorGroup.D or SensorGroup.E && targetGroup is SensorGroup.D or SensorGroup.E;
            if (AorB || DorE)
            {
                if (thisIndex > min && thisIndex < max)
                    return false;
                else
                    return true;
            }
            else
            {
                if(targetGroup is SensorGroup.A or SensorGroup.B)
                {
                    if (thisIndex > min && thisIndex <= max)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (thisIndex >= min && thisIndex < max)
                        return false;
                    else
                        return true;
                }
            }
        }
        public static bool IsRightOf(this SensorType source, SensorType target)
        {
            if (source == SensorType.C || target == SensorType.C)
                throw new InvalidOperationException("cnm");
            else if (source.IsCollinearWith(target))
                return false;
            else
                return !source.IsLeftOf(target);
        }
    }
    public static class MathExtensions
    {
        public static T Clamp<T>(this T source, in T min, in T max) where T : IComparable<T>
        {
            if (source.CompareTo(min) < 0)
                return min;
            else if (source.CompareTo(max) > 0)
                return max;
            else
                return source;
        }
        /// <summary>
        /// such like [<paramref name="min"/>,<paramref name="max"/>]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>if in range return true,else false</returns>
        public static bool InRange<T>(this T source, in T min, in T max) where T : IComparable<T>
        {
            return !(source.CompareTo(min) < 0 || source.CompareTo(max) > 0);
        }
    }
}