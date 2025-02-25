using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.IO
{
    internal partial class InputManager : MonoBehaviour
    {
        readonly static Dictionary<int, int> _instanceID2SensorIndexMappingTable = new();
        static void UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            Input.multiTouchEnabled = true;
            Span<bool> newStates = stackalloc bool[34];
            if (Input.touchCount > 0)
            {
                MajDebug.Log(Input.touchCount);
                
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        continue;
                    }
                    Vector3 cubeRay = Camera.main.ScreenToWorldPoint(touch.position);
                    var ray = new Ray(cubeRay, Vector3.forward);
                    var ishit = Physics.Raycast(ray, out var hitInfo);
                    if (ishit)
                    {
                        var id = hitInfo.colliderInstanceID;
                        if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
                        {
                            newStates[index] = true;
                        }
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 cubeRaym = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var raym = new Ray(cubeRaym, Vector3.forward);
                var ishitm = Physics.Raycast(raym, out var hitInfom);
                if (ishitm)
                {
                    var id = hitInfom.colliderInstanceID;
                    if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
                    {
                        newStates[index] = true;
                    }
                }
            }
            var now = DateTime.Now;
            foreach (var (i, state) in newStates.WithIndex())
            {
                _touchPanelInputBuffer.Enqueue(new InputDeviceReport()
                {
                    Index = i,
                    State = state ? SensorStatus.On : SensorStatus.Off,
                    Timestamp = now
                });
            }
            UpdateSensorState();
        }
        bool isInRange(in float input,in float angle,in float range = 11.25f) => Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
    }
}
