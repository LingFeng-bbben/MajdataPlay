using System;
using UnityEngine;

namespace Unity.VisualScripting
{
    internal sealed class GradientCloner : Cloner<Gradient>
    {
        public override bool Handles(Type type)
        {
            return type == typeof(Gradient);
        }

        public override Gradient ConstructClone(Type type, Gradient original)
        {
            return new Gradient();
        }

        public override void FillClone(Type type, ref Gradient clone, Gradient original, CloningContext context)
        {
            clone.mode = original.mode;
            clone.SetKeys(original.colorKeys, original.alphaKeys);
        }
    }
}
