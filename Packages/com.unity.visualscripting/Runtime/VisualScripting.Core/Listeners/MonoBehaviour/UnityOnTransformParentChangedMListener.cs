namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnTransformParentChangedMessageListener))]
    public sealed class UnityOnTransformParentChangedMessageListener : MessageListener
    {
        private void OnTransformParentChanged()
        {
            EventBus.Trigger(EventHooks.OnTransformParentChanged, gameObject);
        }
    }
}
