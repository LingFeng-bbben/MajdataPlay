using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnTriggerEnterMessageListener))]
    public sealed class UnityOnTriggerEnterMessageListener : MessageListener
    {
        private void OnTriggerEnter(Collider other)
        {
            EventBus.Trigger(EventHooks.OnTriggerEnter, gameObject, other);
        }
    }
#endif
}
