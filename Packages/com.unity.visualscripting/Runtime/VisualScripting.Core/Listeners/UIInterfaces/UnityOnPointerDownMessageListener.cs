using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnPointerDownMessageListener))]
    public sealed class UnityOnPointerDownMessageListener : MessageListener, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnPointerDown, gameObject, eventData);
        }
    }
}
