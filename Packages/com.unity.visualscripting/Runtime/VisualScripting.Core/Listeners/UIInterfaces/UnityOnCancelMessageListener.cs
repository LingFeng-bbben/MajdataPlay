using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnCancelMessageListener))]
    public sealed class UnityOnCancelMessageListener : MessageListener, ICancelHandler
    {
        public void OnCancel(BaseEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnCancel, gameObject, eventData);
        }
    }
}
