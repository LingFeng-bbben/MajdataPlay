namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseEnterMessageListener))]
    public sealed class UnityOnMouseEnterMessageListener : MessageListener
    {
        private void OnMouseEnter()
        {
            EventBus.Trigger(EventHooks.OnMouseEnter, gameObject);
        }
    }
}
