namespace Unity.VisualScripting.Analytics
{
    class StateMacroSavedEvent : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            UsageAnalytics.CollectAndSendForType<StateGraphAsset>(paths);
            return paths;
        }
    }
}
