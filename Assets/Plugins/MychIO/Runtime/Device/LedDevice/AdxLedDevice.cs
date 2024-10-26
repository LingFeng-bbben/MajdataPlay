using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;
using MychIO.Helper;
using UnityEngine;

namespace MychIO.Device
{
    public class AdxLedDevice : Device<LedInteractions, InputState, SerialDeviceProperties>
    {
        public const string DEVICE_NAME = "AdxLedDevice";

        // Settings for microoptimization
        public const int BYTES_TO_READ = 9;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.LedDevice;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new SerialDeviceProperties(
            comPortNumber: "COM21",
            writeTimeoutMS: SerialDeviceProperties.DEFAULT_WRITE_TIMEOUT_MS,
            bufferByteLength: 9,
            pollingRateMs: 10,
            portNumber: 0,
            baudRate: BaudRate.Bd115200,
            stopBit: StopBits.One,
            parityBit: Parity.None,
            dataBits: DataBits.Eight,
            handshake: Handshake.None,
            dtr: false,
            rts: false
        );
        // ** Connection Properties 
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private byte[] _currentState = NO_INPUT_PACKET;
        //private byte[] _currentInput = new byte[BYTES_TO_READ];
        private IDictionary<LedInteractions, bool> _currentActiveStates;

        public static readonly IDictionary<LedCommand, byte[][]> Commands = new Dictionary<LedCommand, byte[][]>
        {
            { LedCommand.ClearAll, new byte[][] {
                    new byte[] {0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6C},
                    new byte[] {0xE0, 0x11, 0x01, 0x04, 0x39, 0x00, 0x00, 0x00, 0x4F},
                    new byte[] {0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F
                  }
                }
            },
        };

        public AdxLedDevice(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<LedInteractions, bool>();
            foreach (LedInteractions zone in Enum.GetValues(typeof(LedInteractions)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override async Task OnStartWrite()
        {
            // Establish connection with LED device
            foreach (var command in new byte[][]{
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23}
            }
            )
            {
                await _connection.Write(command);
            }
            await Write(LedCommand.ClearAll);

        }

        public async override Task OnDisconnectWrite()
        {
            await Write(LedCommand.ClearAll);
        }

        public override void ReadData(byte[] data)
        {
            Debug.Log(data);
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        public override async Task Write(params Enum[] interactions)
        {
            var commandBytes = interactions.OfType<LedCommand>()
            .SelectMany(command =>
            {
                if (Commands.TryGetValue(command, out byte[][] bytes))
                {
                    return bytes;
                }
                else
                {
                    throw new ArgumentException("Command not found.", nameof(command));
                }
            }).ToArray();

            foreach (var command in commandBytes)
            {
                await _connection.Write(command);
            }
        }

        // source: https://stackoverflow.com/a/48599119
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        // Not used
        public override void ReadData(IntPtr intPtr)
        {
            throw new NotImplementedException();
        }


    }

}