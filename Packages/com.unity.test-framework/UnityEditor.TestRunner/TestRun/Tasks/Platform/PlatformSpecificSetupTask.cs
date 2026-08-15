using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform
{
    internal class PlatformSpecificSetupTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.executionSettings.targetPlatform == null)
            {
                throw new Exception($"{nameof(PlatformSpecificSetupTask)} can only run on a task with a target platform.");
            }

            testJobData.PlatformSpecificSetup =
                new PlatformSpecificSetup(testJobData.executionSettings.targetPlatform.Value);
            testJobData.PlatformSpecificSetup.Setup();
            yield break;
        }
    }
}
