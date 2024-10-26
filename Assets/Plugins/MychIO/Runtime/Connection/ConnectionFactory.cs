using System;
using System.Collections.Generic;
using System.Linq;
using MychIO.Connection.HidDevice;
using MychIO.Connection.SerialDevice;
using MychIO.Device;

namespace MychIO.Connection
{
    public class ConnectionFactory
    {
        // Potentially replace this dictionary with Reflection
        private static Dictionary<ConnectionType, Type> _connectionTypeToConnection = new()
        {
            { ConnectionType.HID, typeof(HidDeviceConnection) },
            { ConnectionType.SerialDevice, typeof(SerialDeviceConnection) }
            // Add other connections here...
        };
        internal static IConnection GetConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager)
        {

            var deviceType = device.GetType();
            var connectionTypeMethod = deviceType
                .GetMethod(
                    "GetConnectionType",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public
                 );
            var connectionType = (ConnectionType)connectionTypeMethod.Invoke(null, null);

            if (!_connectionTypeToConnection.TryGetValue(connectionType, out var connectionClassType))
            {
                throw new Exception("Could not find connection type for given device: " + device.GetType().Name);
            }

            var constructor = connectionClassType
                .GetConstructors()
                .First()
            ;

            if (constructor == null)
            {
                throw new Exception($"No suitable constructor found for device type {connectionClassType}");
            }

            return (IConnection)constructor.Invoke(new object[] { device, connectionProperties, manager });

        }
    }
}