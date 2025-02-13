using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;

                stopwatch.Start();
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var now = DateTime.Now;
                        for (var i = 0; i < _buttons.Length; i++)
                        {
                            var button = _buttons[i];
                            var keyCode = button.BindingKey;

                            _buttonRingInputBuffer.Enqueue(new ()
                            {
                                Index = i,
                                State = RawInput.IsKeyDown(keyCode) ? SensorStatus.On : SensorStatus.Off,
                                Timestamp = now
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"From KeyBoard listener: \n{e}");
                    }
                    finally
                    {
                        var t2 = stopwatch.Elapsed;
                        var elapsed = t2 - t1;
                        t1 = t2;
                        if (elapsed < pollingRate)
                            await Task.Delay(pollingRate - elapsed, token);
                    }
                }
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateButtonState()
        {
            while(_buttonRingInputBuffer.TryDequeue(out var report))
            {
                if (!report.Index.InRange(0, 11))
                    continue;
                var button = _buttons[report.Index];
                var oldState = button.Status;
                var newState = report.State;
                var timestamp = report.Timestamp;

                if (oldState == newState)
                    continue;
                else if (_isBtnDebounceEnabled)
                {
                    if (JitterDetect(button.Type, timestamp, true))
                        continue;
                    _btnLastTriggerTimes[button.Type] = timestamp;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}
