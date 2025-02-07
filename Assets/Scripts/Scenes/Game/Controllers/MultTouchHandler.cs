using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class MultTouchHandler : MonoBehaviour
    {
        public GameObject BorderPrefab;
        TouchBorder[] borders = new TouchBorder[33];

        private void Start()
        {
            for (var i = 0; i < 33; i++)
            {
                var sensorType = (SensorType)i;
                var obj = Instantiate(BorderPrefab,transform);
                var border = obj.GetComponent<TouchBorder>();
                border.AreaPosition = sensorType;
                borders[i] = border;
            }
        }
        public void Register(SensorType area,bool isEach,bool isBreak)
        {
            var border = borders[(int)area];
            border.Add(isBreak, isEach);
        }
        public void Unregister(SensorType area)
        {
            var border = borders[(int)area];
            border.Remove();
        }
    }
}