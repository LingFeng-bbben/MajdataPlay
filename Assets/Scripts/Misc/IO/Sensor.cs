using MajdataPlay.Types;
using System;

namespace MajdataPlay.IO
{
#nullable enable
    public class Sensor : IEventPublisher<EventHandler<InputEventArgs>>
    {
        public SensorStatus State { get; set; } = SensorStatus.Off;
        public SensorArea Area { get; set; }
        public SensorGroup Group
        {
            get
            {
                var i = (int)Area;
                if (i <= 7)
                    return SensorGroup.A;
                else if (i <= 15)
                    return SensorGroup.B;
                else if (i <= 16)
                    return SensorGroup.C;
                else if (i <= 24)
                    return SensorGroup.D;
                else
                    return SensorGroup.E;
            }
        }
        event EventHandler<InputEventArgs>? OnStatusChanged;//oStatus nStatus

        public void AddSubscriber(EventHandler<InputEventArgs> handler)
        {
            OnStatusChanged += handler;
        }
        public void RemoveSubscriber(EventHandler<InputEventArgs> handler)
        {
            if(OnStatusChanged is not null)
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