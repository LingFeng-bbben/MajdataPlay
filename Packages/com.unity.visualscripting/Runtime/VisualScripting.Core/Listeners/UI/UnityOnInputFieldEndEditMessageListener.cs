using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnInputFieldEndEditMessageListener))]
    public sealed class UnityOnInputFieldEndEditMessageListener : MessageListener
    {
        private void Start()
        {
            GetComponent<InputField>()?.onEndEdit?.AddListener((value) =>
                EventBus.Trigger(EventHooks.OnInputFieldEndEdit, gameObject, value));
        }
    }
}
