using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game
{
    public sealed class NoteEffectPool : MonoBehaviour
    {
        [SerializeField]
        GameObject tapEffectPrefab;
        [SerializeField]
        GameObject touchEffectPrefab;

        TapEffectDisplayer[] tapEffects = new TapEffectDisplayer[8];
        TouchEffectDisplayer[] touchEffects = new TouchEffectDisplayer[33];

        void Start () 
        {
            for(int i = 0;i < 8;i++)
            {
                var rotation = Quaternion.Euler(0, 0, 22.5f * (i + 1));
                var obj = Instantiate(tapEffectPrefab, transform);
                obj.name = $"TapEffect_{i + 1}";
                obj.transform.rotation = rotation;
                var displayer = obj.GetComponent<TapEffectDisplayer>();
                displayer.ResetAll();
                tapEffects[i] = displayer;
            }
            for(int i = 0;i < 33;i++)
            {
                var sensorPos = (SensorType)i;
                var obj = Instantiate(tapEffectPrefab, transform);
                var displayer = obj.GetComponent<TouchEffectDisplayer>();
                obj.name = $"TouchEffect_{sensorPos}";
                displayer.SensorPos = sensorPos;
                displayer.ResetAll();
                touchEffects[i] = displayer;
            }
        }
    }
}
