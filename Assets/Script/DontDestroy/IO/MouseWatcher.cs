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
                for (int i = 0; i < sensorStates.Length; i++)
                {
                    sensorStates[i] = false;
                }
                if (distance > 0.75)
                {
                    if (isInRange(angle, 0))
                    {
                        sensorStates[TouchNameToIndex["D3"]] = true;
                    }
                    else if (isInRange(angle, 45f))
                    {
                        sensorStates[TouchNameToIndex["D2"]] = true;
                    }
                    else if (isInRange(angle, 90f))
                    {
                        sensorStates[TouchNameToIndex["D1"]] = true;
                    }
                    else if (isInRange(angle, 135f))
                    {
                        sensorStates[TouchNameToIndex["D8"]] = true;
                    }
                    else if (isInRange(angle, 180f))
                    {
                        sensorStates[TouchNameToIndex["D7"]] = true;
                    }
                    else if (isInRange(angle, -135f))
                    {
                        sensorStates[TouchNameToIndex["D6"]] = true;
                    }
                    else if (isInRange(angle, -90f))
                    {
                        sensorStates[TouchNameToIndex["D5"]] = true;
                    }
                    else if (isInRange(angle, -45f))
                    {
                        sensorStates[TouchNameToIndex["D4"]] = true;
                    }


                }
                if (distance > 0.6)
                {
                    if (isInRange(angle, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["A2"]] = true;
                    }
                    else if (isInRange(angle, 67.5f))
                    {
                        sensorStates[TouchNameToIndex["A1"]] = true;
                    }
                    else if (isInRange(angle, 112.5f))
                    {
                        sensorStates[TouchNameToIndex["A8"]] = true;
                    }
                    else if (isInRange(angle, 157.5f))
                    {
                        sensorStates[TouchNameToIndex["A7"]] = true;
                    }
                    else if (isInRange(angle, -157.5f))
                    {
                        sensorStates[TouchNameToIndex["A6"]] = true;
                    }
                    else if (isInRange(angle, -112.5f))
                    {
                        sensorStates[TouchNameToIndex["A5"]] = true;
                    }
                    else if (isInRange(angle, -67.5f))
                    {
                        sensorStates[TouchNameToIndex["A4"]] = true;
                    }
                    else if (isInRange(angle, -22.5f))
                    {
                        sensorStates[TouchNameToIndex["A3"]] = true;
                    }
                }
                if (distance > 0.42 && distance <= 0.71)
                {
                    if (isInRange(angle, 0))
                    {
                        sensorStates[TouchNameToIndex["E3"]] = true;
                    }
                    else if (isInRange(angle, 45f))
                    {
                        sensorStates[TouchNameToIndex["E2"]] = true;
                    }
                    else if (isInRange(angle, 90f))
                    {
                        sensorStates[TouchNameToIndex["E1"]] = true;
                    }
                    else if (isInRange(angle, 135f))
                    {
                        sensorStates[TouchNameToIndex["E8"]] = true;
                    }
                    else if (isInRange(angle, 180f))
                    {
                        sensorStates[TouchNameToIndex["E7"]] = true;
                    }
                    else if (isInRange(angle, -135f))
                    {
                        sensorStates[TouchNameToIndex["E6"]] = true;
                    }
                    else if (isInRange(angle, -90f))
                    {
                        sensorStates[TouchNameToIndex["E5"]] = true;
                    }
                    else if (isInRange(angle, -45f))
                    {
                        sensorStates[TouchNameToIndex["E4"]] = true;
                    }
                }
                if (distance > 0.267 && distance <= 0.53)
                {
                    if (isInRange(angle, 22.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B2"]] = true;
                    }
                    else if (isInRange(angle, 67.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B1"]] = true;
                    }
                    else if (isInRange(angle, 112.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B8"]] = true;
                    }
                    else if (isInRange(angle, 157.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B7"]] = true;
                    }
                    else if (isInRange(angle, -157.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B6"]] = true;
                    }
                    else if (isInRange(angle, -112.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B5"]] = true;
                    }
                    else if (isInRange(angle, -67.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B4"]] = true;
                    }
                    else if (isInRange(angle, -22.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B3"]] = true;
                    }
                }
                if (distance <= 0.267)
                {
                    if (isInRange(angle, 0, 90))
                    {
                        sensorStates[TouchNameToIndex["C2"]] = true;
                    }
                    else if (isInRange(angle, 180, 90))
                    {
                        sensorStates[TouchNameToIndex["C1"]] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < sensorStates.Length; i++)
                {
                    sensorStates[i] = false;
                }
            }
        }
        void UpdateSensorState(SensorGroup group,in float angle)
        {

        }
        bool isInRange(in float input,in float angle,in float range = 11.25f) => Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
    }
}
