using Cysharp.Text;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        static void UpdateSensorState()
        {
            var sensors = _sensors.Span;
            var now = MajTimeline.UnscaledTime;
            var sensorStates = _sensorStates.Span;
            Span<SensorStatus> newStates = stackalloc SensorStatus[34];

            while (_touchPanelInputBuffer.TryDequeue(out var report))
            {
                var index = report.Index;
                if (!index.InRange(0, 33))
                    continue;

                newStates[index] |= report.State;
            }
            for (var i = 0; i < 34; i++)
            {
                var state = TouchPanel.IsOn(i) || TouchPanel.IsHadOn(i);

                newStates[i] |= state ? SensorStatus.On : SensorStatus.Off;
                sensorStates[i] = newStates[i] is SensorStatus.On;
            }
            var C = newStates[16] | newStates[17];
            newStates[16] = C;
            newStates.Slice(18).CopyTo(newStates.Slice(17));
            newStates = newStates.Slice(0, 33);

            for (var i = 0; i < 33; i++)
            {
                var sensor = sensors[i];
                var sensorArea = sensor.Area;
                var sensorIndex = (int)sensorArea;

                if (sensor is null)
                {
                    MajDebug.LogError($"{i}# Sensor instance is null");
                    continue;
                }
                var oldState = sensor.State;
                var newState = newStates[i];

                if (oldState == newState)
                    continue;              
                if (_isSensorDebounceEnabled)
                {
                    if (JitterDetect(sensorArea, now))
                    {
                        continue;
                    }
                    _sensorLastTriggerTimes[sensorIndex] = now;
                }

                MajDebug.Log(ZString.Format("Sensor \"{0}\": {1}", sensor.Area, newState));
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
        public static void BindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensors = _sensors.Span;
            var sensor = sensors.Find(x => x?.Area == sType);
            if (sensor is null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.AddSubscriber(checker);
        }
        public static void UnbindSensor(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensors = _sensors.Span;
            var sensor = sensors.Find(x => x?.Area == sType);
            if (sensor is null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.RemoveSubscriber(checker);
        }
    }
}
