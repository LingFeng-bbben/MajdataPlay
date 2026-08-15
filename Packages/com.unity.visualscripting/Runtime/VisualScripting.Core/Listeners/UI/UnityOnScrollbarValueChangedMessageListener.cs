using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnScrollbarValueChangedMessageListener))]
    public sealed class UnityOnScrollbarValueChangedMessageListener : MessageListener
    {
        private void Start()
        {
            GetComponent<Scrollbar>()?.onValueChanged?.AddListener((value) =>
                EventBus.Trigger(EventHooks.OnScrollbarValueChanged, gameObject, value));
        }
    }
}
