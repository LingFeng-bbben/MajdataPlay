using System;
using System.Collections.Generic;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class UnityWorkItemDataHolder
    {
        public static List<string> alreadyStartedTests = new List<string>();
        public static List<string> alreadyExecutedTests;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticsOnLoad()
        {
            alreadyStartedTests = new List<string>();
            alreadyExecutedTests = null;
        }
#endif
    }
}
