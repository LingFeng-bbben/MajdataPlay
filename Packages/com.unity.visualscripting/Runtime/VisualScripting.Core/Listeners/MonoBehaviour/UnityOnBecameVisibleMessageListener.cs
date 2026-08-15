namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnBecameVisibleMessageListener))]
    public sealed class UnityOnBecameVisibleMessageListener : MessageListener
    {
        private void OnBecameVisible()
        {
            EventBus.Trigger(EventHooks.OnBecameVisible, gameObject);
        }
    }
}
