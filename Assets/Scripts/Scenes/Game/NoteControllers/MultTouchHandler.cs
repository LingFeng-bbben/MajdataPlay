using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    public class MultTouchHandler : MonoBehaviour
    {
        public GameObject BorderPrefab;
        TouchBorder[] borders = new TouchBorder[33];

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
                borders[i] = border;
            }
        }
        private void OnDestroy()
        {
            Majdata<MultTouchHandler>.Free();
        }
        internal void Clear()
        {
            foreach (var border in borders)
            {
                border.Clear();
            }
        }
        public void Register(SensorArea area, bool isEach, bool isBreak)
        {
            var border = borders[(int)area];
            border.Add(isBreak, isEach);
        }
        public void Unregister(SensorArea area)
        {
            var border = borders[(int)area];
            border.Remove();
        }
    }
}