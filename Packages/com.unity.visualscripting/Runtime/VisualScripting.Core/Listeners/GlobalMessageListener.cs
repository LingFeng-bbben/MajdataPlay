using UnityEngine;

namespace Unity.VisualScripting
{
    [Singleton(Name = "VisualScripting GlobalEventListener", Automatic = true, Persistent = true)]
    [DisableAnnotation]
    [AddComponentMenu("")]
    [IncludeInSettings(false)]
    [TypeIcon(typeof(MessageListener))]
    [VisualScriptingHelpURL(typeof(GlobalMessageListener))]
    public sealed class GlobalMessageListener : MonoBehaviour, ISingleton
    {
#if !PLATFORM_VISIONOS
        private void OnGUI()
        {
            EventBus.Trigger(EventHooks.OnGUI);
        }
#endif

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                EventBus.Trigger(EventHooks.OnApplicationFocus);
            }
            else
            {
                EventBus.Trigger(EventHooks.OnApplicationLostFocus);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                EventBus.Trigger(EventHooks.OnApplicationPause);
            }
            else
            {
                EventBus.Trigger(EventHooks.OnApplicationResume);
            }
        }

        private void OnApplicationQuit()
        {
            EventBus.Trigger(EventHooks.OnApplicationQuit);
        }

        public static void Require()
        {
            // Call the singleton getter to force instantiation
            var instance = Singleton<GlobalMessageListener>.instance;
        }
    }
}
