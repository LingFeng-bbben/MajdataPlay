using System;
using System.IO;
using UnityEditor.Scripting.ScriptCompilation;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EditorCompilationInterfaceProxy : IEditorCompilationInterfaceProxy
    {
        public ScriptAssembly[] GetAllEditorScriptAssemblies()
        {
            return EditorCompilationInterface.Instance.GetAllEditorScriptAssemblies(EditorCompilationInterface.GetAdditionalEditorScriptCompilationOptions());
        }

        public PrecompiledAssembly[] GetAllPrecompiledAssemblies()
        {
            return EditorCompilationInterface.Instance.GetAllPrecompiledAssemblies();
        }

        public bool IsMSBuildEnabled()
        {
            return UnityEditor.Compilation.CompilationPipeline.IsUsingMSBuild();
        }
    }
}
