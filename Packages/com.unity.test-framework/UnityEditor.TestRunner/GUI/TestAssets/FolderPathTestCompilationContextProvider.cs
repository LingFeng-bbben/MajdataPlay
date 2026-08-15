using System;
using System.Linq;
using UnityEditor.Scripting.ScriptCompilation;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <inheritdoc />
    internal class FolderPathTestCompilationContextProvider : IFolderPathTestCompilationContextProvider
    {
        internal const string nUnitLibraryFilename = "nunit.framework.dll";

        private static ICustomScriptAssemblyMappingFinder s_CustomScriptAssemblyMappingFinder;

        internal static ICustomScriptAssemblyMappingFinder CustomScriptAssemblyMappingFinder
        {
            private get => s_CustomScriptAssemblyMappingFinder ?? (s_CustomScriptAssemblyMappingFinder = new CustomScriptAssemblyMappingFinder());
            set => s_CustomScriptAssemblyMappingFinder = value;
        }

        /// <summary>
        /// Checks if the provided folder path belongs to a Custom Test Assembly.
        /// A Custom Test Assembly is defined by a valid reference to the precompiled NUnit library.
        /// </summary>
        /// <param name="folderPath">The folder path to check.</param>
        /// <returns>True if a custom test assembly associated with the provided folder can be found; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="folderPath" /> string argument is null.</exception>
        public bool FolderPathBelongsToCustomTestAssembly(string folderPath)
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            var customScriptAssembly = CustomScriptAssemblyMappingFinder.FindCustomScriptAssemblyFromFolderPath(folderPath);
            var assemblyIsCustomTestAssembly = customScriptAssembly != null
                && customScriptAssembly.HasPrecompiledReference(nUnitLibraryFilename);
            return assemblyIsCustomTestAssembly;
        }

        /// <summary>
        /// Checks if the provided folder path belongs to an assembly capable of compiling Test Scripts.
        /// Unless the <see cref="PlayerSettings.playModeTestRunnerEnabled" /> setting is enabled,
        /// a Test Script can only be compiled in a Custom Test Assembly
        /// or an (implicit or explicit) <see cref="AssemblyFlags.EditorOnly" /> assembly.
        /// </summary>
        /// <param name="folderPath">The folder path to check.</param>
        /// <returns>True if Test Scripts can be successfully compiled when added to this folder path; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="folderPath" /> string argument is null.</exception>
        public bool TestScriptWillCompileInFolderPath(string folderPath)
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            if (PlayerSettings.playModeTestRunnerEnabled)
            {
                return true;
            }

            var customScriptAssembly = CustomScriptAssemblyMappingFinder.FindCustomScriptAssemblyFromFolderPath(folderPath);
            if (customScriptAssembly != null)
            {
                var assemblyCanCompileTestScripts = customScriptAssembly.HasPrecompiledReference(nUnitLibraryFilename)
                    || customScriptAssembly.HasAssemblyFlag(AssemblyFlags.EditorOnly);
                return assemblyCanCompileTestScripts;
            }

            var isImplicitEditorAssembly = FolderPathBelongsToImplicitEditorAssembly(folderPath);
            return isImplicitEditorAssembly;
        }

        /// <summary>
        /// Checks if the provided folder path is a special editor path that belongs to an implicit editor assembly.
        /// </summary>
        /// <param name="folderPath">The folder path to check.</param>
        /// <returns>True if the folder path belongs to an implicit editor assembly; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="folderPath" /> string argument is null.</exception>
        internal static bool FolderPathBelongsToImplicitEditorAssembly(string folderPath)
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            const char unityPathSeparator = '/';
            var unityFormatPath = folderPath.Replace('\\', unityPathSeparator);
            var folderComponents = unityFormatPath.Split(unityPathSeparator);
            var folderComponentsIncludeEditorFolder = folderComponents.Any(n => n.ToLower().Equals("editor"));
            return folderComponentsIncludeEditorFolder;
        }
    }
}
