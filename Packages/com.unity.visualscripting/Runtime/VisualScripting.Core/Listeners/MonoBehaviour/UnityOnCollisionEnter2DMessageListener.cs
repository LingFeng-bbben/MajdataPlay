using UnityEngine;

namespace Unity.VisualScripting
{
#if MODULE_PHYSICS_2D_EXISTS
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnCollisionEnter2DMessageListener))]
    public sealed class UnityOnCollisionEnter2DMessageListener : MessageListener
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            EventBus.Trigger(EventHooks.OnCollisionEnter2D, gameObject, collision);
        }
    }
#endif
}
