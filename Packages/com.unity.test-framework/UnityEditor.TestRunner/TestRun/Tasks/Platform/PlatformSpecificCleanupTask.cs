using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform
{
    internal class PlatformSpecificCleanupTask : TestTaskBase
    {
        public PlatformSpecificCleanupTask()
        {
            RunOnError = ErrorRunMode.RunAlways;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.PlatformSpecificSetup?.CleanUp();
            yield break;
        }
    }
}
