using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    internal abstract class MajSingleton<T> : MajComponent where T : class
    {
        protected override void Awake()
        {
            base.Awake();
            if(this is T instance)
            {
                Majdata<T>.Instance = instance;
            }
            else
            {
                throw new TypeInitializationException(typeof(T).FullName, new InvalidOperationException("Unable to convert to target type"));
            }
            DontDestroyOnLoad(GameObject);
            base.Awake();
        }

        protected virtual void OnDestroy()
        {
            Majdata<T>.Free();
        }
    }
}
