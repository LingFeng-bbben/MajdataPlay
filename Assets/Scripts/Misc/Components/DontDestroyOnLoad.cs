using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay.Components
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("dontDestroyOnLoad")]
        bool _dontDestroyOnLoad;

        void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
