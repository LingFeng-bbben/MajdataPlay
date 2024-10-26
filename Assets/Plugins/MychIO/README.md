# MychIO

Input manager that supports Mai2 controllers in Unity.

# Adding to Unity

To add this package to your project simply copy the clone link and go to Window->Package Manager. Hit the + in the window and select add package from git Url.

# API

Full example of using the package is viewable [ here ](https://github.com/istareatscreens/MychIODev/blob/master/Assets/Scripts/TestBoard.cs)

## Initialization

Add the following imports to your project

```C#
using MychIO.Device;
using MychIO;
using MychIO.Event;
```

Retrieve the ExecutionQueue (type `ConcurrentQueue<Action>`) from the IOManager and Instantiate it

```C#
_executionQueue = IOManager.ExecutionQueue;
_ioManager = new IOManager();
```

The executionQueue is used to store callbacks that will be executed on the main thread. Since this input manager uses threading you must execute all code in the context of the ExecutionQueue. Otherwise undefined/undesirable behaviour will likely be observed.

## Event System

To subscribe to error events use IOEventType and generate a Dictionary of callbacks:

```C#
var eventCallbacks = new Dictionary<IOEventType, ControllerEventDelegate>{
            { IOEventType.Attach,
                (eventType, deviceType, message) =>
                {
                    # Note: appendEventText uses the IOManagers ExecutionQueue
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.ConnectionError,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.Debug,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.Detach,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.SerialDeviceReadError,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            }
        };
```

Then pass the callbacks into the instantiated IOManager

```C#
    _ioManager.SubscribeToEvents(eventCallbacks);
```

## Connecting To Devices

Currently this system supports three device types specified in `MychIO.Device.DeviceClassification`.

Each device type has specified interaction zones (type Enums):

Interaction Zones:

- `MychIO.Device.TouchPanelZone`
- `MychIO.Device.ButtonRingZone`

Defined State (type Enum) of these zones:

- `MychIO.Device.InputState`

Using these enums we can construct callbacks that are executed only when a specific zones input state changes. Doing this we can specify any specific logic we want to trigger when these states are changed.

An example of generating these callbacks:

```C#
var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();
foreach (TouchPanelZone touchZone in System.Enum.GetValues(typeof(TouchPanelZone)))
{
    // _touchIndicatorMap is a map of TouchPanelZone => GameObject
    if (!_touchIndicatorMap.TryGetValue(touchZone, out var touchIndicator))
    {
        throw new Exception($"Failed to find GameObject for {touchZone}");
    }

    touchPanelCallbacks[touchZone] = (TouchPanelZone input, InputState state) =>
    {
        _executionQueue.Enqueue(() =>
        {
            // In this execution queue callback we any changes we need to (This will happen on the MainThread)
            touchIndicator.SetActive(state == InputState.On);
        });
    };
}
```

You can connect to the three supported devices (TouchPanel, ButtonRing and LedDevice) using the following functions as follows:

```C#
_ioManager
    .AddTouchPanel(
        AdxTouchPanel.GetDeviceName(),
        inputSubscriptions: touchPanelCallbacks
    );
```

```C#
_ioManager.AddButtonRing(
    AdxIO4ButtonRing.GetDeviceName(),
    inputSubscriptions: buttonRingCallbacks
);
```

```C#
_ioManager.AddLedDevice(
   AdxLedDevice.GetDeviceName()
);
```

Method Signatures for these methods are as follows:

```C#
public void AddTouchPanel(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null,
    IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions = null
)
```

```C#
public void AddButtonRing(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null,
    IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions = null
)
```

```C#
public void AddLedDevice(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null
)
```

Where,

- deviceName is the unique name of the device e.g. (AdxButtonRing)
- connectionProperties - stores the properties specific to their connection interface. These can be used to overwrite the default device connection properties. These properties implement the `MychIO.Connection.IConnectionProperties` interface and can be easily serialized/unserialized using the an instantiated IConnection class (IDictionary<string, dynamic> <==> concrete IConnection object)
- inputSubscriptions - Callbacks that are triggered by controller interaction mapped by device interaction zone enum

## Adding Custom Connection Properties to A Device

To add connection properties to a device you acquire instantiate a new ConnectionProperties class for specific device you can instantiate an appropriate IConnnectionProperties class and use its copy constructor. You can call the GetDefaultConnectionProperties method on the specific device you want to generate a properties object for and then pass whatever other properties you wish to change.
Then call the `GetProperties` method on this properties object to serialize them.

```C#
        var propertiesTouchPanel = new SerialDeviceProperties(
            (SerialDeviceProperties)AdxTouchPanel.GetDefaultConnectionProperties(),
            comPortNumber: "COM10"
        ).GetProperties();

        _ioManager
            .AddTouchPanel(
                AdxTouchPanel.GetDeviceName(),
                propertiesTouchPanel,
                inputSubscriptions: touchPanelCallbacks
        );
```

The current implemented devices have the following connection objects:

| Device Concrete Class | ConnectionProperties Object |
| --------------------- | --------------------------- |
| `AdxTouchPanel`       | `SerialDeviceProperties`    |
| `AdxLedDevice`        | `SerialDeviceProperties`    |
| `AdxIO4ButtonRing`    | `HidDeviceProperties`       |
| `AdxHIDButtonRing`    | `HidDeviceProperties`       |

## Writing to Devices

Once the connection to a device is established you can send commands to execute specific functions using the follow IOManager method:

```C#
public async Task WriteToDevice(DeviceClassification deviceClassification, params Enum[] command)
```

Where,

- deviceClassification is the classification of the device see `MychIO.Device.DeviceClassification`
- command is a variadic parameter that takes one or more Enum commands specified in the following Enums
  - `MychIO.Device.ButtonRingCommand`
  - `MychIO.Device.TouchPanelCommand`
  - `MychIO.Device.LedDeviceCommand`

## Changing Interaction Subscriptions

Since controller input needs to be dynamic based on scene (i.e. menu versus in game) callbacks can be changed and reloaded using the following functions.

### Adding/Replacing Input (Subscription) callbacks

```C#
public void AddTouchPanelInputSubscriptions(
    IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions,
    string tag
)

public void AddButtonRingInputSubscriptions(
    IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions,
    string tag
)
```

Where,

- inputSubscriptions are a list of callbacks mapped to input zone
- tag is a unique identifier specifying which callbacks to load. When you first connect a device its inputSubscriptions are saved under the tag `MychIO.IOManager.STANDARD_INPUT`(type: string)

_Important Note_: this will only update the internal state of the IOManager itself, if you overwrite a tag that already exists and is currently loaded it will not change what the device is currently using for inputSubscriptions. You must call the Change methods. This is intentional as to prevent side effects.

### Switch Input (Subscription) callbacks

To change what callbacks your zones are subscribed to (i.e. update a devices subscription callbacks) you can call the following functions. Internally this will halt reading on the device, then replace the callbacks then start reading again:

```C#
public void ChangeTouchPanelInputSubscriptions(string tag)
```

```C#
public void ChangeButtonRingInputSubscriptions(string tag)
```

Where,

- tag specifies a Dictionary of InputSubscriptions if a tag does not exist it will throw an exception

## Executing input callbacks

To execute the Subscription callbacks simply add the following to your Update() method in unity:

```C#
    while (_executionQueue.TryDequeue(out var action))
    {
        action();
    }
```

This will execute all callbacks passed to the queue every frame

# Testing and Development

For integration of other controllers, testing, contributing there exists a unity development project that can be cloned [here](https://github.com/istareatscreens/MychIODev).

## Adding a Device

To integrate a Serial or HID device it is as simple as going copying one of the already used classes `AdxTouchPanel` or `AdxIO4ButtonRing` or `AdexLedDevice` and modifying it to meet your needs.

After creation of your custom controller interface class simply go to DeviceFactory and add your deviceName to the following Dictionary to facilitate loading it using IOManager:

```C#
private static Dictionary<string, Type> _deviceNameToType = new()
{
    { AdxTouchPanel.GetDeviceName(), typeof(AdxTouchPanel) },
    { AdxIO4ButtonRing.GetDeviceName(), typeof(AdxIO4ButtonRing) },
    { AdxHIDButtonRing.GetDeviceName(), typeof(AdxHIDButtonRing) },
    { AdxLedDevice.GetDeviceName(), typeof(AdxLedDevice) },
    // Add other devices here...
};
```

## Adding a Device Command

Each Device type has a Enum that specifies specific Commands:

- `LedDeviceCommand`
- `ButtonRingCommand`
- `TouchPanelCommand`

These commands are setup in their respected device classes and can be made as complex as required for example `TouchPanelCommands` are placed in the concerete class as follows:

```C#
public static readonly IDictionary<TouchPanelCommand, byte[][]> Commands = new Dictionary<TouchPanelCommand, byte[][]>
{
    { TouchPanelCommand.Start, new byte[][] { new byte[] { 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } } },
    { TouchPanelCommand.Reset, new byte[][] { new byte[] { 0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D } } },
    { TouchPanelCommand.Halt, new byte[][] { new byte[] { 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D } } },
};
```
