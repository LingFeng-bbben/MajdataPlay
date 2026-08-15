using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Filters;

namespace UnityEngine.TestTools.TestRunner.GUI
{
    internal class SynchronousFilter : NonExplicitFilter
    {
        public new TNode ToXml(bool recursive)
        {
            return new TNode("synchronousOnly");
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            return parentNode.AddElement("synchronousOnly");
        }

        public override bool Match(ITest test)
        {
            if (test.Method == null)
                return true;

            if (test.Method.ReturnType.Type == typeof(IEnumerator))
                return false;

            if (test.Method.GetCustomAttributes<IOuterUnityTestAction>(true).Any())
                return false;

            if (test.TypeInfo?.Type != null)
            {
                if (Reflect.GetMethodsWithAttribute(test.TypeInfo.Type, typeof(UnitySetUpAttribute), true)
                    .Any(mi => mi.ReturnType == typeof(IEnumerator)))
                    return false;

                if (Reflect.GetMethodsWithAttribute(test.TypeInfo.Type, typeof(UnityTearDownAttribute), true)
                    .Any(mi => mi.ReturnType == typeof(IEnumerator)))
                    return false;
            }

            if (test.Method.ReturnType.Type == typeof(void))
                return true;

            if (test.Method.ReturnType.Type == typeof(Task))
                return true;

            return false;
        }

        public override bool Pass(ITest test)
        {
            return Match(test);
        }
    }
}
