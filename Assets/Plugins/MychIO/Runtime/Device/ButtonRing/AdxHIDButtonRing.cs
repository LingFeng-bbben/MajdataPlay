using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;
using MychIO.Helper;
using UnityEditorInternal;
using UnityEngine;

namespace MychIO.Device
{
    public class AdxHIDButtonRing : Device<ButtonRingZone, InputState, HidDeviceProperties>
    {

        /*              byte index
            No Input:   ?
            BA1:        4 
            BA2:        3
            BA3:        2
            BA4:        1
            BA5:        8
            BA6:        7
            BA7:        6
            BA8:        5
            up :        9
            select:     10
            down:       11
            coin:       12
        */

        public const string DEVICE_NAME = "AdxHIDButtonRing";
        // Rather hardcode it here for micro optimization if you need different 
        // settings just copy this class and change these values
        public const int BUFFER_SIZE = 13;
        public const int LEFT_BYTES_TO_TRUNCATE = 1;
        public const int BYTES_TO_READ = 12;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.HID;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.ButtonRing;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x2e3c,
            productId: 0x5750,
            bufferSize: BUFFER_SIZE,
            leftBytesToTruncate: LEFT_BYTES_TO_TRUNCATE,
            bytesToRead: BYTES_TO_READ
        );
        // ** Connection Properties
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,
            0x00,0x00
        };
        private byte[] _currentState = NO_INPUT_PACKET;
        private IDictionary<ButtonRingZone, bool> _currentActiveStates;
        public static readonly IDictionary<ButtonRingCommand, byte[]> Commands = new Dictionary<ButtonRingCommand, byte[]> { };

        public AdxHIDButtonRing(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<ButtonRingZone, bool>();
            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        public unsafe override void ReadData(IntPtr pointer)
        {

            /*
                if the code below causes any crashes or issues it might be better to 
                change this function to safe and copy the bytes this way.
                This is much slower though:

                byte[] currentInput = new byte[BYTES_TO_READ];

                Marshal.Copy(pointer, currentInput, 0, BYTES_TO_READ);
            **/
            /** UNSAFE CODE */
            if (pointer == IntPtr.Zero)
            {
                return;
            }

            byte[] currentInput = new byte[BYTES_TO_READ];
            byte* pByte = (byte*)pointer;
            for (int i = 0; i < BYTES_TO_READ; i++)
            {
                currentInput[i] = *(pByte + i);
            }
            /** UNSAFE CODE */

            // Check if the state has changed
            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }

            handleInputChange(ButtonRingZone.BA3, currentInput[1]);

            handleInputChange(ButtonRingZone.ArrowUp, currentInput[8]);

            handleInputChange(ButtonRingZone.BA1, currentInput[3]);

            handleInputChange(ButtonRingZone.BA2, currentInput[2]);

            handleInputChange(ButtonRingZone.ArrowDown, currentInput[10]);

            handleInputChange(ButtonRingZone.BA4, currentInput[0]);

            handleInputChange(ButtonRingZone.BA5, currentInput[7]);

            handleInputChange(ButtonRingZone.BA6, currentInput[6]);

            handleInputChange(ButtonRingZone.BA7, currentInput[5]);

            handleInputChange(ButtonRingZone.BA8, currentInput[4]);

            handleInputChange(ButtonRingZone.Select, currentInput[9]);

            handleInputChange(ButtonRingZone.InsertCoin, currentInput[11]);

            _currentState = currentInput;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void handleInputChange(ButtonRingZone zone, byte input)
        {
            _currentActiveStates.TryGetValue(zone, out var currentActiveState);
            if ((input == LEAST_SIGNIFICANT_BIT) != currentActiveState)
            {
                _inputSubscriptions.TryGetValue(zone, out var callback);
                var newState = currentActiveState ? InputState.Off : InputState.On;
                callback(zone, newState);
                _currentActiveStates[zone] = !currentActiveState;
            }
        }


        // source: https://stackoverflow.com/a/48599119
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        // Not used
        public override Task Write(params Enum[] interactions)
        {
            return Task.CompletedTask;
        }
        public override Task OnStartWrite()
        {
            return Task.CompletedTask;
        }
        public override void ReadData(byte[] data) { }

        public override Task OnDisconnectWrite()
        {
            return Task.CompletedTask;
        }
    }


}