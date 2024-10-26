using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Device;
using System.Linq;
using UnityEngine;
using MychIO.Event;
using System.Collections.Concurrent;

namespace MychIO
{
    using DeviceClassificationToInputAction = Dictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>;
    public class IOManager
    {

        public const string STANDARD_INPUT = "standard-input";
        // Handle concurrency from the input system
        public static readonly ConcurrentQueue<Action> ExecutionQueue = new ConcurrentQueue<Action>();
        // Devices
        protected IDictionary<DeviceClassification, IDevice> _deviceClassificationToDevice = new Dictionary<DeviceClassification, IDevice>();

        // InputHandling
        protected IDictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>> _deviceClassificationToDeviceInputAction = new Dictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>();
        protected IDictionary<string, IDictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>> _tagToDeviceClassificationToDeviceInputAction
        = new Dictionary<string, IDictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>>();

        // Event System
        protected IDictionary<IOEventType, ControllerEventDelegate> _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>();

        public IOManager() { }

        public void AddDeviceByName(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            DeviceClassification deviceClassification = DeviceClassification.Undefined,
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions = null
        )
        {
            if (DeviceClassification.Undefined == deviceClassification)
            {
                deviceClassification = DeviceFactory.GetClassificationFromDeviceName(deviceName);
            }
            if (DeviceClassification.Undefined == deviceClassification)
            {
                throw new Exception("Invalid classification passed to function");
            }
            if (null != inputSubscriptions)
            {
                _deviceClassificationToDeviceInputAction.Add(deviceClassification, inputSubscriptions);
            }
            else if (_deviceClassificationToDeviceInputAction.TryGetValue(deviceClassification, out var setDeviceClassification))
            {
                inputSubscriptions = setDeviceClassification;
            }

            Task.Run(async () =>
            {
                if (_deviceClassificationToDevice.TryGetValue(deviceClassification, out var oldDevice))
                {
                    await oldDevice.Disconnect();
                }

                var device = await DeviceFactory.GetDeviceAsync(
                    deviceName,
                    connectionProperties,
                    inputSubscriptions,
                    _deviceClassificationToDevice.Values.ToArray(),
                    this
                );
                _deviceClassificationToDevice.Add(device.GetClassification(), device);

                // Save for reloading
                _tagToDeviceClassificationToDeviceInputAction[STANDARD_INPUT] =
                new DeviceClassificationToInputAction { { deviceClassification, inputSubscriptions } };
            });
        }

        public void AddTouchPanel(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions = null
        )
        {
            EnsureAllInputStatesAreAccountedFor(inputSubscriptions);
            AddDeviceByName
           (
               deviceName,
               connectionProperties,
               DeviceClassification.TouchPanel,
               ConvertDictionary(inputSubscriptions)
           );
        }

        public void AddButtonRing(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions = null
        )
        {
            EnsureAllInputStatesAreAccountedFor(inputSubscriptions);
            AddDeviceByName
           (
               deviceName,
               connectionProperties,
               DeviceClassification.ButtonRing,
               ConvertDictionary(inputSubscriptions)
           );
        }

        public void AddLedDevice(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null
        )
        {
            AddDeviceByName
            (
                deviceName,
                connectionProperties,
                DeviceClassification.LedDevice,
                ConvertDictionary(new Dictionary<Enum, Action<Enum, Enum>>())
            );
        }

        private IDictionary<Enum, Action<Enum, Enum>> ConvertDictionary<T1, T2>(IDictionary<T1, Action<T1, T2>> dictionary) where T1 : Enum where T2 : Enum
        {
            var newDict = new Dictionary<Enum, Action<Enum, Enum>>();
            foreach (var kvp in dictionary)
            {
                newDict[kvp.Key] = (arg1, arg2) =>
                    kvp.Value((T1)arg1, (T2)arg2);
            }

            return newDict;
        }

        public async Task WriteToDevice(DeviceClassification deviceClassification, params Enum[] command)
        {
            if (_deviceClassificationToDevice.TryGetValue(deviceClassification, out var device) && device.IsConnected())
            {
                await device.Write(command);
            }
        }

        public void Destroy()
        {
            foreach (var (_, device) in _deviceClassificationToDevice)
            {
                device.Disconnect();
            }
            _deviceClassificationToDevice = new Dictionary<DeviceClassification, IDevice>();
            _deviceClassificationToDeviceInputAction = new Dictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>();
            _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>();
        }

