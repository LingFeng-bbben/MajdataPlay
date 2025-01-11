using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;
using UnityRawInput;
using static UnityEngine.Rendering.DebugUI.Table;
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
        readonly Button[] _buttons = new Button[12]
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
        readonly bool[] _buttonStates = Enumerable.Repeat(false, 12).ToArray();
        async void RefreshKeyboardStateAsync()
        {
            await Task.Run(async () =>
            {
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();

                    for (var i = 0; i < _buttons.Length; i++)
                    {
                        var button = _buttons[i];
                        var keyCode = button.BindingKey;
                        _buttonStates[i] = RawInput.IsKeyDown(keyCode) ? true : false;
                    }
                    await Task.Delay(_btnPollingRateMs,token);
                }
            });
        }
        void UpdateButtonState()
        {
            var now = DateTime.Now;
            for (var i = 0; i < _buttons.Length; i++)
            {
                var button = _buttons[i];
                var oldState = button.Status;
                var newState = _buttonStates[i] ? SensorStatus.On : SensorStatus.Off;
                if (oldState == newState)
                    continue;
                else if(_isBtnDebounceEnabled)
                {
                    if (JitterDetect(button.Type, now, true))
                        continue;
                    _btnLastTriggerTimes[button.Type] = now;
                }
                button.Status = newState;
                MajDebug.Log($"Key \"{button.BindingKey}\": {newState}");
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
            //var buttons = _buttons.AsSpan();
            //foreach (var keyId in _bindingKeys.AsSpan())
            //{
            //    var button = buttons.Find(x => x.BindingKey == keyId);
            //    if (button == null)
            //    {
            //        MajDebug.LogError($"Key not found:\n{keyId}");
            //        continue;
            //    }
            //    var oldState = button.Status;
            //    var newState = RawInput.IsKeyDown(keyId) ? SensorStatus.On : SensorStatus.Off;
            //    var now = DateTime.Now;
            //    if (oldState == newState)
            //        continue;
            //    else if (JitterDetect(button.Type, now, true))
            //        continue;
            //    _btnLastTriggerTimes[button.Type] = now;
            //    button.Status = newState;
            //    MajDebug.Log($"Key \"{button.BindingKey}\": {newState}");
            //    var msg = new InputEventArgs()
            //    {
            //        Type = button.Type,
            //        OldStatus = oldState,
            //        Status = newState,
            //        IsButton = true
            //    };
            //    button.PushEvent(msg);
            //    PushEvent(msg);
            //    SetIdle(msg);
            //}
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
        RawKey ButtonRingZone2RawKey(ButtonRingZone btnZone)
        {
            return btnZone switch
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
        }
        int GetIndexByButtonRingZone(ButtonRingZone btnZone)
        {
            return btnZone switch
            {
                ButtonRingZone.BA1 => 0,
                ButtonRingZone.BA2 => 1,
                ButtonRingZone.BA3 => 2,
                ButtonRingZone.BA4 => 3,
                ButtonRingZone.BA5 => 4,
                ButtonRingZone.BA6 => 5,
                ButtonRingZone.BA7 => 6,
                ButtonRingZone.BA8 => 7,
                ButtonRingZone.ArrowUp => 9,
                ButtonRingZone.ArrowDown => 11,
                ButtonRingZone.Select => 8,
                ButtonRingZone.InsertCoin => 10,
                _ => throw new ArgumentOutOfRangeException("Does your 8-key game have 9 keys?")
            };
        }
        //void OnKeyStateChanged(ButtonRingZone btnZone, InputState state)
        //{
        //    var key = btnZone switch
        //    {
        //        ButtonRingZone.BA1 => RawKey.W,
        //        ButtonRingZone.BA2 => RawKey.E,
        //        ButtonRingZone.BA3 => RawKey.D,
        //        ButtonRingZone.BA4 => RawKey.C,
        //        ButtonRingZone.BA5 => RawKey.X,
        //        ButtonRingZone.BA6 => RawKey.Z,
        //        ButtonRingZone.BA7 => RawKey.A,
        //        ButtonRingZone.BA8 => RawKey.Q,
        //        ButtonRingZone.ArrowUp => RawKey.Multiply,
        //        ButtonRingZone.ArrowDown => RawKey.Numpad3,
        //        ButtonRingZone.Select => RawKey.Numpad9,
        //        ButtonRingZone.InsertCoin => RawKey.Numpad7,
        //        _ => throw new ArgumentOutOfRangeException("Does your 8-key game have 9 keys?")
        //    };
        //    var keyState = state is InputState.Off ? SensorStatus.Off : SensorStatus.On;
        //    OnKeyStateChanged(key, keyState);
        //}

        //void OnKeyStateChanged(RawKey key,SensorStatus state)
        //{
        //    if (!_buttonCheckerMutex.WaitOne(4))
        //        return;
        //    if (_bindingKeys.All(x => x != key))
        //        return;
        //    var buttons = _buttons.AsSpan();
        //    var button = buttons.Find(x => x.BindingKey == key);
        //    if (button == null)
        //    {
        //        MajDebug.LogError($"Key not found:\n{key}");
        //        return;
        //    }
        //    var oldState = button.Status;
        //    var newState = state;
        //    var now = DateTime.Now;
        //    if (oldState == newState)
        //        return;
        //    else if (JitterDetect(button.Type, now, true))
        //        return;
        //    _btnLastTriggerTimes[button.Type] = now;
        //    button.Status = newState;
        //    MajDebug.Log($"Key \"{button.BindingKey}\": {newState}");
        //    var msg = new InputEventArgs()
        //    {
        //        Type = button.Type,
        //        OldStatus = oldState,
        //        Status = newState,
        //        IsButton = true
        //    };
        //    button.PushEvent(msg);
        //    PushEvent(msg);
        //    SetIdle(msg);
        //    _buttonCheckerMutex.ReleaseMutex();
        //}

        //void OnRawKeyUp(RawKey key) => OnKeyStateChanged(key, SensorStatus.Off);
        //void OnRawKeyDown(RawKey key) => OnKeyStateChanged(key, SensorStatus.On);
    }
}
