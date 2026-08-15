using System;
using UnityEditor.Scripting.ScriptCompilation;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// Provides a wrapper for a Custom Script Assembly and exposes its basic properties.
    /// </summary>
    internal interface ICustomScriptAssembly
    {
        /// <summary>
        /// Checks if the Custom Script Assembly is referencing the provided precompiled library.
        /// </summary>
        /// <param name="libraryFilename">The name of the precompiled library reference to be checked.</param>
        /// <returns>True if the assembly references the provided precompiled library; false otherwise.</returns>
        bool HasPrecompiledReference(string libraryFilename);

        /// <summary>
        /// Checks if the Custom Script Assembly has the provided <see cref="AssemblyFlags" /> value set.
        /// </summary>
        /// <param name="flag">The <see cref="AssemblyFlags" /> value to check against.</param>
        /// <returns>True if the provided <paramref name="flag" /> value is set; false otherwise.</returns>
        bool HasAssemblyFlag(AssemblyFlags flag);
    }
}
