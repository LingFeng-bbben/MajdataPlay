using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    class FilterTestTreeTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.filteredTests ??= new List<ITest>();

            TestFiltering.GetMatchingTests(
                testJobData.testTree,
                testJobData.testFilter,
                ref testJobData.filteredTests,
                testJobData.TargetRuntimePlatform);

            yield return null;
        }
    }
}
