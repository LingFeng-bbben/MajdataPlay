namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseDragMessageListener))]
    public sealed class UnityOnMouseDragMessageListener : MessageListener
    {
        private void OnMouseDrag()
        {
            EventBus.Trigger(EventHooks.OnMouseDrag, gameObject);
        }
    }
}