        // Callbacks for Input
        // Both ChangeTouchPanelInputSubscriptions and ChangeButtonRingInputSubscriptions should use a common generic function.
        public void AddTouchPanelInputSubscriptions(
                IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions,
                string tag
            )
        {
            EnsureAllInputStatesAreAccountedFor(inputSubscriptions);
            AddDeviceInputSubscriptionsToTagMap(inputSubscriptions, DeviceClassification.TouchPanel, tag);
        }

        public void AddButtonRingInputSubscriptions(
            IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions,
            string tag)
        {
            EnsureAllInputStatesAreAccountedFor(inputSubscriptions);
            AddDeviceInputSubscriptionsToTagMap(inputSubscriptions, DeviceClassification.ButtonRing, tag);
        }

        public void ChangeTouchPanelInputSubscriptions(string tag)
        {
            Task.Run(async () =>
            {
                await ChangeDeviceInputSubscription<TouchPanelZone, InputState>(DeviceClassification.TouchPanel, tag);
            });
        }

        public void ChangeButtonRingInputSubscriptions(string tag)
        {
            Task.Run(async () =>
            {
                await ChangeDeviceInputSubscription<ButtonRingZone, InputState>(DeviceClassification.TouchPanel, tag);
            });
        }

        private async Task ChangeDeviceInputSubscription<T1, T2>(
            DeviceClassification deviceClassification,
            string tag
            ) where T1 : Enum where T2 : Enum
        {

            if (!_tagToDeviceClassificationToDeviceInputAction.TryGetValue(tag, out var deviceInputActionByTag))
            {
                throw new Exception($"No input actions found for the provided tag '{tag}'.");
            }

            if (!deviceInputActionByTag.TryGetValue(deviceClassification, out var inputAction))
            {
                throw new Exception($"No input action found for the device classification: '{deviceClassification}' under the tag '{tag}'.");
            }

            // Update the current input subscriptions on the IOManger 
            _deviceClassificationToDeviceInputAction[deviceClassification] = inputAction;

            // Load device
            if (_deviceClassificationToDevice.TryGetValue(deviceClassification, out IDevice device))
            {
                throw new ArgumentException($"device of classification: {deviceClassification} not found.");
            }

            // Update the devices input subscription
            if (inputAction is Dictionary<T1, Action<T1, T2>> typedInputAction)
            {
                // Update the device's input subscription
                await ((IDevice<T1, T2>)device).SetInputCallbacks(typedInputAction);
            }
            else
            {
                // impossible to reach here
                throw new InvalidCastException(
                    $"Input action for '{deviceClassification}' is not of the expected" +
                    " type Dictionary<{typeof(T1).Name}, Action<{typeof(T1).Name}, {typeof(T2).Name}>>."
                );
            }
        }

        private void AddDeviceInputSubscriptionsToTagMap<T1, T2>(IDictionary<T1, Action<T1, T2>>
         inputSubscriptions,
           DeviceClassification classification,
          string tag
           ) where T1 : Enum where T2 : Enum
        {
            // EnsureAllInputStatesAreAccountedFor(inputSubscriptions);
            var convertedSubscriptions = ConvertDictionary(inputSubscriptions);

            if (!_tagToDeviceClassificationToDeviceInputAction.ContainsKey(tag))
            {
                _tagToDeviceClassificationToDeviceInputAction[tag] = new DeviceClassificationToInputAction();
            }
            _tagToDeviceClassificationToDeviceInputAction[tag][classification] = convertedSubscriptions;

        }

        // All Enum otates must be accounted for, could potentially replace them with empty callbacks in the future
        // Instead of throwing an error
        private void EnsureAllInputStatesAreAccountedFor<T1, T2>(IDictionary<T1, Action<T1, T2>> inputSubscriptions)
         where T1 : Enum where T2 : Enum
        {
            foreach (T1 zone in Enum.GetValues(typeof(T1)))
            {
                if (inputSubscriptions.ContainsKey(zone))
                {
                    continue;
                }
                throw new ArgumentException($"InputSubscriptions must cover all {typeof(T1).Name} values.");
            }

        }

        // Events
        public void SubscribeToEvent(IOEventType eventType, ControllerEventDelegate callback)
        {
            _eventTypeToCallback[eventType] = callback;
        }

        public void SubscribeToEvents(IDictionary<IOEventType, ControllerEventDelegate> eventSubscriptions)
        {
            // clone to prevent side effects
            _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>(eventSubscriptions);
        }

        // For Internal use only
        public void handleEvent(
            IOEventType eventType,
            DeviceClassification deviceType = DeviceClassification.Undefined,
            string message = "N/A"
         )
        {
            if (!_eventTypeToCallback.TryGetValue(eventType, out ControllerEventDelegate eventDelegate))
            {
                return;
            }
            eventDelegate(eventType, deviceType, message);
        }

    }

}