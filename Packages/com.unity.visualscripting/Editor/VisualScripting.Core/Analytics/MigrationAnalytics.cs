using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.VisualScripting.Analytics
{
    internal static class MigrationAnalytics
    {
        private const int MaxEventsPerHour = 120;
        private const int MaxNumberOfElements = 1000;
        private const string VendorKey = "unity.visualscripting";
        private const string EventName = "VScriptMigration";
#if !UNITY_2023_2_OR_NEWER
        private static bool _isRegistered = false;
#endif

#if UNITY_2023_2_OR_NEWER
        internal static void Send(Data data)
        {
            if (!EditorAnalytics.enabled)
                return;

            EditorAnalytics.SendAnalytic(new MigrationAnalytic(data));
        }
#else
        internal static void Send(Data data)
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!RegisterEvent())
                return;

            EditorAnalytics.SendEventWithLimit(EventName, data);
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

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: EventName, vendorKey: VendorKey, maxEventsPerHour:MaxEventsPerHour, maxNumberOfElements:MaxNumberOfElements)]
        class MigrationAnalytic : IAnalytic
        {
            private Data data;
            public MigrationAnalytic(Data data)
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
        internal class Data : IAnalytic.IData
#else
        [Serializable]
        internal class Data
#endif
        {
            [SerializeField]
            internal MigrationStepAnalyticsData total;
            [SerializeField]
            internal List<MigrationStepAnalyticsData> steps;
        }

        [Serializable]
        internal class MigrationStepAnalyticsData
        {
            [SerializeField]
            internal string pluginId;
            [SerializeField]
            internal string from;
            [SerializeField]
            internal string to;
            [SerializeField]
            internal bool success;
            [SerializeField]
            internal string exception;
        }
    }
}
