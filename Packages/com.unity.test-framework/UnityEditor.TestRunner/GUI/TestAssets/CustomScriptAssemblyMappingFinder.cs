using System;
using System.IO;
using System.Linq;
using UnityEditor.Scripting.ScriptCompilation;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <inheritdoc />
    internal class CustomScriptAssemblyMappingFinder : ICustomScriptAssemblyMappingFinder
    {
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">The provided <paramref name="folderPath" /> string argument is null.</exception>
        public ICustomScriptAssembly FindCustomScriptAssemblyFromFolderPath(string folderPath)
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            var scriptInFolderPath = Path.Combine(folderPath, "Foo.cs");
            var customScriptAssembly = FindCustomScriptAssemblyFromScriptPath(scriptInFolderPath);
            return customScriptAssembly;
        }

        /// <summary>
        /// Finds the Custom Script Assembly associated with the provided script asset path.
        /// </summary>
        /// <param name="scriptPath">The script path to check.</param>
        /// <returns>The associated <see cref="ICustomScriptAssembly" />; null if none.</returns>
        private static ICustomScriptAssembly FindCustomScriptAssemblyFromScriptPath(string scriptPath)
        {
            try
            {
                var customScriptAssembly = EditorCompilationInterface.Instance.FindCustomScriptAssemblyFromScriptPath(scriptPath);
                return new CustomScriptAssemblyWrapper(customScriptAssembly);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Custom Script Assembly wrapper.
        /// </summary>
        internal class CustomScriptAssemblyWrapper : ICustomScriptAssembly
        {
            internal readonly CustomScriptAssembly targetAssembly;

            /// <summary>
            /// Creates a new instance of the <see cref="CustomScriptAssemblyWrapper" /> class.
            /// </summary>
            /// <param name="assembly">The <see cref="CustomScriptAssembly" /> to be represented by the wrapper.</param>
            /// <exception cref="ArgumentNullException">The provided <paramref name="assembly" /> argument is null.</exception>
            internal CustomScriptAssemblyWrapper(CustomScriptAssembly assembly)
            {
                targetAssembly = assembly
                    ?? throw new ArgumentNullException(nameof(assembly), "The provided assembly must not be null.");
            }

            /// <inheritdoc />
            public bool HasPrecompiledReference(string libraryFilename)
            {
                var precompiledReferences = targetAssembly.PrecompiledReferences;
                var libraryReferenceExists = precompiledReferences != null
                    && precompiledReferences.Any(r => Path.GetFileName(r) == libraryFilename);
                return libraryReferenceExists;
            }

            /// <inheritdoc />
            public bool HasAssemblyFlag(AssemblyFlags flag)
            {
                var hasAssemblyFlag = (targetAssembly.AssemblyFlags & flag) == flag;
                return hasAssemblyFlag;
            }
        }
    }
}
