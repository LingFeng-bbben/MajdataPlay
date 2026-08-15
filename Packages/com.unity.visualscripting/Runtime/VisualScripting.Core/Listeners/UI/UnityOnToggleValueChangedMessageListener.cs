using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnToggleValueChangedMessageListener))]
    public sealed class UnityOnToggleValueChangedMessageListener : MessageListener
    {
        private void Start() => GetComponent<Toggle>()?.onValueChanged?.AddListener((value) =>
            EventBus.Trigger(EventHooks.OnToggleValueChanged, gameObject, value));
    }
}
