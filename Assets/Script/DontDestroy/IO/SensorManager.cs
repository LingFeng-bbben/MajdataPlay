using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        Sensor[] sensors = new Sensor[33];

        void UpdateSensorState()
        {
            foreach(var (index, on) in COMReport.WithIndex())
            {
                var sensor = sensors[index];
                if(sensor == null)
                {
                    Debug.LogError($"{index}# Sensor instance is null");
                    continue;
                }
                var oState = sensor.Status;
                var nState = on ? SensorStatus.On : SensorStatus.Off;
                if(oState != nState)
                {
                    sensor.Status = nState;
                    var msg = new InputEventArgs()
                    {
                        Type = sensor.Type,
                        OldStatus = oState,
                        Status = nState,
                        IsButton = false
                    };
                    sensor.PushEvent(msg);
                    PushEvent(msg);
                }
            }
        }
        void SetSensorState(SensorType type,SensorStatus nState)
        {
            var sensor = sensors[(int)type];
            if (sensor == null)
                throw new Exception($"{type} Sensor or Button not found.");
            var oState = sensor.Status;
            sensor.Status = nState;

            if (oState != nState)
            {
                sensor.Status = nState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Type,
                    OldStatus = oState,
                    Status = nState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
            }
        }
        public void BindSensor(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x?.Type == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.OnStatusChanged += checker;
        }
        public void UnbindSensor(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x?.Type == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.OnStatusChanged -= checker;
        }
    }
}
