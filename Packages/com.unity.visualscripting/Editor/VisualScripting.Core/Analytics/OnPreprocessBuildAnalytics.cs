using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.VisualScripting.Analytics
{
    internal static class OnPreprocessBuildAnalytics
    {
        private const int MaxEventsPerHour = 120;
        private const int MaxNumberOfElements = 1000;
        private const string VendorKey = "unity.visualscripting";
        private const string EventName = "VScriptOnPreprocessBuild";
#if !UNITY_2023_2_OR_NEWER
        private static bool _isRegistered = false;
#endif

#if UNITY_2023_2_OR_NEWER
        internal static void Send(Data data)
        {
            if (!EditorAnalytics.enabled)
                return;
            
            EditorAnalytics.SendAnalytic(new OnPreprocessBuildAnalytic(data));
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
        class OnPreprocessBuildAnalytic : IAnalytic
        {
            private Data data;

            public OnPreprocessBuildAnalytic(Data data)
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
        internal struct Data : IAnalytic.IData
#else
        [Serializable]
        internal struct Data
#endif
        {
            [SerializeField]
            internal string guid;
            [SerializeField]
            internal BuildTarget buildTarget;
            [SerializeField]
            internal BuildTargetGroup buildTargetGroup;
        }
    }
}
