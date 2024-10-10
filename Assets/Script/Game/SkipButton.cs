using MajdataPlay.IO;
using MajdataPlay.Types;
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
            InputManager.Instance.BindSensor(OnAreaDown, SensorType.B4);
            InputManager.Instance.BindSensor(OnAreaDown, SensorType.B5);
            InputManager.Instance.BindSensor(OnAreaDown, SensorType.E5);
        }
        void OnDestroy()
        {
            OnDisable();
        }
        void OnDisable()
        {
            InputManager.Instance.UnbindSensor(OnAreaDown, SensorType.B4);
            InputManager.Instance.UnbindSensor(OnAreaDown, SensorType.B5);
            InputManager.Instance.UnbindSensor(OnAreaDown, SensorType.E5);
        }
    }
}
