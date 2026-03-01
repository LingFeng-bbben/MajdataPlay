using MajdataPlay.Collections;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        const int TOUCH_ANGLE_SMAPLE_COUNT = 128;
        const float FINGER_RADIUS_SEGMENT_LENGTH = 0.5f / 4;
        
        // Button bit (12bit)
        // 1 2 3 4 5 6 7 8 9 10 11 12
        // 0 0 0 0 0 0 0 0 0 0  0  0
        // Sensor bit (34bit)
        // A1 A2 A3 A4 A5 A6 A7 A8 B1 B2 B3 B4 B5 B6 B7 B8 C1 C2 D1 D2 D3 D4 D5 D6 D7 D8 E1 E2 E3 E4 E5 E6 E7 E8
        // 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0
        // Version bit (16bit)
        // uint16
        readonly static ulong* _posData = null;
        readonly static Dictionary<int, ulong> _touchRecorder = new(32);
        readonly static ReadOnlyMemory<Vector4> _unitCircle = ReadOnlyMemory<Vector4>.Empty;

        static ushort _version = 0;
        static int _lastScreenWidth = -1;
        static int _lastScreenHeight = -1;
        static float _lastFingerRadius = 0.5f;
        static float _lastTouchRadiusAdjust = 1f;
        static float _lastAAreaExtraRadius = 1f;
        static float _lastBAreaExtraRadius = 1f;
        static float _lastCAreaExtraRadius = 1f;
        static float _lastDAreaExtraRadius = 1f;
        static float _lastEAreaExtraRadius = 1f;
        static float _maxTouchRadius = -1f;
        //readonly static Dictionary<SensorArea, HashSet<int>> _touchRecords = new(8);
        public static bool UseOuterTouchAsSensor { get; set; }
        static void UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            var mainCamera = Majdata<IMainCameraProvider>.Instance!.MainCamera;

            Span<int> sensorClickedCount = stackalloc int[34];
            Span<bool> newStates = stackalloc bool[34];
            Span<bool> extraButtonStates = stackalloc bool[12];

            var touches = Touch.activeTouches;

            if (touches.Count > 0)
            {
                FromTouchPanel(touches, sensorClickedCount, newStates, extraButtonStates, mainCamera);
            }
#if UNITY_STANDALONE || UNITY_EDITOR
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
#if UNITY_ANDROID || UNITY_IOS
            for (var i = 0; i < sensorClickedCount.Length; i++) 
            {
                var clickedCount = sensorClickedCount[i];
                if (i == 16)
                {
                    clickedCount = Mathf.Max(clickedCount, sensorClickedCount[17]);
                    i++;
                }
                if(i >= 16)
                {
                    _sensorClickedCountInThisFrame[i - 1] = clickedCount;
                }
                else
                {
                    _sensorClickedCountInThisFrame[i] = clickedCount;
                }
            }
