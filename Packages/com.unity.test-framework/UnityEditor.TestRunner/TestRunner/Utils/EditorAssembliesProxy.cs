using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorAssembliesProxy : IEditorAssembliesProxy
    {
        public IAssemblyWrapper[] loadedAssemblies
        {
            get {
                var assemblies = new SortedDictionary<string, EditorAssemblyWrapper>();
                foreach (var assembly in EditorAssemblies.loadedAssemblies)
                {
                    assemblies.TryAdd(assembly.FullName, new EditorAssemblyWrapper(assembly));
                }

                return assemblies.Select(pair => (IAssemblyWrapper)pair.Value).ToArray();
            }
        }
    }
}
