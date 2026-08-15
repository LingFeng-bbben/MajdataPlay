using UnityEngine;

namespace Unity.VisualScripting
{
    [AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnParticleCollisionMessageListener))]
    public sealed class UnityOnParticleCollisionMessageListener : MessageListener
    {
        private void OnParticleCollision(GameObject other)
        {
            EventBus.Trigger(EventHooks.OnParticleCollision, gameObject, other);
        }
    }
}
