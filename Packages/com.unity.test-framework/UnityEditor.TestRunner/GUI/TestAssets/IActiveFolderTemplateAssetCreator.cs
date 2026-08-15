using System;

namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <summary>
    /// Provides basic utility methods for creating assets in the active Project Browser folder path.
    /// </summary>
    internal interface IActiveFolderTemplateAssetCreator
    {
        /// <summary>
        /// The active Project Browser folder path relative to the root project folder.
        /// </summary>
        /// <returns>The active folder path string.</returns>
        string GetActiveFolderPath();

        /// <summary>
        /// Creates a new folder asset in the active folder path with assets defined by provided templates.
        /// </summary>
        /// <param name="defaultName">The default name of the folder.</param>
        /// <param name="templateNames">The names of templates to be used when creating embedded assets.</param>
        void CreateFolderWithTemplates(string defaultName, params string[] templateNames);

        /// <summary>
        /// Creates a new script asset in the active folder path defined by the provided template.
        /// </summary>
        /// <param name="defaultName">The default name of the new script asset.</param>
        /// <param name="templatePath">The template to be used when creating the asset.</param>
        void CreateScriptAssetFromTemplateFile(string defaultName, string templatePath);
    }
}
