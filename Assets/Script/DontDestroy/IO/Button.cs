#nullable enable
using MajdataPlay.Types;
using System;
using UnityRawInput;

namespace MajdataPlay.IO
{
    public class Button
    {
        public RawKey BindingKey { get; set; }
        public SensorType Type { get; set; }
        public bool IsJudging { get; set; } = false;
        public SensorStatus Status { get; set; }
        public event EventHandler<InputEventArgs>? OnStatusChanged;
        public Button(RawKey bindingKey, SensorType type)
        {
            BindingKey = bindingKey;
            Type = type;
            IsJudging = false;
            Status = SensorStatus.Off;
            OnStatusChanged = null;
        }
        public void PushEvent(in InputEventArgs args)
        {
            if (OnStatusChanged is not null)
                OnStatusChanged(this, args);
        }
        public void ClearSubscriber() => OnStatusChanged = null;
    }
}
