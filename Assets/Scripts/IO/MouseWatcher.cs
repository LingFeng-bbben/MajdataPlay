using MajdataPlay.Extensions;
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
        private static float _radius => MajEnv.UserSetting.Misc.InputDevice.TouchPanel.TouchSimulationRadius;
        static bool[] UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            var mainCamera = GameManager.MainCamera;
            Span<bool> newStates = stackalloc bool[34];
            //button ring + extras
            Span<bool> extraButtonStates = stackalloc bool[12];
            if (Input.touchCount > 0)
            {
                FromTouchPanel(newStates,extraButtonStates, mainCamera);
            }
            else if (Input.GetMouseButton(0))
            {
                var button = FromMouse(newStates, mainCamera);
                if (button != -1)
                {
                    extraButtonStates[button] = true;
                }
            }
            var now = MajTimeline.UnscaledTime;
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
            return extraButtonStates.ToArray();
        }
        static void FromTouchPanel(Span<bool> newStates, Span<bool> extraButton, Camera mainCamera)
        {
            try
            {
                for (var j = 0; j< Input.touchCount; j++)
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
            catch(NullReferenceException)
            {
                GameManager.OnSceneChanged();
            }
            catch(UnityException)
            {
                GameManager.OnSceneChanged();
            }
        }


        //return extra button pos 0-7, if none return -1
        static int FromMouse(Span<bool> newStates, Camera mainCamera)
        {
            try
            {
                return PositionToSensorState(newStates, mainCamera, Input.mousePosition);
            }
            catch (NullReferenceException)
            {
                GameManager.OnSceneChanged();
            }
            catch (UnityException)
            {
                GameManager.OnSceneChanged();
            }

            return -1;

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
                var rad = _radius;
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
        bool isInRange(in float input,in float angle,in float range = 11.25f) => Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
    }
}
