namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnJointBreakMessageListener))]
    public sealed class UnityOnJointBreakMessageListener : MessageListener
    {
        private void OnJointBreak(float breakForce)
        {
            EventBus.Trigger(EventHooks.OnJointBreak, gameObject, breakForce);
        }
    }
}
