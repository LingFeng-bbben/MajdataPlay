using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnDeselectMessageListener))]
    public sealed class UnityOnDeselectMessageListener : MessageListener, IDeselectHandler
    {
        public void OnDeselect(BaseEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnDeselect, gameObject, eventData);
        }
    }
}
