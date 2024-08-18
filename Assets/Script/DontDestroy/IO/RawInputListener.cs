using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Linq;
using UnityEngine;
using UnityRawInput;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        readonly static RawKey[] bindingKeys = new RawKey[12]
        {
            RawKey.W,
            RawKey.E,
            RawKey.D,
            RawKey.C,
            RawKey.X,
            RawKey.Z,
            RawKey.A,
            RawKey.Q,
            RawKey.Numpad9,
            RawKey.Multiply,
            RawKey.Numpad7,
            RawKey.Numpad3,
        };
        Button[] buttons = new Button[12]
        {
            new Button(RawKey.W,SensorType.A1),
            new Button(RawKey.E,SensorType.A2),
            new Button(RawKey.D,SensorType.A3),
            new Button(RawKey.C,SensorType.A4),
            new Button(RawKey.X,SensorType.A5),
            new Button(RawKey.Z,SensorType.A6),
            new Button(RawKey.A,SensorType.A7),
            new Button(RawKey.Q,SensorType.A8),
            new Button(RawKey.Numpad9,SensorType.Test),
            new Button(RawKey.Multiply,SensorType.P1),
            new Button(RawKey.Numpad7,SensorType.Service),
            new Button(RawKey.Numpad3,SensorType.P2),
        };
        public void BindButton(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var button = buttons.Find(x => x?.Type == sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.OnStatusChanged += checker;
        }
        public void UnbindButton(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var button = buttons.Find(x => x?.Type == sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.OnStatusChanged -= checker;
        }
        void OnRawKeyUp(RawKey key)
        {
            if (bindingKeys.All(x => x != key))
                return;
            var button = buttons.Find(x => x.BindingKey == key);
            if (button == null)
            {
                Debug.LogError($"Key not found:\n{key}");
                return;
            }
            var oldState = button.Status;
            var newState = SensorStatus.Off;
            button.Status = newState;
            Debug.Log($"Key \"{button.BindingKey}\": {newState}");
            var msg = new InputEventArgs()
            {
                Type = button.Type,
                OldStatus = oldState,
                Status = newState,
                IsButton = true
            };
            button.PushEvent(msg);
            PushEvent(msg);
        }
        void OnRawKeyDown(RawKey key)
        {
            if (bindingKeys.All(x => x != key))
                return;
            var button = buttons.Find(x => x.BindingKey == key);
            if (button == null)
            {
                Debug.LogError($"Key not found:\n{key}");
                return;
            }
            var oldState = button.Status;
            var newState = SensorStatus.On;
            button.Status = newState;
            Debug.Log($"Key \"{button.BindingKey}\": {newState}");
            var msg = new InputEventArgs()
            {
                Type = button.Type,
                OldStatus = oldState,
                Status = newState,
                IsButton = true
            };
            button.PushEvent(msg);
            PushEvent(msg);
        }
    }
}
