using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        void UpdateMousePosition()
        {
            if (Input.GetMouseButton(0))
            {
                var x = Input.mousePosition.x / Screen.width * 2 - 1;
                var y = Input.mousePosition.y / Screen.width * 2 - 1;
                var distance = Math.Sqrt(x * x + y * y);
                var angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                Dictionary<SensorType, SensorStatus> oldSensorsState = sensors.ToDictionary(s => s.Type,s => s.Status);
                Dictionary<SensorType, SensorStatus> newSensorsState = sensors.ToDictionary(s => s.Type,x => SensorStatus.Off);

                if (distance > 0.75)
                {
                    if (isInRange(angle, 0))
                        newSensorsState[SensorType.D3] = SensorStatus.On;
                    else if (isInRange(angle, 45f))
                        newSensorsState[SensorType.D2] = SensorStatus.On;
                    else if (isInRange(angle, 90f))
                        newSensorsState[SensorType.D1] = SensorStatus.On;
                    else if (isInRange(angle, 135f))
                        newSensorsState[SensorType.D8] = SensorStatus.On;
                    else if (isInRange(angle, 180f))
                        newSensorsState[SensorType.D7] = SensorStatus.On;
                    else if (isInRange(angle, -135f))
                        newSensorsState[SensorType.D6] = SensorStatus.On;
                    else if (isInRange(angle, -90f))
                        newSensorsState[SensorType.D5] = SensorStatus.On;
                    else if (isInRange(angle, -45f))
                        newSensorsState[SensorType.D4] = SensorStatus.On;
                }
                if (distance > 0.6)
                {
                    if (isInRange(angle, 22.5f))
                        newSensorsState[SensorType.A2] = SensorStatus.On;
                    else if (isInRange(angle, 67.5f))
                        newSensorsState[SensorType.A1] = SensorStatus.On;
                    else if (isInRange(angle, 112.5f))
                        newSensorsState[SensorType.A8] = SensorStatus.On;
                    else if (isInRange(angle, 157.5f))
                        newSensorsState[SensorType.A7] = SensorStatus.On;
                    else if (isInRange(angle, -157.5f))
                        newSensorsState[SensorType.A6] = SensorStatus.On;
                    else if (isInRange(angle, -112.5f))
                        newSensorsState[SensorType.A5] = SensorStatus.On;
                    else if (isInRange(angle, -67.5f))
                        newSensorsState[SensorType.A4] = SensorStatus.On;
                    else if (isInRange(angle, -22.5f))
                        newSensorsState[SensorType.A3] = SensorStatus.On;
                }
                if (distance > 0.42 && distance <= 0.71)
                {
                    if (isInRange(angle, 0))
                        newSensorsState[SensorType.E3] = SensorStatus.On;
                    else if (isInRange(angle, 45f))
                        newSensorsState[SensorType.E2] = SensorStatus.On;
                    else if (isInRange(angle, 90f))
                        newSensorsState[SensorType.E1] = SensorStatus.On;
                    else if (isInRange(angle, 135f))
                        newSensorsState[SensorType.E8] = SensorStatus.On;
                    else if (isInRange(angle, 180f))
                        newSensorsState[SensorType.E7] = SensorStatus.On;
                    else if (isInRange(angle, -135f))
                        newSensorsState[SensorType.E6] = SensorStatus.On;
                    else if (isInRange(angle, -90f))
                        newSensorsState[SensorType.E5] = SensorStatus.On;
                    else if (isInRange(angle, -45f))
                        newSensorsState[SensorType.E4] = SensorStatus.On;
                }
                if (distance > 0.267 && distance <= 0.53)
                {
                    if (isInRange(angle, 22.5f, 22.5f))
                        newSensorsState[SensorType.B2] = SensorStatus.On;
                    else if (isInRange(angle, 67.5f, 22.5f))
                        newSensorsState[SensorType.B1] = SensorStatus.On;
                    else if (isInRange(angle, 112.5f, 22.5f))
                        newSensorsState[SensorType.B8] = SensorStatus.On;
                    else if (isInRange(angle, 157.5f, 22.5f))
                        newSensorsState[SensorType.B7] = SensorStatus.On;
                    else if (isInRange(angle, -157.5f, 22.5f))
                        newSensorsState[SensorType.B6] = SensorStatus.On;
                    else if (isInRange(angle, -112.5f, 22.5f))
                        newSensorsState[SensorType.B5] = SensorStatus.On;
                    else if (isInRange(angle, -67.5f, 22.5f))
                        newSensorsState[SensorType.B4] = SensorStatus.On;
                    else if (isInRange(angle, -22.5f, 22.5f))
                        newSensorsState[SensorType.B3] = SensorStatus.On;
                }
                if (distance <= 0.267)
                {
                    if (isInRange(angle, 0, 90) || isInRange(angle, 180, 90))
                        newSensorsState[SensorType.C] = SensorStatus.On;
                }

                foreach (var pair in newSensorsState) 
                {
                    var type = pair.Key;
                    var nState = newSensorsState[type];
                    var oState = oldSensorsState[type];
                    if (oState != nState)
                        SetSensorState(type, nState);
                }
            }
            else
            {
                foreach (var s in sensors)
                    SetSensorState(s.Type, SensorStatus.Off);
            }
        }
        bool isInRange(in float input,in float angle,in float range = 11.25f) => Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
    }
}
