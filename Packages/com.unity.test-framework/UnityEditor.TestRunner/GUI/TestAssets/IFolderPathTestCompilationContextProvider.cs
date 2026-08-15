using System;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// Provides Test Script compilation context associated with project folder paths.
    /// </summary>
    internal interface IFolderPathTestCompilationContextProvider
    {
        /// <summary>
        /// Checks if the provided folder path belongs to a Custom Test Assembly.
        /// </summary>
        bool FolderPathBelongsToCustomTestAssembly(string folderPath);

        /// <summary>
        /// Checks if the provided folder path belongs to an assembly capable of compiling Test Scripts.
        /// </summary>
        bool TestScriptWillCompileInFolderPath(string folderPath);
    }
}
