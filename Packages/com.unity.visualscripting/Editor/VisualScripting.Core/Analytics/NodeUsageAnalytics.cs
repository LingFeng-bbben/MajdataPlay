using System;
using System.Collections.Generic;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.VisualScripting.Analytics
{
    internal static class NodeUsageAnalytics
    {
        private const int MaxEventsPerHour = 120;
        private const int MaxNumberOfElements = 1000;
        private const string VendorKey = "unity.visualscripting";
        private const string EventName = "VScriptNodeUsage";
#if !UNITY_2023_2_OR_NEWER
        private static bool _isRegistered = false;
#endif

        private const int NodeUseLimitBeforeSend = 100;
        private static bool _interruptEventsRegistered = false;
        private static Data _collectedData = null;

        private static readonly HashSet<string> AllowedNamespaces = new HashSet<string>()
        {
            "System",
            "Mono",
            "Unity", // Includes UnityEngine, UnityEngineInternal, UnityEditor, UnityEditorInternal
        };

        internal static void NodeAdded(AnalyticsIdentifier aid)
        {
            EnsureCollectedDataInitialized();
            EnsureInterruptEventsRegistered();

            var node = GetNodeStringFromAnalyticsIdentifier(aid);

            _collectedData.nodeUsageCount.Add(new NodeCount() { node = node, count = 1 });

            if (_collectedData.nodeUsageCount.Count >= NodeUseLimitBeforeSend)
                Send();
        }

        internal static void NodeRemoved(AnalyticsIdentifier aid)
        {
            EnsureCollectedDataInitialized();
            EnsureInterruptEventsRegistered();

            var node = GetNodeStringFromAnalyticsIdentifier(aid);

            _collectedData.nodeUsageCount.Add(new NodeCount() { node = node, count = -1 });

            if (_collectedData.nodeUsageCount.Count >= NodeUseLimitBeforeSend)
                Send();
        }

        private static string GetNodeStringFromAnalyticsIdentifier(AnalyticsIdentifier aid)
        {
            foreach (var allowedNamespace in AllowedNamespaces)
            {
                if (aid.Namespace.StartsWith(allowedNamespace))
                    return aid.Identifier;
            }

            return aid.Hashcode.ToString();
        }

#if UNITY_2023_2_OR_NEWER
        private static void Send()
        {
            if (!EditorAnalytics.enabled)
                return;

            EditorAnalytics.SendAnalytic(new NodeUsageAnalytic(_collectedData));

            ResetCollectedData();
        }
#else
        private static void Send()
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!RegisterEvent())
                return;

            EditorAnalytics.SendEventWithLimit(EventName, _collectedData);

            ResetCollectedData();
        }

        private static bool RegisterEvent()
        {
            if (!_isRegistered)
            {
                var result = EditorAnalytics.RegisterEventWithLimit(EventName, MaxEventsPerHour, MaxNumberOfElements, VendorKey);
                if (result == UnityEngine.Analytics.AnalyticsResult.Ok)
                {
                    _isRegistered = true;
                }
            }

            return _isRegistered;
        }
#endif

        private static void EnsureInterruptEventsRegistered()
        {
            if (_interruptEventsRegistered) return;

            EditorApplication.quitting += Send;
            AssemblyReloadEvents.beforeAssemblyReload += Send;
            _interruptEventsRegistered = true;
        }

        private static void EnsureCollectedDataInitialized()
        {
            if (_collectedData != null) return;
            ResetCollectedData();
        }

        private static void ResetCollectedData()
        {
            _collectedData = new Data()
            {
                nodeUsageCount = new List<NodeCount>(),
            };
        }

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: EventName, vendorKey: VendorKey, maxEventsPerHour:MaxEventsPerHour, maxNumberOfElements:MaxNumberOfElements)]
        class NodeUsageAnalytic : IAnalytic
        {
            private Data data;

            public NodeUsageAnalytic(Data data)
            {
                this.data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                error = null;
                data = this.data;
                return data != null;
            }
        }

        [Serializable]
        private class Data : IAnalytic.IData
#else
        [Serializable]
        private class Data
#endif
        {
            [SerializeField]
            internal List<NodeCount> nodeUsageCount;
        }

        [Serializable]
        private struct NodeCount
        {
            [SerializeField]
            internal string node;
            [SerializeField]
            internal int count;
        }
    }
}
