using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    static class TestFiltering
    {
        internal static void GetMatchingTests(ITest tests, ITestFilter filter, ref List<ITest> resultList, RuntimePlatform testTargetPlatform)
            {
                foreach (var test in tests.Tests)
                {
                    if (IsTestEnabledOnPlatform(test, testTargetPlatform))
                    {
                        if (test.IsSuite)
                        {
                            GetMatchingTests(test, filter, ref resultList, testTargetPlatform);
                        }
                        else
                        {
                            if (filter.Pass(test))
                                resultList.Add(test);
                        }
                    }
                }
            }

        internal static bool IsTestEnabledOnPlatform(ITest test, RuntimePlatform testTargetPlatform)
        {
            if (test.Method == null)
            {
                return true;
            }

            var attributesFromMethods = test.Method.GetCustomAttributes<UnityPlatformAttribute>(true);
            var attributesFromTypes = test.Method.TypeInfo.GetCustomAttributes<UnityPlatformAttribute>(true);

            if (attributesFromMethods.Length == 0 && attributesFromTypes.Length == 0)
            {
                return true;
            }

            if (!attributesFromMethods.All(a => a.IsPlatformSupported(testTargetPlatform)))
            {
                return false;
            }

            if (!attributesFromTypes.All(a => a.IsPlatformSupported(testTargetPlatform)))
            {
                return false;
            }

            return true;
        }

    }
}
