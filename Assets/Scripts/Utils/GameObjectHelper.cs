using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class GameObjectHelper
    {
        public static void Destroy<T>(ref T? obj) where T : UnityEngine.Object?
        {
            if (obj is null)
                return;
            UnityEngine.Object.Destroy(obj);
            obj = null;
        }
        public static void Destroy<T>(ref T?[] objs) where T : UnityEngine.Object?
        {
            for (int i = 0; i < objs.Length; i++)
                Destroy(ref objs[i]);
        }
    }
}
