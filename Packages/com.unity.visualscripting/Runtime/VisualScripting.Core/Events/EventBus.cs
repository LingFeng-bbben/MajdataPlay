using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// A global container that maps Event names and Component references to actions for registered listeners.
    /// </summary>
    /// <remarks>
    /// <para>It is the EventMachine base class for ScriptMachine and StateMachine that triggers events.
    /// This overrides almost all Unity callbacks (such as Awake, OnEnable, Update, etc.) and triggers an event
    /// on the EventBus.</para>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how to use the EventBus to send a custom event from a script to a node in
    /// a graph. It also shows how to use the EventBus as a global event manager by executing a callback in a
    /// script, not just a node.
    ///
    /// For more information on how to create custom event nodes refer to the
    /// <a href="../manual/vs-create-own-custom-event-node.html">User Manual</a>.
    ///
    /// In this example we've added some code to a GameObject. This code checks for when the user presses a sequence of
    /// keys to enable a cheat code, then triggers the <c>CheatCodeActivated</c> event. We register the
    /// <c>CheatCodeActivated</c> event in the <c>Start</c> method. The <c>Update</c> method triggers the event twice
    /// with 2 different targets: one for the <c>CheatCodeActivated</c> callback and the other to trigger the
    /// CheatCodeEnabled Node.</para>
    ///
    /// <code source="../../../DocCodeExamples/EventBusExamples.cs" region="CheatCodeController" title="CheatCodeController"/>
    ///
    /// <para>The CheatCodeEnabled Node:</para>
    ///
    /// <code source="../../../DocCodeExamples/EventBusExamples.cs" region="CheatCodeEnabled" title="CheatCodeEnabled"/>
    /// </example>
    public static class EventBus
    {
        static EventBus()
        {
            events = new Dictionary<EventHook, HashSet<Delegate>>(new EventHookComparer());
        }

        private static readonly Dictionary<EventHook, HashSet<Delegate>> events;
        internal static Dictionary<EventHook, HashSet<Delegate>> testAccessEvents => events;

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            events.Clear();
        }
#endif

        public static void Register<TArgs>(EventHook hook, Action<TArgs> handler)
        {
            if (!events.TryGetValue(hook, out var handlers))
            {
                handlers = new HashSet<Delegate>();
                events.Add(hook, handlers);
            }

            handlers.Add(handler);
        }

        public static void Unregister(EventHook hook, Delegate handler)
        {
            if (events.TryGetValue(hook, out var handlers))
            {
                if (handlers.Remove(handler))
                {
                    // Free the key references for GC collection
                    if (handlers.Count == 0)
                    {
                        events.Remove(hook);
                    }
                }
            }
        }

        public static void Trigger<TArgs>(EventHook hook, TArgs args)
        {
            HashSet<Action<TArgs>> handlers = null;

            if (events.TryGetValue(hook, out var potentialHandlers))
            {
                foreach (var potentialHandler in potentialHandlers)
                {
                    if (potentialHandler is Action<TArgs> handler)
                    {
                        if (handlers == null)
                        {
                            handlers = HashSetPool<Action<TArgs>>.New();
                        }

                        handlers.Add(handler);
                    }
                }
            }

            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    if (!potentialHandlers.Contains(handler))
                    {
                        continue;
                    }

                    handler.Invoke(args);
                }

                handlers.Free();
            }
        }

        public static void Trigger<TArgs>(string name, GameObject target, TArgs args)
        {
            Trigger(new EventHook(name, target), args);
        }

        public static void Trigger(EventHook hook)
        {
            Trigger(hook, new EmptyEventArgs());
        }

        public static void Trigger(string name, GameObject target)
        {
            Trigger(new EventHook(name, target));
        }
    }
}
