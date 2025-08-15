using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    public class SkipButton : MonoBehaviour
    {
        bool _isBound = false;
        bool a = false;
        void OnAreaDown(object? sender,InputEventArgs args)
        {
            if (a)
                return;
            a = true;
            Majdata<GamePlayManager>.Instance!.EndGame().Forget();
            Destroy(gameObject);
        }
        void OnEnable()
        {
            DelayBind().Forget();
        }
        async UniTaskVoid DelayBind()
        {
            if (_isBound)
                return;
            _isBound = true;
            await UniTask.Delay(1000);
            InputManager.BindSensor(OnAreaDown, SensorArea.B4);
            InputManager.BindSensor(OnAreaDown, SensorArea.B5);
            InputManager.BindSensor(OnAreaDown, SensorArea.E5);
        }
        void OnDestroy()
        {
            OnDisable();
        }
        void OnDisable()
        {
            _isBound = false;
            InputManager.UnbindSensor(OnAreaDown, SensorArea.B4);
            InputManager.UnbindSensor(OnAreaDown, SensorArea.B5);
            InputManager.UnbindSensor(OnAreaDown, SensorArea.E5);
        }
    }
}
