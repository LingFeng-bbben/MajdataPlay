namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnTransformChildrenChangedMessageListener))]
    public sealed class UnityOnTransformChildrenChangedMessageListener : MessageListener
    {
        private void OnTransformChildrenChanged()
        {
            EventBus.Trigger(EventHooks.OnTransformChildrenChanged, gameObject);
        }
    }
}
