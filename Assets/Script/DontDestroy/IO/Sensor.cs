using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.IO
{
#nullable enable
    public class Sensor : MonoBehaviour, IEventPublisher<EventHandler<InputEventArgs>>
    {
        public bool IsJudging { get; set; } = false;
        public SensorStatus Status = SensorStatus.Off;
        public SensorType Type;
        public SensorGroup Group
        {
            get
            {
                var i = (int)Type;
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

        bool _isDebug = false;

        MeshRenderer _meshRenderer;
        Material _material;
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
        void Start()
        {
            _isDebug = MajInstances.Setting.Debug.DisplaySensor;
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = new Material(Shader.Find("Sprites/Default"));
            var color = Color.black;
            _meshRenderer.material = _material;
            color.a = 0;
            _material.color = color; 

            if(Group == SensorGroup.C)
            {
                var c2MeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
                c2MeshRenderer.material = _material;
            }
        }
        private void Update()
        {
            if (_isDebug)
            {
                _material.color = Status == SensorStatus.On ? new Color(0, 0, 0, 0.3f) : new Color(0, 0, 0, 0f);
            }
        }
    }
}