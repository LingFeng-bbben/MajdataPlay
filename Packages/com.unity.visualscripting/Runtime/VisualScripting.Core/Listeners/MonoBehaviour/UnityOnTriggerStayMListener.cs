using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnTriggerStayMessageListener))]
    public sealed class UnityOnTriggerStayMessageListener : MessageListener
    {
        private void OnTriggerStay(Collider other)
        {
            EventBus.Trigger(EventHooks.OnTriggerStay, gameObject, other);
        }
    }
#endif
}
