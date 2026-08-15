using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnDragMessageListener))]
    public sealed class UnityOnDragMessageListener : MessageListener, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnDrag, gameObject, eventData);
        }
    }
}
