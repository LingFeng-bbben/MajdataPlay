using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnCollisionExitMessageListener))]
    public sealed class UnityOnCollisionExitMessageListener : MessageListener
    {
        private void OnCollisionExit(Collision collision)
        {
            EventBus.Trigger(EventHooks.OnCollisionExit, gameObject, collision);
        }
    }
#endif
}
