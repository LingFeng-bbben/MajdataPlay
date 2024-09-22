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

        TapEffectDisplayer[] tapEffects = new TapEffectDisplayer[8];

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
        }
    }
}
