using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform
{
    internal class PlatformSpecificSuccessfulLaunchTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if ((testJobData.GetCurrentBuildOptions() & BuildOptions.AutoRunPlayer) != 0)
            {
                testJobData.PlatformSpecificSetup.PostSuccessfulLaunchAction();
            }

            yield return null;
        }
    }
}
