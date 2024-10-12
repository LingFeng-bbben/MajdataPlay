using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public class SkipButton : MonoBehaviour
    {
        bool a = false;
        void OnAreaDown(object? sender,InputEventArgs args)
        {
            if (a)
                return;
            a = true;
            GamePlayManager.Instance.EndGame();
            Destroy(gameObject);
        }
        void OnEnable()
        {
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.E5);
        }
        void OnDestroy()
        {
            OnDisable();
        }
        void OnDisable()
        {
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E5);
        }
    }
}
