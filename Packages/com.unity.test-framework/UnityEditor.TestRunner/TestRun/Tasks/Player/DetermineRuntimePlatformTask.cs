using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player
{
    internal class DetermineRuntimePlatformTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            var targetPlatform = testJobData.executionSettings.targetPlatform;
            if (targetPlatform.HasValue)
            {
                testJobData.TargetRuntimePlatform = BuildTargetConverter.TryConvertToRuntimePlatform(targetPlatform.Value);
            }

            yield return null;
        }
    }
}
