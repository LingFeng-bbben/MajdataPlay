using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnScrollMessageListener))]
    public sealed class UnityOnScrollMessageListener : MessageListener, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnScroll, gameObject, eventData);
        }
    }
}
