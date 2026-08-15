using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnSubmitMessageListener))]
    public sealed class UnityOnSubmitMessageListener : MessageListener, ISubmitHandler
    {
        public void OnSubmit(BaseEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnSubmit, gameObject, eventData);
        }
    }
}
