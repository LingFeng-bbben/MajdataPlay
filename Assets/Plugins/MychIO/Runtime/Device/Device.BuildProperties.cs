using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
    public abstract partial class Device<T1, T2, T3> : IDevice<T1, T2> where T1 : Enum where T3 : IConnectionProperties where T2 : Enum
    {
        // Properties used by DeviceFactory to construct the concrete class. These must be overridden via the new keyword! 
        public static ConnectionType GetConnectionType()
        {
            throw new NotImplementedException("Error GetConnectionType method not overwritten in base class");
        }
        public static DeviceClassification GetDeviceClassification()
        {
            throw new NotImplementedException("Error GetDeviceClassification method not overwitten in base class");
        }
        public static string GetDeviceName()
        {
            throw new NotImplementedException("Error GetDeviceName method not overwitten in base class");
        }
        public static IConnectionProperties GetDefaultConnectionProperties()
        {
            throw new NotImplementedException("Error GetDefaultConnectionProperties method not overwitten in base class");
        }

        // Helper method to access these static methods
        private static MethodInfo GetBaseClassStaticMethod(string method, Type type)
        {
            return type.GetMethod(
                method,
                BindingFlags.Static | BindingFlags.Public
            );
        }

    }
}
