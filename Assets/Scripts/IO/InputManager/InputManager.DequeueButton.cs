using Cysharp.Text;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
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
            
            Span<SwitchStatus> newStates = stackalloc SwitchStatus[12];

            while (_buttonRingInputBuffer.TryDequeue(out var report))
            {
                var index = report.Index;
                if (!index.InRange(0, 11))
                {
                    continue;
                }
                newStates[index] |= report.State;
            }

            for (var i = 0; i < 12; i++)
            {
                var state = (ButtonRing.IsOn(i) || ButtonRing.IsHadOn(i)) ? SwitchStatus.On : SwitchStatus.Off;
                newStates[i] |= state;
            }

            for (var i = 0; i < 12; i++)
            {
                var button = buttons[i];
                var oldState = button.State;
                var newState = newStates[i];

                if (oldState == newState)
                {
                    continue;
                }
                if (_isBtnDebounceEnabled)
                {
                    if (JitterDetect(button.Zone, now))
                    {
                        continue;
                    }
                    _btnLastTriggerTimes[i] = now;
                }
                button.State = newState;
                MajDebug.LogDebug(ZString.Format("Key \"{0}\": {1}", button.BindingKey, newState));
                var msg = new InputEventArgs()
                {
                    BZone = button.Zone,
                    OldStatus = oldState,
                    Status = newState,
                    IsButton = true
                };
                button.PushEvent(msg);
                PushEvent(msg);
            }
        }
        public static void BindButton(EventHandler<InputEventArgs> checker, ButtonZone zone)
        {
            var button = GetButton(zone);
            if (button == null)
            {
                throw new Exception($"{zone} Button not found.");
            }
            button.AddSubscriber(checker);
        }
        public static void UnbindButton(EventHandler<InputEventArgs> checker, ButtonZone zone)
        {
            var button = GetButton(zone);
            if (button == null)
            {
                throw new Exception($"{zone} Button not found.");
            }
            button.RemoveSubscriber(checker);
        }
    }
}
