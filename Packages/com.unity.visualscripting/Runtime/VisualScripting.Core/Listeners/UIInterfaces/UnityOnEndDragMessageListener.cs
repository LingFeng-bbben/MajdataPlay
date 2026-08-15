using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnEndDragMessageListener))]
    public sealed class UnityOnEndDragMessageListener : MessageListener, IEndDragHandler
    {
        public void OnEndDrag(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnEndDrag, gameObject, eventData);
        }
    }
}
