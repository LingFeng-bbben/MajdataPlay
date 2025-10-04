using MajdataPlay.Collections;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        //public static bool UseOuterTouchAsSensor;
        static void UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            var mainCamera = Majdata<IMainCameraProvider>.Instance!.MainCamera;
            Span<bool> newStates = stackalloc bool[34];
            Span<bool> extraButtonStates = stackalloc bool[12];

            var touches = Touch.activeTouches;

            if (touches.Count > 0)
            {
                FromTouchPanel(touches, newStates, extraButtonStates, mainCamera);
            }
#if UNITY_STANDALONE
            else if (Mouse.current != null)
            {
                FromMouse(Mouse.current, newStates, extraButtonStates, mainCamera);
            }
#endif
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
            //if (UseOuterTouchAsSensor) 
            //{
            //    foreach (var (i, state) in extraButtonStates.WithIndex())
            //    {
            //        var _state = state ? SwitchStatus.On : SwitchStatus.Off;
            //        if (i >= 8) continue;
            //        _touchPanelInputBuffer.Enqueue(new InputDeviceReport()
            //        {
            //            Index = i,
            //            State = _state,
            //            Timestamp = now
            //        });
            //    }
            //}
            foreach(var (i, state) in extraButtonStates.WithIndex())
            {
                var _state = state ? SwitchStatus.On : SwitchStatus.Off;
                //if (i < 8 && UseOuterTouchAsSensor) continue;
                _buttonRingInputBuffer.Enqueue(new InputDeviceReport()
                {
                    Index = i,
                    State = _state,
                    Timestamp = now
                });
            }
        }
        static void FromTouchPanel(in ReadOnlyArray<Touch> touches, Span<bool> newStates, Span<bool> extraButton, Camera mainCamera)
        {
            for (var j = 0; j < touches.Count; j++)
            {
                var touch = touches[j];
                if(!touch.valid)
                {
                    continue;
                }
                switch(touch.phase)
                {
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        continue;
                }
                var button = PositionToSensorState(newStates, mainCamera, touch.screenPosition);
                if (button != -1)
                {
                    extraButton[button] = true;
                }
            }
        }


        static void FromMouse(Mouse mouse, Span<bool> newStates, Span<bool> extraButton, Camera mainCamera)
        {
            var leftButton = mouse.leftButton;
            if(!leftButton.isPressed)
            {
                return;
            }
            var button = PositionToSensorState(newStates, mainCamera, mouse.position.value);
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
