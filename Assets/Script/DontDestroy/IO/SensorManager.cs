using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        Sensor[] sensors = new Sensor[33];

        void UpdateSensorState()
        {
            foreach(var (index, on) in COMReport.WithIndex())
            {
                if (index > sensors.Length)
                    break;
                var sensor = index switch
                {
                    <= (int)SensorType.C => sensors[index],
                     > 17 => sensors[index - 1],
                     _    => sensors[16],
                };
                if (sensor == null)
                {
                    Debug.LogError($"{index}# Sensor instance is null");
                    continue;
                }
                var oState = sensor.Status;
                var nState = on ? SensorStatus.On : SensorStatus.Off;
                if (sensor.Type == SensorType.C)
                    nState = COMReport[16] || COMReport[17] ? SensorStatus.On : SensorStatus.Off;
                if(oState != nState)
                {
                    Debug.Log($"Sensor \"{sensor.Type}\": {nState}");
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
                throw new Exception($"{type} Sensor not found.");
            var oState = sensor.Status;
            sensor.Status = nState;

            if (oState != nState)
            {
                Debug.Log($"Sensor \"{sensor.Type}\": {nState}");
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
