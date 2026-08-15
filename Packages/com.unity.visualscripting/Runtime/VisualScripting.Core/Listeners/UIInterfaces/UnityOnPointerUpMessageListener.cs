using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnPointerUpMessageListener))]
    public sealed class UnityOnPointerUpMessageListener : MessageListener, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnPointerUp, gameObject, eventData);
        }
    }
}
