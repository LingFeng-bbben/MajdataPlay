using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnPointerClickMessageListener))]
    public sealed class UnityOnPointerClickMessageListener : MessageListener, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnPointerClick, gameObject, eventData);
        }
    }
}
