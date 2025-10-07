using MajdataPlay.Collections;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        // Button bit (12bit)
        // 1 2 3 4 5 6 7 8 9 10 11 12
        // 0 0 0 0 0 0 0 0 0 0  0  0
        // Sensor bit (34bit)
        // A1 A2 A3 A4 A5 A6 A7 A8 B1 B2 B3 B4 B5 B6 B7 B8 C1 C2 D1 D2 D3 D4 D5 D6 D7 D8 E1 E2 E3 E4 E5 E6 E7 E8
        // 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0
        // Version bit (16bit)
        // uint16
#if UNITY_ANDROID
        readonly static ulong?[][] _cachedPositions = new ulong?[4096][];
#else
        readonly static ulong?[][] _cachedPositions = new ulong?[4096][];
#endif

        static ushort _version = 0;
        static int _lastScreenWidth = -1;
        static int _lastScreenHeight = -1;
        //readonly static Dictionary<SensorArea, HashSet<int>> _touchRecords = new(8);
        public static bool UseOuterTouchAsSensor { get; set; }
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
            var cubeRay = mainCamera.ScreenToWorldPoint(position);
            if (cachedPosition is not null)
            {
                var p = (ulong)cachedPosition;
                var version = p >> (12 + 34);
                if (version == _version)
                {
                    //MajDebug.LogDebug("Cached position");
                    var eB = -1;
                    for (var i = 0; i < 12; i++)
                    {
                        if ((p & (1UL << i)) != 0)
                        {
                            if (UseOuterTouchAsSensor)
                            {
                                if (i < 8)
                                {
                                    newStates[i] = true;
                                }
                                else
                                {
                                    eB = i;
                                    break;
                                }
                            }
                            else
                            {
                                return i;
                            }
                        }
                    }
                    for (var i = 0; i < 34; i++)
                    {
                        newStates[i] |= (p & (1UL << (i + 12))) != 0;
                    }
                    return eB;
                }
            }
            var newP = ((ulong)_version) << (12 + 34);
            var rayToCenter = cubeRay - new Vector3(0, 0, -10);
            var radToCenter = (rayToCenter).magnitude;
            var extraButton = -1;
            if(radToCenter > 9.28)
            {
                extraButton = 9;
            }
            else if(radToCenter > 5.4f)
            {
                // out of the screen area to the button area
                var degree = -Mathf.Atan2(rayToCenter.y, rayToCenter.x) * Mathf.Rad2Deg + 180;
                var pos = (int)(degree / 45f);
                switch (pos)
                {
                    case 0:
                        extraButton = 6;
                        break;
                    case 1:
                        extraButton = 7;
                        break;
                    default:
                        extraButton = (pos - 2);
                        break;
                }
            }
            var userRad = FingerRadius;
            //var lastCircular = cubeRay + new Vector3(0, userRad);
            const int SMAPLE_COUNT = 128;
            const int HORIZONTAL_SMAPLE_COUNT = 16;
            const float DEG_STEP = 360f / SMAPLE_COUNT;
            var radStep = userRad / HORIZONTAL_SMAPLE_COUNT;

            for (var rad = userRad; ; rad -= radStep) 
            {
                if(rad <= 0)
                {
                    RaycastNow(cubeRay, newStates, ref newP);
                    break;
                }
                for (int i = 0; i < SMAPLE_COUNT; i++)
                {
                    var circular = new Vector3(rad * Mathf.Sin(DEG_STEP * i), rad * Mathf.Cos(DEG_STEP * i));
                    var pos = cubeRay + circular;
                    //Debug.DrawLine(lastCircular, pos, Color.red, MajEnv.FRAME_LENGTH_SEC);
                    //lastCircular = pos;

                    RaycastNow(pos, newStates, ref newP);
                }
            }
            if(extraButton != -1)
            {
                newP |= 1UL << extraButton;
            }
            cachedPosition = newP;
            if (UseOuterTouchAsSensor)
            {
                if(extraButton < 8 && extraButton != -1)
                {
                    newStates[extraButton] = true;
                }    
                return -1;
            }
            else
            {
                if(extraButton != -1)
                {
                    newStates.Clear();
                    return extraButton;
                }
                else
                {
                    return -1;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RaycastNow(in Vector3 pos, in Span<bool> newStates, ref ulong newP)
        {
            var ray = new Ray(pos, Vector3.forward);
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
    }
}
