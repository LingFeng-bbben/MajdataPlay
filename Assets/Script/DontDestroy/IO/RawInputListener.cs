using MajdataPlay.Extensions;
using MajdataPlay.Types;
using UnityEngine;
using UnityRawInput;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        Button[] buttons = new Button[8]
        {
            new Button(RawKey.W,SensorType.A1),
            new Button(RawKey.E,SensorType.A2),
            new Button(RawKey.D,SensorType.A3),
            new Button(RawKey.C,SensorType.A4),
            new Button(RawKey.X,SensorType.A5),
            new Button(RawKey.Z,SensorType.A6),
            new Button(RawKey.A,SensorType.A7),
            new Button(RawKey.Q,SensorType.A8),
        };
        void OnRawKeyUp(RawKey key)
        {
            var button = buttons.Find(x => x.BindingKey == key);
            if (button == null)
            {
                Debug.LogError($"Key not found:\n{key}");
                return;
            }
            var oldState = button.Status;
            var newState = SensorStatus.Off;
            Debug.Log($"Key \"{button.BindingKey}\": {newState}");
            var msg = new InputEventArgs()
            {
                Type = button.Type,
                OldStatus = oldState,
                Status = newState,
                IsButton = true
            };
            button.PushEvent(msg);
        }
        void OnRawKeyDown(RawKey key)
        {
            var button = buttons.Find(x => x.BindingKey == key);
            if (button == null)
            {
                Debug.LogError($"Key not found:\n{key}");
                return;
            }
            var oldState = button.Status;
            var newState = SensorStatus.On;
            Debug.Log($"Key \"{button.BindingKey}\": {newState}");
            var msg = new InputEventArgs()
            {
                Type = button.Type,
                OldStatus = oldState,
                Status = newState,
                IsButton = true
            };
            button.PushEvent(msg);
        }
    }
}
