using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.TestTools.Utils;
#if UNITY_6000_5_OR_NEWER
using UnityEngine;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorAssemblyWrapper : AssemblyWrapper
    {
        public EditorAssemblyWrapper(Assembly assembly)
            : base(assembly) {}

        public override AssemblyName[] GetReferencedAssemblies()
        {
            return Assembly.GetReferencedAssemblies();
        }

        public override string Location
        {
            get
            {
#if UNITY_6000_5_OR_NEWER
                return Assembly.GetLoadedAssemblyPath();
#else
                return Assembly.Location;
#endif
            }
        }
    }
}
