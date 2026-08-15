namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseDownMessageListener))]
    public sealed class UnityOnMouseDownMessageListener : MessageListener
    {
        private void OnMouseDown()
        {
            EventBus.Trigger(EventHooks.OnMouseDown, gameObject);
        }
    }
}
