#nullable enable
using MajdataPlay.IO;
using System;
using System.Runtime.CompilerServices;

namespace MajdataPlay.IO
{
    internal class Button: IEventPublisher<EventHandler<InputEventArgs>>
    {
        public KeyCode BindingKey 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        }
        public SensorArea Area 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        }
        /// <summary>
        /// Update by InputManager.PreUpdate
        /// </summary>
        public SensorStatus State 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        }

        event EventHandler<InputEventArgs>? OnStatusChanged;
        public Button(KeyCode bindingKey, SensorArea type)
        {
            BindingKey = bindingKey;
            Area = type;
            State = SensorStatus.Off;
            OnStatusChanged = null;
        }
        public void AddSubscriber(EventHandler<InputEventArgs> handler)
        {
            OnStatusChanged += handler;
        }
        public void RemoveSubscriber(EventHandler<InputEventArgs> handler)
        {
            if (OnStatusChanged is not null)
                OnStatusChanged -= handler;
        }
        public void PushEvent(in InputEventArgs args)
        {
            if (OnStatusChanged is not null)
                OnStatusChanged(this, args);
        }
        public void ClearSubscriber() => OnStatusChanged = null;
    }
}
