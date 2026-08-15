using System;
using System.Collections;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CleanupTestControllerTask : TestTaskBase
    {
        public CleanupTestControllerTask()
        {
            RunOnCancel = true;
            RunOnError = ErrorRunMode.RunAlways;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.PlaymodeTestsController == null)
            {
                yield break;
            }
            
            testJobData.PlaymodeTestsController.Cleanup();
        }
    }
}
