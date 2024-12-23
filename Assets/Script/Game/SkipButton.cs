using Cysharp.Threading.Tasks;
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
        bool _isBound = false;
        bool a = false;
        void OnAreaDown(object? sender,InputEventArgs args)
        {
            if (a)
                return;
            a = true;
            MajInstanceHelper<GamePlayManager>.Instance!.EndGame().Forget();
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
            _isBound = false;
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E5);
        }
    }
}
