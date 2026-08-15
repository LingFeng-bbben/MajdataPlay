using System;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// The set of Menu Items dedicated to creating test assets: Test Scripts and Custom Test Assemblies.
    /// </summary>
    internal static class TestScriptAssetMenuItems
    {
        internal const string addNewFolderWithTestAssemblyDefinitionMenuItem = "Assets/Create/Testing/Tests Assembly Folder";
        internal const string addNewTestScriptMenuItem = "Assets/Create/Testing/C# Test Script";

        /// <summary>
        /// Adds a new folder asset and an associated Custom Test Assembly in the active folder path.
        /// </summary>
        [MenuItem(addNewFolderWithTestAssemblyDefinitionMenuItem, false, 83)]
        public static void AddNewFolderWithTestAssemblyDefinition()
        {
            TestScriptAssetsCreator.Instance.AddNewFolderWithTestAssemblyDefinition();
        }

        /// <summary>
        /// Checks if it is possible to add a new Custom Test Assembly inside the active folder path.
        /// </summary>
        /// <returns>False if the active folder path already contains a Custom Test Assembly; true otherwise.</returns>
        [MenuItem(addNewFolderWithTestAssemblyDefinitionMenuItem, true, 83)]
        public static bool CanAddNewFolderWithTestAssemblyDefinition()
        {
            var testAssemblyAlreadyExists = TestScriptAssetsCreator.Instance.ActiveFolderContainsTestAssemblyDefinition();
            return !testAssemblyAlreadyExists;
        }

        /// <summary>
        /// Adds a new Test Script asset in the active folder path.
        /// </summary>
        [MenuItem(addNewTestScriptMenuItem, false, 83)]
        public static void AddNewTestScript()
        {
            TestScriptAssetsCreator.Instance.AddNewTestScript();
        }

        /// <summary>
        /// Checks if it is possible to add a new Test Script in the active folder path.
        /// </summary>
        /// <returns>True if a Test Script can be compiled in the active folder path; false otherwise.</returns>
        [MenuItem(addNewTestScriptMenuItem, true, 83)]
        public static bool CanAddNewTestScript()
        {
            var testScriptWillCompile = TestScriptAssetsCreator.Instance.TestScriptWillCompileInActiveFolder();
            return testScriptWillCompile;
        }
    }
}
