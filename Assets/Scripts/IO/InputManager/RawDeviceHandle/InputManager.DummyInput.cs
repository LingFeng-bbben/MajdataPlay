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
using static UnityEditor.PlayerSettings;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        // Button bit (12bit)
        // 1 2 3 4 5 6 7 8 9 10 11 12
        // 0 0 0 0 0 0 0 0 0 0  0  0
        // Sensor bit (34bit)
        // A1 A2 A3 A4 A5 A6 A7 A8 B1 B2 B3 B4 B5 B6 B7 B8 C1 C2 D1 D2 D3 D4 D5 D6 D7 D8 E1 E2 E3 E4 E5 E6 E7 E8
        // 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0
        // Version bit (16bit)
        // uint16
        readonly static ulong?[][] _cachedPositions = new ulong?[16384][];
        static ushort _version = 0;
        static int _lastScreenWidth = -1;
        static int _lastScreenHeight = -1;
        //readonly static Dictionary<SensorArea, HashSet<int>> _touchRecords = new(8);
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
            var x = (int)position.x;
            var y = (int)position.y;
            if(x < 0 || y < 0)
            {
                return -1;
            }
            ref var cachedPosition = ref _cachedPositions[x][y];

            if(cachedPosition is not null)
            {
                var p = (ulong)cachedPosition;
                var version = p >> (12 + 34);
                if(version == _version)
                {
                    //MajDebug.LogDebug("Cached position");
                    for (var i = 0; i < 12; i++)
                    {
                        if ((p & (1UL << i)) != 0)
                        {
                            return i;
                        }
                    }
                    for (var i = 0; i < 34; i++)
                    {
                        newStates[i] = (p & (1UL << (i + 12))) != 0;
                    }
                    return -1;
                }
            }
            var newP = ((ulong)_version) << (12 + 34);
            Vector3 cubeRay = mainCamera.ScreenToWorldPoint(position);
            var rayToCenter = cubeRay - new Vector3(0, 0, -10);
            var radToCenter = (rayToCenter).magnitude;
            if(radToCenter > 9.28)
            {
                newP |= 1UL << 9;
                cachedPosition = newP;
                return 9;
            }
            if(radToCenter > 5.4f)
            {
                // out of the screen area to the button area
                var degree = -Mathf.Atan2(rayToCenter.y, rayToCenter.x) * Mathf.Rad2Deg + 180;
                var pos = (int)(degree / 45f);
                switch (pos)
                {
                    case 0:
                        newP |= 1UL << 6;
                        cachedPosition = newP;
                        return 6;
                    case 1:
                        newP |= 1UL << 7;
                        cachedPosition = newP;
                        return 7;
                    default:
                        newP |= 1UL << (pos - 2);
                        cachedPosition = newP;
                        return pos - 2;
                }
            }
            for (int i = 0; i < 9; i++)
            {
                var rad = FingerRadius;
                var circular = new Vector3(rad * Mathf.Sin(45f * i), rad * Mathf.Cos(45f * i));
                if (i == 8)
                {
                    circular = Vector3.zero;
                }
                var ray = new Ray(cubeRay + circular, Vector3.forward);
                var ishit = Physics.Raycast(ray, out var hitInfom);
                if (ishit)
                {
                    var id = hitInfom.colliderInstanceID;
                    if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
                    {
                        newP |= 1UL << (index + 12);
                        newStates[index] = true;
                    }
                }
            }
            cachedPosition = newP;
            return -1;
        }
    }
}
