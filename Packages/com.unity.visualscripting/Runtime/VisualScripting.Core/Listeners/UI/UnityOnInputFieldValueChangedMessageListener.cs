using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnInputFieldValueChangedMessageListener))]
    public sealed class UnityOnInputFieldValueChangedMessageListener : MessageListener
    {
        private void Start()
        {
            GetComponent<InputField>()?.onValueChanged?.AddListener((value) =>
                EventBus.Trigger(EventHooks.OnInputFieldValueChanged, gameObject, value));
        }
    }
}
