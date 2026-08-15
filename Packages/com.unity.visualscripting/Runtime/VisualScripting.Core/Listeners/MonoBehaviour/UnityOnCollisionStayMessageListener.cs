using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnCollisionStayMessageListener))]
    public sealed class UnityOnCollisionStayMessageListener : MessageListener
    {
        private void OnCollisionStay(Collision collision)
        {
            EventBus.Trigger(EventHooks.OnCollisionStay, gameObject, collision);
        }
    }
#endif
}
