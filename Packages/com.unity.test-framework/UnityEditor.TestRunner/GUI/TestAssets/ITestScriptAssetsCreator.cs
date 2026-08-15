using System;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// Provides an interface for creating test assets from templates.
    /// </summary>
    internal interface ITestScriptAssetsCreator
    {
        /// <summary>
        /// Creates a new folder in the active folder path with an associated Test Script Assembly definition.
        /// </summary>
        /// <param name="isEditorOnly">Should the assembly definition be editor-only?</param>
        void AddNewFolderWithTestAssemblyDefinition(bool isEditorOnly = false);

        /// <summary>
        /// Checks if the active folder path already contains a Test Script Assembly definition.
        /// </summary>
        /// <returns>True if the active folder path contains a Test Script Assembly; false otherwise.</returns>
        bool ActiveFolderContainsTestAssemblyDefinition();

        /// <summary>
        /// Adds a new Test Script asset in the active folder path.
        /// </summary>
        void AddNewTestScript();

        /// <summary>
        /// Checks if a Test Script asset can be compiled in the active folder path.
        /// </summary>
        /// <returns>True if a Test Script can be compiled in the active folder path; false otherwise.</returns>
        bool TestScriptWillCompileInActiveFolder();
    }
}
