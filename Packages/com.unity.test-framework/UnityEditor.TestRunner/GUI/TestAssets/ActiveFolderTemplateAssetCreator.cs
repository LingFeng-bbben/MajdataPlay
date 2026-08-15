namespace UnityEditor.TestTools.TestRunner.GUI.TestAssets
{
    /// <inheritdoc />
    internal class ActiveFolderTemplateAssetCreator : IActiveFolderTemplateAssetCreator
    {
        /// <inheritdoc />
        public string GetActiveFolderPath()
        {
            return AssetDatabase.GetAssetPath(Selection.activeObject);
        }

        /// <inheritdoc />
        public void CreateFolderWithTemplates(string defaultName, params string[] templateNames)
        {
            ProjectWindowUtil.CreateFolderWithTemplates(defaultName, templateNames);
        }

        /// <inheritdoc />
        public void CreateScriptAssetFromTemplateFile(string defaultName, string templatePath)
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultName);
        }
    }
}
