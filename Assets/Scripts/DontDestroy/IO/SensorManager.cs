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
    internal partial class InputManager : MonoBehaviour
    {
        
        static void UpdateSensorState()
        {
            var sensors = _sensors.Span;
            while (_touchPanelInputBuffer.TryDequeue(out var report))
            {
                if (!report.Index.InRange(0, 33))
                    continue;
                var index = report.Index;
                var sensor = index switch
                {
                    <= (int)SensorArea.C => sensors[index],
                    > 17 => sensors[index - 1],
                    _ => sensors[16],
                };
                var timestamp = report.Timestamp;
                if (sensor is null)
                {
                    MajDebug.LogError($"{index}# Sensor instance is null");
                    continue;
                }
                var sensorStates = _sensorStates.Span;
                var oldState = sensor.State;
                var newState = report.State;
                var C1 = sensorStates[16];
                var C2 = sensorStates[17];

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
                                sensorStates[index] = report.State is SensorStatus.On ? true : false;
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
                            sensorStates[index] = report.State is SensorStatus.On ? true : false;
                            break;
                    }
                    continue;
                }
                MajDebug.Log($"Sensor \"{sensor.Area}\": {newState}");
                sensorStates[index] = report.State is SensorStatus.On ? true : false;
                sensor.State = newState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Area,
                    OldStatus = oldState,
                    Status = newState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
            }
        }
        static void SetSensorState(SensorArea type,SensorStatus nState)
        {
            var sensors = _sensors.Span;
            var sensor = sensors[(int)type];
            if (sensor is null)
                throw new Exception($"{type} Sensor not found.");
            var oState = sensor.State;
            sensor.State = nState;

            if (oState != nState)
            {
                MajDebug.Log($"Sensor \"{sensor.Area}\": {nState}");
                sensor.State = nState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Area,
                    OldStatus = oState,
                    Status = nState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
            }
        }
        public void BindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensors = _sensors.Span;
            var sensor = sensors.Find(x => x?.Area == sType);
            if (sensor is null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.AddSubscriber(checker);
        }
        public void UnbindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensors = _sensors.Span;
            var sensor = sensors.Find(x => x?.Area == sType);
            if (sensor is null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.RemoveSubscriber(checker);
        }
    }
}
