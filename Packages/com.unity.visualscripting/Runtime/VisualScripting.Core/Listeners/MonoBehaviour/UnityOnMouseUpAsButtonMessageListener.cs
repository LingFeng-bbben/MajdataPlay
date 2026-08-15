namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseUpAsButtonMessageListener))]
    public sealed class UnityOnMouseUpAsButtonMessageListener : MessageListener
    {
        private void OnMouseUpAsButton()
        {
            EventBus.Trigger(EventHooks.OnMouseUpAsButton, gameObject);
        }
    }
}
