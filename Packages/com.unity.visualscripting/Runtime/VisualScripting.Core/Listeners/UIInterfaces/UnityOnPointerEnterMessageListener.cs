using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnPointerEnterMessageListener))]
    public sealed class UnityOnPointerEnterMessageListener : MessageListener, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnPointerEnter, gameObject, eventData);
        }
    }
}
