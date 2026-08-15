using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnTriggerExitMessageListener))]
    public sealed class UnityOnTriggerExitMessageListener : MessageListener
    {
        private void OnTriggerExit(Collider other)
        {
            EventBus.Trigger(EventHooks.OnTriggerExit, gameObject, other);
        }
    }
#endif
}