#endif
            foreach (var (i, state) in extraButtonStates.WithIndex())
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
        static void FromTouchPanel(in ReadOnlyArray<Touch> touches,
                                   Span<int> sensorClickedCount,
                                   Span<bool> sensorStates, 
                                   Span<bool> extraButton, Camera mainCamera)
        {
#if UNITY_IOS
            const float PLATFORM_TOUCH_RADIUS_ADJUST = 78f * 4f;
#else
            const float PLATFORM_TOUCH_RADIUS_ADJUST = 1f;
#endif
            for (var j = 0; j < touches.Count; j++)
            {
                var touch = touches[j];
                if(!touch.valid)
                {
                    continue;
                }
                var touchPosData = 0UL;
                var touchRadius = touch.radius.magnitude;
                var button = PositionToSensorState(sensorStates, 
                    mainCamera, 
                    touch.screenPosition, 
                    touchRadius / PLATFORM_TOUCH_RADIUS_ADJUST, 
                    ref touchPosData);
                if (touchRadius > _maxTouchRadius)
                {
                    MajDebug.LogInfo($"Touch radius: {touchRadius}");
                    _maxTouchRadius = touchRadius;
                }
                if (button != -1)
                {
                    extraButton[button] = true;
                }
#if UNITY_ANDROID || UNITY_IOS
                _touchRecorder.TryGetValue(touch.touchId, out var lastTouchPosData);

                for (var i = 0; i < 34; i++)
                {
                    var lastState = false;
                    var currentState = false;

                    if (UseOuterTouchAsSensor && i < 8)
                    {
                        lastState = ((lastTouchPosData & (1UL << (i + 12))) | (lastTouchPosData & (1UL << i))) != 0;
                        currentState = ((touchPosData & (1UL << (i + 12))) | (touchPosData & (1UL << i))) != 0;
                    }
                    else
                    {
                        lastState = (lastTouchPosData & (1UL << (i + 12))) != 0;
                        currentState = (touchPosData & (1UL << (i + 12))) != 0;
                    }

                    if (!lastState && currentState)
                    {
                        sensorClickedCount[i]++;
                    }
                }

                if (touch.ended)
                {
                    _touchRecorder.Remove(touch.touchId);
                }
                else
                {
                    _touchRecorder[touch.touchId] = touchPosData;
                }
#endif

            }
        }


        static void FromMouse(Mouse mouse, Span<bool> sensorStates, Span<bool> extraButton, Camera mainCamera)
        {
            var leftButton = mouse.leftButton;
            if(!leftButton.isPressed)
            {
                return;
            }
            var button = PositionToSensorState(sensorStates, mainCamera, mouse.position.value);
            if (button != -1)
            {
                extraButton[button] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PositionToSensorState(Span<bool> newStates, Camera mainCamera, Vector3 position)
        {
            var _ = 0UL;
            return PositionToSensorState(newStates, mainCamera, position, 0, ref _);
        }
        /// <summary>
        /// return extra button pos 0-7, if none return -1
        /// </summary>
        /// <param name="newStates"></param>
        /// <param name="mainCamera"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PositionToSensorState(Span<bool> newStates, Camera mainCamera, Vector3 position, float touchRadius, ref ulong rawPositionData)
        {
            var x = (int)position.x;
            var y = (int)position.y;
            if(x < 0 || y < 0)
            {
                return -1;
            }
            var cubeRay = mainCamera.ScreenToWorldPoint(position);
            var newP = ((ulong)_version) << (12 + 34);
            var rayToCenter = cubeRay - new Vector3(0, 0, -10);
            var radToCenter = rayToCenter.magnitude;
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
            var circleSamples = _unitCircle.Span;
            var userRad = _lastFingerRadius * (1 + touchRadius * _lastTouchRadiusAdjust);
            var a_extraRad = _lastAAreaExtraRadius;
            var b_extraRad = _lastBAreaExtraRadius;
            var c_extraRad = _lastCAreaExtraRadius;
            var d_extraRad = _lastDAreaExtraRadius;
            var e_extraRad = _lastEAreaExtraRadius;
            //var lastCircular = cubeRay + new Vector3(0, userRad);
            fixed(Vector4* circleSamplesPtr = &circleSamples.GetPinnableReference())
            {
                MobileTouchPanelHelper.PositionHandle(cubeRay,
                    userRad,
                    a_extraRad,
                    b_extraRad,
                    c_extraRad,
                    d_extraRad,
                    e_extraRad,
                    FINGER_RADIUS_SEGMENT_LENGTH,
                    TOUCH_ANGLE_SMAPLE_COUNT,
                    _posData,
                    circleSamplesPtr,
                    ref newP);
            }
            

            for (var i = 0; i < 34; i++)
            {
                newStates[i] |= (newP & (1UL << (i + 12))) != 0;
            }
            if (extraButton != -1)
            {
                newP |= 1UL << extraButton;
            }
            if (UseOuterTouchAsSensor)
            {
                if (extraButton < 8 && extraButton != -1)
                {
                    newStates[extraButton] = true;
                    return -1;
                }
                else
                {
                    return extraButton;
                }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RaycastNow(in Vector3 pos, ref ulong newP)
        {
            var ray = new Ray(pos, Vector3.forward);
            var ishit = Physics.Raycast(ray, out var hitInfom);
            if (ishit)
            {
                var id = hitInfom.colliderInstanceID;
                if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
                {
                    newP |= 1UL << (index + 12);
                }
            }
        }
    }
}
