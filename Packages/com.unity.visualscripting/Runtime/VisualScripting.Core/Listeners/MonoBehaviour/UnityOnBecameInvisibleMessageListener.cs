namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnBecameInvisibleMessageListener))]
    public sealed class UnityOnBecameInvisibleMessageListener : MessageListener
    {
        private void OnBecameInvisible()
        {
            EventBus.Trigger(EventHooks.OnBecameInvisible, gameObject);
        }
    }
}
