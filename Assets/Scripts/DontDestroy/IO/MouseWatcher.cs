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
        private static float _radius => MajEnv.UserSetting.Misc.InputDevice.TouchPanel.TouchSimulationRadius;
        static void UpdateMousePosition()
        {
            var sensors = _sensors.Span;
            var mainCamera = GameManager.MainCamera;
            Span<bool> newStates = stackalloc bool[34];
            if (Input.touchCount > 0)
            {
                FromTouchPanel(newStates, mainCamera);
            }
            else if (Input.GetMouseButton(0))
            {
                FromMouse(newStates, mainCamera);
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
        }
        static void FromTouchPanel(Span<bool> newStates, Camera mainCamera)
        {
            MajDebug.Log(Input.touchCount);

            try
            {
                for (var j = 0; j< Input.touchCount; j++)
                {
                    var touch = Input.GetTouch(j);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        continue;
                    }
                    Vector3 cubeRay = mainCamera.ScreenToWorldPoint(touch.position);
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
        static void FromMouse(Span<bool> newStates, Camera mainCamera)
        {
            try
            {
                Vector3 cubeRay = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                for(int i=0;i<9; i++)
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
                
            }
            catch (NullReferenceException)
            {
                GameManager.OnSceneChanged();
            }
            catch (UnityException)
            {
                GameManager.OnSceneChanged();
            }
        }
        bool isInRange(in float input,in float angle,in float range = 11.25f) => Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
    }
}
