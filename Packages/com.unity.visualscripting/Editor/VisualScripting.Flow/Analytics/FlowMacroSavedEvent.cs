namespace Unity.VisualScripting.Analytics
{
    class FlowMacroSavedEvent : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            UsageAnalytics.CollectAndSendForType<ScriptGraphAsset>(paths);
            return paths;
        }
    }
}
