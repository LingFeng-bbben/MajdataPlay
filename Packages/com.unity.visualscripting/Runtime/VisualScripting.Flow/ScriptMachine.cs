using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting
{
    [AddComponentMenu("Visual Scripting/Script Machine")]
    [RequireComponent(typeof(Variables))]
    [DisableAnnotation]
    [RenamedFrom("Bolt.FlowMachine")]
    [RenamedFrom("Unity.VisualScripting.FlowMachine")]
    [VisualScriptingHelpURL(typeof(ScriptMachine))]
    public sealed class ScriptMachine : EventMachine<FlowGraph, ScriptGraphAsset>
    {
        Graph m_SubscribedGraphs;

        public override FlowGraph DefaultGraph()
        {
            return FlowGraph.WithStartUpdate();
        }

        protected override void OnEnable()
        {
            if (hasGraph)
            {
                graph.StartListening(reference);
                SubscribeToGraphConnectionEvents();
            }

            nest.beforeGraphChange += UnsubscribeFromGraphConnectionEvents;
            nest.afterGraphChange += SubscribeToGraphConnectionEvents;

            base.OnEnable();
        }

        protected override void OnInstantiateWhileEnabled()
        {
            if (hasGraph)
            {
                graph.StartListening(reference);
                SubscribeToGraphConnectionEvents();
            }

            base.OnInstantiateWhileEnabled();
        }

        protected override void OnUninstantiateWhileEnabled()
        {
            base.OnUninstantiateWhileEnabled();


            if (hasGraph)
            {
                graph.StopListening(reference);
                UnsubscribeFromGraphConnectionEvents();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (hasGraph)
            {
                UnsubscribeFromGraphConnectionEvents();
            }

            nest.beforeGraphChange -= UnsubscribeFromGraphConnectionEvents;
            nest.afterGraphChange -= SubscribeToGraphConnectionEvents;
        }

        void SubscribeToGraphConnectionEvents()
        {
            if (!hasGraph || m_SubscribedGraphs == graph)
            {
                return;
            }

            // Subscribe to control and value connection additions/removals so we can start/stop listening
            // when a connection to a listener unit is created or removed at runtime.
            graph.controlConnections.ItemAdded += OnControlConnectionAdded;
            graph.valueConnections.ItemAdded += OnValueConnectionAdded;

            graph.controlConnections.ItemRemoved += OnControlConnectionRemoved;
            graph.valueConnections.ItemRemoved += OnValueConnectionRemoved;

            m_SubscribedGraphs = graph;
        }

        void UnsubscribeFromGraphConnectionEvents()
        {
            if (!hasGraph || m_SubscribedGraphs != graph)
            {
                return;
            }

            graph.controlConnections.ItemAdded -= OnControlConnectionAdded;
            graph.valueConnections.ItemAdded -= OnValueConnectionAdded;

            graph.controlConnections.ItemRemoved -= OnControlConnectionRemoved;
            graph.valueConnections.ItemRemoved -= OnValueConnectionRemoved;

            m_SubscribedGraphs = null;
        }

        void OnControlConnectionAdded(ControlConnection connection)
        {
            // Only handle runtime changes, not deserialization rebuilds
            if (graph.isAddingElementsPostDeserialization || !Application.isPlaying)
            {
                return;
            }

            if (connection.destination?.unit is IGraphEventListener || connection.source?.unit is IGraphEventListener)
            {
                // Ensure the graph begins listening for events that the newly-connected listener needs.
                graph.StartListening(reference);
            }
        }

        void OnValueConnectionAdded(ValueConnection connection)
        {
            // Only handle runtime changes, not deserialization rebuilds
            if (graph.isAddingElementsPostDeserialization || !Application.isPlaying)
            {
                return;
            }

            var unit = connection.destination?.unit ?? connection.source?.unit;

            if (unit is IGraphEventListener)
            {
                // Ensure the graph begins listening for events that the newly-connected listener needs.
                graph.StartListening(reference);
            }
        }

        void OnControlConnectionRemoved(ControlConnection connection)
        {
            // Only handle runtime changes, not deserialization rebuilds
            if (graph.isAddingElementsPostDeserialization || !Application.isPlaying)
            {
                return;
            }

            // If there are no remaining connections involving listener units, stop listening.
            if (!GraphHasAnyListenerConnections())
            {
                graph.StopListening(reference);
            }
        }

        void OnValueConnectionRemoved(ValueConnection connection)
        {
            // Only handle runtime changes, not deserialization rebuilds
            if (graph.isAddingElementsPostDeserialization || !Application.isPlaying)
            {
                return;
            }

            if (!GraphHasAnyListenerConnections())
            {
                graph.StopListening(reference);
            }
        }

        bool GraphHasAnyListenerConnections()
        {
            if (!hasGraph)
            {
                return false;
            }

            foreach (var c in graph.controlConnections)
            {
                if (c.destination?.unit is IGraphEventListener || c.source?.unit is IGraphEventListener)
                {
                    return true;
                }
            }

            foreach (var c in graph.valueConnections)
            {
                if (c.destination?.unit is IGraphEventListener || c.source?.unit is IGraphEventListener)
                {
                    return true;
                }
            }

            return false;
        }

        [ContextMenu("Show Data...")]
        protected override void ShowData()
        {
            base.ShowData();
        }
    }
}
