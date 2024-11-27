using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MychIO.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityRawInput;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        readonly static RawKey[] _bindingKeys = new RawKey[12]
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
        readonly Dictionary<SensorType, DateTime> _btnLastTriggerTimes = new();
        Button[] _buttons = new Button[12]
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
        void UpdateButtonState()
        {
            if (!_buttonCheckerMutex.WaitOne(4))
                return;
            var buttons = _buttons.AsSpan();
            foreach (var keyId in _bindingKeys.AsSpan())
            {
                var button = buttons.Find(x => x.BindingKey == keyId);
                if (button == null)
                {
                    Debug.LogError($"Key not found:\n{keyId}");
                    continue;
                }
                var oldState = button.Status;
                var newState = RawInput.IsKeyDown(keyId) ? SensorStatus.On : SensorStatus.Off;
                var now = DateTime.Now;
                if (oldState == newState)
                    continue;
                else if (JitterDetect(button.Type, now, true))
                    continue;
                _btnLastTriggerTimes[button.Type] = now;
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
                SetIdle(msg);
            }
            _buttonCheckerMutex.ReleaseMutex();
        }
        public void BindButton(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var button = GetButton(sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.AddSubscriber(checker);
        }
        public void UnbindButton(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var button = GetButton(sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.RemoveSubscriber(checker);
        }
        void OnKeyStateChanged(ButtonRingZone btnZone, InputState state)
        {
            var key = btnZone switch
            {
                ButtonRingZone.BA1 => RawKey.W,
                ButtonRingZone.BA2 => RawKey.E,
                ButtonRingZone.BA3 => RawKey.D,
                ButtonRingZone.BA4 => RawKey.C,
                ButtonRingZone.BA5 => RawKey.X,
                ButtonRingZone.BA6 => RawKey.Z,
                ButtonRingZone.BA7 => RawKey.A,
                ButtonRingZone.BA8 => RawKey.Q,
                ButtonRingZone.ArrowUp => RawKey.Multiply,
                ButtonRingZone.ArrowDown => RawKey.Numpad3,
                ButtonRingZone.Select => RawKey.Numpad9,
                ButtonRingZone.InsertCoin => RawKey.Numpad7,
                _ => throw new ArgumentOutOfRangeException("Does your 8-key game have 9 keys?")
            };
            var keyState = state is InputState.Off ? SensorStatus.Off : SensorStatus.On;
            OnKeyStateChanged(key, keyState);
        }
        void OnKeyStateChanged(RawKey key,SensorStatus state)
        {
            if (!_buttonCheckerMutex.WaitOne(4))
                return;
            if (_bindingKeys.All(x => x != key))
                return;
            var buttons = _buttons.AsSpan();
            var button = buttons.Find(x => x.BindingKey == key);
            if (button == null)
            {
                Debug.LogError($"Key not found:\n{key}");
                return;
            }
            var oldState = button.Status;
            var newState = state;
            var now = DateTime.Now;
            if (oldState == newState)
                return;
            else if (JitterDetect(button.Type, now, true))
                return;
            _btnLastTriggerTimes[button.Type] = now;
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
            SetIdle(msg);
            _buttonCheckerMutex.ReleaseMutex();
        }
        void OnRawKeyUp(RawKey key) => OnKeyStateChanged(key, SensorStatus.Off);
        void OnRawKeyDown(RawKey key) => OnKeyStateChanged(key, SensorStatus.On);
    }
}
