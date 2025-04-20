using Cysharp.Text;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using MychIO.Device;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateButtonState()
        {
            var buttons = _buttons.Span;
            var now = MajTimeline.UnscaledTime;
            
            Span<SensorStatus> newStates = stackalloc SensorStatus[12];

            while (_buttonRingInputBuffer.TryDequeue(out var report))
            {
                var index = report.Index;
                if (!index.InRange(0, 11))
                    continue;
                newStates[index] |= report.State;
            }

            for (var i = 0; i < 12; i++)
            {
                var state = (ButtonRing.IsOn(i) || ButtonRing.IsHadOn(i)) ? SensorStatus.On : SensorStatus.Off;
                newStates[i] |= state;
            }

            for (var i = 0; i < 12; i++)
            {
                var button = buttons[i];
                var oldState = button.State;
                var newState = newStates[i];

                if (oldState == newState)
                    continue;
                if (_isBtnDebounceEnabled && i.InRange(0, 7))
                {
                    if (JitterDetect(button.Area, now, true))
                        continue;
                    _btnLastTriggerTimes[i] = now;
                }
                button.State = newState;
                MajDebug.Log(ZString.Format("Key \"{0}\": {1}", button.BindingKey, newState));
                var msg = new InputEventArgs()
                {
                    Type = button.Area,
                    OldStatus = oldState,
                    Status = newState,
                    IsButton = true
                };
                button.PushEvent(msg);
                PushEvent(msg);
            }
        }
        public static void BindButton(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var button = GetButton(sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.AddSubscriber(checker);
        }
        public static void UnbindButton(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var button = GetButton(sType);
            if (button == null)
                throw new Exception($"{sType} Button not found.");
            button.RemoveSubscriber(checker);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetIndexByButtonRingZone(ButtonRingZone btnZone)
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
