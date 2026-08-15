using System;
using Unity.VisualScripting;
using UnityEngine;

class EventBusExamples
{
    #region CheatCodeController
    public class CheatCodeController : MonoBehaviour
    {
        public const string CheatCodeActivated = "CheatCodeActivated";

        static readonly KeyCode[] famousCheatCode = {
            KeyCode.UpArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.B,
            KeyCode.A
        };

        int index;
        EventHook cheatCodeHook;
        Action<EmptyEventArgs> godModeDelegate;

        // A GameObject with a ScriptMachine that holds a graph with a
        // CheatCodeEnabled Event Node.
        public GameObject player;

        void Start()
        {
            // Hold a reference to the EventHook and the Delegate for the
            // EventBus.Unregister call in the OnDestroy method.
            cheatCodeHook = new EventHook(CheatCodeActivated);
            godModeDelegate = _ => EnableGodMode();

            EventBus.Register(cheatCodeHook, godModeDelegate);
        }

        void Update()
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(famousCheatCode[index]))
                {
                    index++;
                }
                else
                {
                    index = 0;
                }

                if (index >= famousCheatCode.Length)
                {
                    // Triggers the EnableGodMode delegate
                    EventBus.Trigger(CheatCodeActivated);

                    // Triggers the CheatCodeEnabled Visual Scripting Node
                    EventBus.Trigger(new EventHook(
                        CheatCodeActivated,
                        player.GetComponent<ScriptMachine>()));

                    index = 0;
                }
            }
        }

        void OnDestroy()
        {
            EventBus.Unregister(cheatCodeHook, godModeDelegate);
        }

        void EnableGodMode()
        {
            Debug.Log("Cheat code has been entered. Enabling god mode.");
        }
    }
    #endregion

    #region CheatCodeEnabled
    [UnitTitle("On Cheat Code Enabled")]
    public sealed class CheatCodeEnabled : MachineEventUnit<EmptyEventArgs>
    {
        protected override string hookName => CheatCodeController.CheatCodeActivated;
    }
    #endregion
}
