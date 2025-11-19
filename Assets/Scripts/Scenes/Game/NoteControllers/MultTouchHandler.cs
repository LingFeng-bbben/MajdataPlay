using MajdataPlay.IO;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    public class MultTouchHandler : MonoBehaviour
    {
        public GameObject BorderPrefab;
        TouchBorder[] _borders = new TouchBorder[33];

        private void Awake()
        {
            Majdata<MultTouchHandler>.Instance = this;
        }
        private void Start()
        {
            for (var i = 0; i < 33; i++)
            {
                var sensorType = (SensorArea)i;
                var obj = Instantiate(BorderPrefab, transform);
                var border = obj.GetComponent<TouchBorder>();
                border.AreaPosition = sensorType;
                _borders[i] = border;
            }
        }
        private void OnDestroy()
        {
            Majdata<MultTouchHandler>.Free();
        }
        internal void Clear()
        {
            foreach (var border in _borders)
            {
                border.Clear();
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Register(SensorArea area, bool isEach, bool isBreak)
        {
            var border = _borders[(int)area];
            border.Add(isBreak, isEach);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unregister(SensorArea area)
        {
            var border = _borders[(int)area];
            border.Remove();
        }
    }
}