using MajdataPlay.Collections;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        static void UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            var mainCamera = Majdata<IMainCameraProvider>.Instance!.MainCamera;
            Span<bool> newStates = stackalloc bool[34];
            Span<bool> extraButtonStates = stackalloc bool[12];

            if (Input.touchCount > 0)
            {
                FromTouchPanel(newStates, extraButtonStates, mainCamera);
            }
            else if (Input.GetMouseButton(0))
            {
                FromMouse(newStates, extraButtonStates, mainCamera);
            }
            var now = MajTimeline.UnscaledTime;
            foreach (var (i, state) in newStates.WithIndex())
            {
                var _state = state ? SwitchStatus.On : SwitchStatus.Off;
                _touchPanelInputBuffer.Enqueue(new InputDeviceReport()
                {
                    Index = i,
                    State = _state,
                    Timestamp = now
                });
            }
            foreach(var (i, state) in extraButtonStates.WithIndex())
            {
                var _state = state ? SwitchStatus.On : SwitchStatus.Off;
                _buttonRingInputBuffer.Enqueue(new InputDeviceReport()
                {
                    Index = i,
                    State = _state,
                    Timestamp = now
                });
            }
        }
        static void FromTouchPanel(Span<bool> newStates, Span<bool> extraButton, Camera mainCamera)
        {
            for (var j = 0; j < Input.touchCount; j++)
            {
                var touch = Input.GetTouch(j);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    continue;
                }
                var button = PositionToSensorState(newStates, mainCamera, touch.position);
                if (button != -1)
                {
                    extraButton[button] = true;
                }
            }
        }


        static void FromMouse(Span<bool> newStates, Span<bool> extraButton, Camera mainCamera)
        {
            var button = PositionToSensorState(newStates, mainCamera, Input.mousePosition);
            if (button != -1)
            {
                extraButton[button] = true;
            }
        }

        //return extra button pos 0-7, if none return -1
        private static int PositionToSensorState(Span<bool> newStates, Camera mainCamera, Vector3 position)
        {
            Vector3 cubeRay = mainCamera.ScreenToWorldPoint(position);
            var rayToCenter = cubeRay - new Vector3(0, 0, -10);
            var radToCenter = (rayToCenter).magnitude;
            if(radToCenter > 9.28)
            {
                return 9;
            }
            if(radToCenter > 5.4f)
            {
                // out of the screen area to the button area
                var degree = -Mathf.Atan2(rayToCenter.y, rayToCenter.x) * Mathf.Rad2Deg + 180;
                var pos = (int)(degree/45f);
                switch (pos)
                {
                    case 0:
                        return 6;
                    case 1:
                        return 7;
                    default:
                        return pos - 2;
                }
            }
            for (int i = 0; i < 9; i++)
            {
                var rad = FingerRadius;
                var circular = new Vector3(rad * Mathf.Sin(45f * i), rad * Mathf.Cos(45f * i));
                if (i == 8) circular = Vector3.zero;
                var ray = new Ray(cubeRay + circular, Vector3.forward);
                var ishit = Physics.Raycast(ray, out var hitInfom);
                if (ishit)
                {
                    var id = hitInfom.colliderInstanceID;
                    if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
                    {
                        newStates[index] = true;
                    }
                }
            }
            return -1;
        }
    }
}
