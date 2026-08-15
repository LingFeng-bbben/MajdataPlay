using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorLoadedTestAssemblyProvider : IEditorLoadedTestAssemblyProvider
    {
        private const string k_NunitAssemblyName = "nunit.framework";
        private const string k_TestRunnerAssemblyName = "UnityEngine.TestRunner";
        internal const string k_PerformanceTestingAssemblyName = "Unity.PerformanceTesting";

        private readonly IEditorAssembliesProxy m_EditorAssembliesProxy;
        private readonly ScriptAssembly[] m_AllEditorScriptAssemblies;
        private readonly PrecompiledAssembly[] m_AllPrecompiledAssemblies;
        private readonly bool m_IsMSBuildEnabled;

        public EditorLoadedTestAssemblyProvider(IEditorCompilationInterfaceProxy compilationInterfaceProxy, IEditorAssembliesProxy editorAssembliesProxy)
        {
            m_EditorAssembliesProxy = editorAssembliesProxy;
            m_AllEditorScriptAssemblies = compilationInterfaceProxy.GetAllEditorScriptAssemblies();
            m_AllPrecompiledAssemblies = compilationInterfaceProxy.GetAllPrecompiledAssemblies();
            m_IsMSBuildEnabled = compilationInterfaceProxy.IsMSBuildEnabled();
        }

        public List<IAssemblyWrapper> GetAssembliesGroupedByType(TestPlatform mode)
        {
            var assemblies = GetAssembliesGroupedByTypeAsync(mode);
            while (assemblies.MoveNext())
            {
            }

            return assemblies.Current.Where(pair => mode.IsFlagIncluded(pair.Key)).SelectMany(pair => pair.Value).ToList();
        }

        public IEnumerator<IDictionary<TestPlatform, List<IAssemblyWrapper>>> GetAssembliesGroupedByTypeAsync(TestPlatform mode)
        {
            IAssemblyWrapper[] loadedAssemblies = m_EditorAssembliesProxy.loadedAssemblies;

            IDictionary<TestPlatform, List<IAssemblyWrapper>> result = new Dictionary<TestPlatform, List<IAssemblyWrapper>>
            {
                {TestPlatform.EditMode, new List<IAssemblyWrapper>() },
                {TestPlatform.PlayMode, new List<IAssemblyWrapper>() }
            };
            var filteredAssemblies = FilterAssembliesWithTestReference(loadedAssemblies);

            foreach (var loadedAssembly in filteredAssemblies)
            {
                var assemblyName = new FileInfo(loadedAssembly.Location).Name;
                var scriptAssemblies = m_AllEditorScriptAssemblies.Where(x => x.Filename == assemblyName).ToList();
                var precompiledAssemblies = m_AllPrecompiledAssemblies.Where(x => new FileInfo(x.Path).Name == assemblyName).ToList();

                TestPlatform assemblyType;
                if (scriptAssemblies.Count < 1 && precompiledAssemblies.Count < 1)
                {
                    // Untracked: MSBU outputs land here. Fall back to [assembly: PlayModeTests].
                    if (!m_IsMSBuildEnabled)
                        continue;

                    assemblyType = loadedAssembly.HasCustomAttribute(typeof(PlayModeTestsAttribute))
                        ? TestPlatform.PlayMode
                        : TestPlatform.EditMode;
                }
                else
                {
                    var assemblyFlags = scriptAssemblies.Any() ? scriptAssemblies.First().Flags : precompiledAssemblies.First().Flags;
                    assemblyType = (assemblyFlags & UnityEditor.Scripting.ScriptCompilation.AssemblyFlags.EditorOnly) == UnityEditor.Scripting.ScriptCompilation.AssemblyFlags.EditorOnly
                        ? TestPlatform.EditMode
                        : TestPlatform.PlayMode;
                }

                result[assemblyType].Add(loadedAssembly);
                yield return null;
            }

            yield return result;
        }

        private IAssemblyWrapper[] FilterAssembliesWithTestReference(IAssemblyWrapper[] loadedAssemblies)
        {
            var resultsCache = new Dictionary<IAssemblyWrapper, bool>();
            var loadedAssembliesDict = loadedAssemblies.ToDictionary(asm => asm.Name.Name, asm => asm);
            return loadedAssemblies
                       .Where(assembly => FilterAssemblyForTestReference(assembly, loadedAssembliesDict, resultsCache, new HashSet<string>()))
                       .ToArray();
        }

        private bool FilterAssemblyForTestReference(IAssemblyWrapper assemblyToFilter, IReadOnlyDictionary<string, IAssemblyWrapper> loadedAssemblies, IDictionary<IAssemblyWrapper, bool> resultsCache, HashSet<string> visitedAssemblies)
        {
            if (!visitedAssemblies.Add(assemblyToFilter.Name.FullName))
            {
                return false;
            }

            if (resultsCache.TryGetValue(assemblyToFilter, out var existingResult))
            {
                return existingResult;
            }

            foreach (var reference in assemblyToFilter.GetReferencedAssemblies())
            {
                if (IsTestReference(reference) || (loadedAssemblies.TryGetValue(reference.Name, out var referencedAssembly) && FilterAssemblyForTestReference(referencedAssembly, loadedAssemblies, resultsCache, visitedAssemblies)))
                {
                    resultsCache[assemblyToFilter] = true;
                    return true;
                }
            }

            resultsCache[assemblyToFilter] = false;
            return false;
        }

        private static bool IsTestReference(System.Reflection.AssemblyName assemblyName)
        {
            return assemblyName.Name == k_NunitAssemblyName ||
                   assemblyName.Name == k_TestRunnerAssemblyName ||
                   assemblyName.Name == k_PerformanceTestingAssemblyName;
        }
    }
}
