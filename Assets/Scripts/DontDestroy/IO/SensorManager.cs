using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        readonly Sensor[] _sensors = new Sensor[33];
        readonly Dictionary<SensorArea, DateTime> _sensorLastTriggerTimes = new();
        void UpdateSensorState()
        {
            while (_touchPanelInputBuffer.TryDequeue(out var report))
            {
                if (!report.Index.InRange(0, 33))
                    continue;
                var index = report.Index;
                var sensor = index switch
                {
                    <= (int)SensorArea.C => _sensors[index],
                    > 17 => _sensors[index - 1],
                    _ => _sensors[16],
                };
                var timestamp = report.Timestamp;
                if (sensor is null)
                {
                    MajDebug.LogError($"{index}# Sensor instance is null");
                    continue;
                }
                var oldState = sensor.Status;
                var newState = report.State;
                var C1 = _sensorStatuses[16];
                var C2 =  _sensorStatuses[17];

                if (sensor.Area == SensorArea.C)
                {
                    var CSubAreaState = report.State is SensorStatus.On ? true: false;
                    switch (index)
                    {
                        case 16:
                            newState = CSubAreaState || C2 ? SensorStatus.On : SensorStatus.Off;
                            break;
                        case 17:
                            newState = CSubAreaState || C1 ? SensorStatus.On : SensorStatus.Off;
                            break;
                    }
                }
                if (_isSensorDebounceEnabled)
                {
                    if (JitterDetect(sensor.Area, timestamp))
                    {
                        continue;
                    }
                    else if(oldState == newState)
                    {
                        switch (index)
                        {
                            case 16:
                            case 17:
                                _sensorStatuses[index] = report.State is SensorStatus.On ? true : false;
                                break;
                        }
                        continue;
                    }
                    _sensorLastTriggerTimes[sensor.Area] = timestamp;
                }
                else if(oldState == newState)
                {
                    switch (index)
                    {
                        case 16:
                        case 17:
                            _sensorStatuses[index] = report.State is SensorStatus.On ? true : false;
                            break;
                    }
                    continue;
                }
                MajDebug.Log($"Sensor \"{sensor.Area}\": {newState}");
                _sensorStatuses[index] = report.State is SensorStatus.On ? true : false;
                sensor.Status = newState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Area,
                    OldStatus = oldState,
                    Status = newState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
                SetIdle(msg);
            }
        }
        void SetSensorState(SensorArea type,SensorStatus nState)
        {
            var sensor = _sensors[(int)type];
            if (sensor == null)
                throw new Exception($"{type} Sensor not found.");
            var oState = sensor.Status;
            sensor.Status = nState;

            if (oState != nState)
            {
                MajDebug.Log($"Sensor \"{sensor.Area}\": {nState}");
                sensor.Status = nState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Area,
                    OldStatus = oState,
                    Status = nState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
                SetIdle(msg);
            }
        }
        public void BindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = _sensors.Find(x => x?.Area == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.AddSubscriber(checker);
        }
        public void UnbindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = _sensors.Find(x => x?.Area == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.RemoveSubscriber(checker);
        }
    }
}
