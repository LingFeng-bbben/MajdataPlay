using System;
using System.Runtime.CompilerServices;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        class Button : IEventPublisher<EventHandler<InputEventArgs>>
        {
            public KeyCode BindingKey
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }
            public ButtonZone Zone
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }
            /// <summary>
            /// Update by InputManager.PreUpdate
            /// </summary>
            public SwitchStatus State
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            event EventHandler<InputEventArgs>? OnStatusChanged;
            public Button(KeyCode bindingKey, ButtonZone zone)
            {
                BindingKey = bindingKey;
                Zone = zone;
                State = SwitchStatus.Off;
                OnStatusChanged = null;
            }
            public void AddSubscriber(EventHandler<InputEventArgs> handler)
            {
                OnStatusChanged += handler;
            }
            public void RemoveSubscriber(EventHandler<InputEventArgs> handler)
            {
                if (OnStatusChanged is not null)
                {
                    OnStatusChanged -= handler;
                }
            }
            public void PushEvent(in InputEventArgs args)
            {
                if (OnStatusChanged is not null)
                {
                    OnStatusChanged(this, args);
                }
            }
            public void ClearSubscriber() => OnStatusChanged = null;
        }
    }
}