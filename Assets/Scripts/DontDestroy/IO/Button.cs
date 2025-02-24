#nullable enable
using MajdataPlay.Types;
using System;
using UnityRawInput;

namespace MajdataPlay.IO
{
    public class Button: IEventPublisher<EventHandler<InputEventArgs>>
    {
        public RawKey BindingKey { get; set; }
        public SensorArea Area { get; set; }
        public SensorStatus State { get; set; }
        event EventHandler<InputEventArgs>? OnStatusChanged;
        public Button(RawKey bindingKey, SensorArea type)
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
