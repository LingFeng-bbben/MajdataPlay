using System;
using UnityEditor;
using UnityEngine.Analytics;

namespace Unity.VisualScripting
{
    public sealed class UsageAnalytics
    {
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 1000;
        const string k_VendorKey = "unity.bolt";
        const string k_EventName = "BoltUsage";
#if !UNITY_2023_2_OR_NEWER
        static bool isRegistered = false;
#endif

        internal static void CollectAndSendForType<T>(string[] paths)
        {
            if (!EditorAnalytics.enabled || !VSUsageUtility.isVisualScriptingUsed)
                return;

            foreach (string path in paths)
            {
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == typeof(T))
                {
                    CollectAndSend();
                    return;
                }
            }
        }

#if UNITY_2023_2_OR_NEWER
        public static void CollectAndSend()
        {
            if (!EditorAnalytics.enabled || !VSUsageUtility.isVisualScriptingUsed)
                return;

            EditorAnalytics.SendAnalytic(new UsageAnalytic(CollectData()));
        }
#else
        public static void CollectAndSend()
        {
            if (!EditorAnalytics.enabled || !VSUsageUtility.isVisualScriptingUsed)
                return;

            if (!RegisterEvent())
                return;

            var data = CollectData();

            EditorAnalytics.SendEventWithLimit(k_EventName, data);
        }

        private static bool RegisterEvent()
        {
            if (!isRegistered)
            {
                var result = EditorAnalytics.RegisterEventWithLimit(k_EventName, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey);
                if (result == UnityEngine.Analytics.AnalyticsResult.Ok)
                {
                    isRegistered = true;
                }
            }

            return isRegistered;
        }
#endif

        private static UsageAnalyticsData CollectData()
        {
            var data = new UsageAnalyticsData
            {
                productVersion = BoltProduct.instance.version.ToString(),
            };

            return data;
        }

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: k_EventName, vendorKey: k_VendorKey, maxEventsPerHour:k_MaxEventsPerHour, maxNumberOfElements:k_MaxNumberOfElements)]
        class UsageAnalytic : IAnalytic
        {
            private UsageAnalyticsData data;
            public UsageAnalytic(UsageAnalyticsData data)
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
        private struct UsageAnalyticsData: IAnalytic.IData
#else
        private struct UsageAnalyticsData
#endif
        {
            public string productVersion;
        }
    }
}
