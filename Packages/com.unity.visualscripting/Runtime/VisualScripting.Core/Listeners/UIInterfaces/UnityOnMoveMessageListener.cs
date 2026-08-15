using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    [UnityEngine.AddComponentMenu("")]
    [VisualScriptingHelpURL(typeof(UnityOnMoveMessageListener))]
    public sealed class UnityOnMoveMessageListener : MessageListener, IMoveHandler
    {
        public void OnMove(AxisEventData eventData)
        {
            EventBus.Trigger(EventHooks.OnMove, gameObject, eventData);
        }
    }
}
