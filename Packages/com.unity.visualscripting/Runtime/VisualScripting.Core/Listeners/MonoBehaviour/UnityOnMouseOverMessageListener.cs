namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseOverMessageListener))]
    public sealed class UnityOnMouseOverMessageListener : MessageListener
    {
        private void OnMouseOver()
        {
            EventBus.Trigger(EventHooks.OnMouseOver, gameObject);
        }
    }
}
