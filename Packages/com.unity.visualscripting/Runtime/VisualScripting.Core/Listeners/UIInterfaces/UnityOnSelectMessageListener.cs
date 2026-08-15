using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnSelectMessageListener))]
    public sealed class UnityOnSelectMessageListener : MessageListener, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnSelect, gameObject, eventData);
        }
    }
}
