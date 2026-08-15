namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMouseExitMessageListener))]
    public sealed class UnityOnMouseExitMessageListener : MessageListener
    {
        private void OnMouseExit()
        {
            EventBus.Trigger(EventHooks.OnMouseExit, gameObject);
        }
    }
}
