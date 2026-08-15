using UnityEngine.UI;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnSliderValueChangedMessageListener))]
    public sealed class UnityOnSliderValueChangedMessageListener : MessageListener
    {
        private void Start()
        {
            GetComponent<Slider>()?.onValueChanged?.AddListener((value) =>
                EventBus.Trigger(EventHooks.OnSliderValueChanged, gameObject, value));
        }
    }
}
