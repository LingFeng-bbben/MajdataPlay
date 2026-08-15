using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform
{
    internal class PlatformSpecificPostBuildTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if ((testJobData.GetCurrentBuildOptions() & BuildOptions.AutoRunPlayer) != 0)
            {
                testJobData.PlatformSpecificSetup.PostBuildAction();
            }

            yield return null;
        }
    }
}
