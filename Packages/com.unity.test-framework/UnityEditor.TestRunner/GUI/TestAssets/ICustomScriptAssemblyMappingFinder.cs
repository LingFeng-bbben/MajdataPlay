using System;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// Provides mapping information from folder paths to their corresponding Custom Script Assembly scope.
    /// </summary>
    internal interface ICustomScriptAssemblyMappingFinder
    {
        /// <summary>
        /// Finds the Custom Script Assembly associated with the provided folder path.
        /// </summary>
        /// <param name="folderPath">The folder path to check.</param>
        /// <returns>The associated <see cref="ICustomScriptAssembly" />; null if none.</returns>
        ICustomScriptAssembly FindCustomScriptAssemblyFromFolderPath(string folderPath);
    }
}
