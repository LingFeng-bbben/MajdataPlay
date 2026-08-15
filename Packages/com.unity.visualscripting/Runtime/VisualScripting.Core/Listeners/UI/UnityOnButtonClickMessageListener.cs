using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnButtonClickMessageListener))]
    public sealed class UnityOnButtonClickMessageListener : MessageListener
    {
        private void Start()
        {
            GetComponent<Button>()?.onClick
            ?.AddListener(() => EventBus.Trigger(EventHooks.OnButtonClick, gameObject));
        }
    }
}
