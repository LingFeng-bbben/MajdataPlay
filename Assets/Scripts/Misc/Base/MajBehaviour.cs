using MajdataPlay.Databases;
using MajdataPlay.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
    public abstract class MajBehaviour : MonoBehaviour
    {
        [field: SerializeField, ReadOnlyField]
        protected GameRuntime RuntimeDatabase { get; private set; }

        protected virtual void Awake()
        {
            RuntimeDatabase = GameRuntime.Instance;

            var type = GetType();
            if(type.GetCustomAttribute<DontDestroyOnLoadAttribute>() is not null)
            {
                DontDestroyOnLoad(this);
            }
        }
    }
}
