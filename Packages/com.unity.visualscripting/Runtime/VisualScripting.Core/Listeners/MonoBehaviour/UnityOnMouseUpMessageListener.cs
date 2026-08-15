namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseUpMessageListener))]
    public sealed class UnityOnMouseUpMessageListener : MessageListener
    {
        private void OnMouseUp()
        {
            EventBus.Trigger(EventHooks.OnMouseUp, gameObject);
        }
    }
}
